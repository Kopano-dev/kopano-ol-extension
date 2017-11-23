#include "EASAccount.h"

struct Account
{
public:
	wstring profileName;
	wstring accountName;
	wstring displayName;
	wstring email;
	wstring server;
	wstring username;
	wstring password;
	wstring dataFolder;
private:
	wstring path;
	int initializedMAPI = 0;
	IProfAdmin *lpProfAdmin = nullptr;
	IMsgServiceAdmin *lpServiceAdmin = nullptr;
	IMsgServiceAdmin2* lpServiceAdmin2 = nullptr;
	IOlkAccountManager *lpAccountManager = nullptr;
	MAPIUID service;
	vector<byte> entryId;
	DWORD accountId;
	HKEY hKeyAccounts = nullptr;
	HKEY hKeyNewAccount = nullptr;

public:
	Account()
	{
		// Initialize the mapi session
		MAPIINIT_0	MAPIINIT = { 0, 0 };
		CHECK_H(MAPIInitialize(&MAPIINIT), "MAPIInitialize");
		++initializedMAPI;
	}

	~Account()
	{
		if (lpServiceAdmin2) lpServiceAdmin2->Release();
		if (lpServiceAdmin) lpServiceAdmin->Release();
		if (lpProfAdmin) lpProfAdmin->Release();
		if (lpAccountManager) lpAccountManager->Release();
		if (hKeyAccounts) RegCloseKey(hKeyAccounts);
		if (hKeyNewAccount) RegCloseKey(hKeyNewAccount);

		if (--initializedMAPI == 0)
		{
			MAPIUninitialize();
		}
		else if (initializedMAPI < 0)
		{
			exit(1);
		}
	}

	void Create()
	{
		CheckInit();
		
		// Determine the .ost path
		if (dataFolder.empty())
		{
			wchar_t szPath[MAX_PATH];
			CHECK_H(SHGetFolderPath(nullptr, CSIDL_LOCAL_APPDATA, nullptr, 0, szPath), "GetAppData");
			dataFolder = wstring(szPath) + L"\\Microsoft\\Outlook\\";
		}
		// Somehow it only works if there's a number in between parentheses
		path = dataFolder + email + L" - " + profileName + L"(1).ost";

		// Set up the account
		OpenProfileAdmin();
		CreateMessageService();
		GetEntryId();
		CreateAccount();
		CommitAccountKey();
		PatchMessageStore();
	}
private:
	void CheckInit()
	{
		#define DoCheckInit(field) do{ if (field.empty()) throw exception("Field " #field " not initialised"); } while(0)
		DoCheckInit(profileName);
		DoCheckInit(accountName);
		DoCheckInit(displayName);
		DoCheckInit(email);
		DoCheckInit(server);
		DoCheckInit(username);
		#undef DoCheckInit
	}

	static void CHECK_H(HRESULT hr, const char *ident)
	{
		if (FAILED(hr))
			throw CustomException(hr, ident, _com_error(hr).ErrorMessage());
	}

	static void CHECK_L(LSTATUS status, const char *ident)
	{
		if (status != ERROR_SUCCESS)
			throw CustomException(status, ident);
	}

	static string WideToString(const wstring &s)
	{
		std::string result;
		for (auto i = s.begin(); i != s.end(); ++i)
			result += (char)*i;
		return result;
	}

	void OpenProfileAdmin()
	{
		if (!lpServiceAdmin2)
		{
			// Get the profile admin 
			CHECK_H(MAPIAdminProfiles(0, &lpProfAdmin), "MAPIAdminProfiles");
			CHECK_H(lpProfAdmin->AdminServices((LPTSTR)WideToString(profileName).c_str(), nullptr, NULL, 0, &lpServiceAdmin), "AdminServices");
			CHECK_H(lpServiceAdmin->QueryInterface(IID_IMsgServiceAdmin2, (LPVOID*)&lpServiceAdmin2), "AdminServices2");
		}
	}

	void CreateMessageService()
	{
		CHECK_H(lpServiceAdmin2->CreateMsgServiceEx((LPTSTR)"EAS", (LPTSTR)displayName.c_str(), 0, 0, &service), "CreateMsgServiceEx");

		// Delete any existing ost
		DeleteFile(path.c_str());

		// Configure the service
		SPropValue msprops[6];
		msprops[0].ulPropTag = PR_PST_CONFIG_FLAGS;
		msprops[0].Value.l = 2;

		msprops[1].ulPropTag = PR_PROFILE_OFFLINE_STORE_PATH_W;
		msprops[1].Value.lpszW = (LPWSTR)path.c_str();

		msprops[2].ulPropTag = PR_DISPLAY_NAME_W;
		msprops[2].Value.lpszW = (LPWSTR)displayName.c_str();

		msprops[3].ulPropTag = PR_PROFILE_SECURE_MAILBOX;
		vector<byte> encPassword = EncryptPassword(password, L"S010267f0");
		msprops[3].Value.bin.cb = (ULONG)encPassword.size();
		msprops[3].Value.bin.lpb = &encPassword[0];

		msprops[4].ulPropTag = PR_RESOURCE_FLAGS;
		msprops[4].Value.l = SERVICE_NO_PRIMARY_IDENTITY | SERVICE_CREATE_WITH_STORE;

		msprops[5].ulPropTag = 0x67060003;
		msprops[5].Value.l = 4;

		CHECK_H(lpServiceAdmin2->ConfigureMsgService(&service, 0, SERVICE_UI_ALLOWED, 6, msprops), "ConfigureMSGService");
	}

	void GetEntryId()
	{
		IProfSect *profSect = nullptr;
		LPSPropValue vals = nullptr;
		try
		{
			// Open the profile section
			CHECK_H(lpServiceAdmin2->OpenProfileSection(const_cast<LPMAPIUID>(&service), NULL, MAPI_FORCE_ACCESS, &profSect), "ProfSect");

			// Get the entry id
			SizedSPropTagArray(1, props);
			props.cValues = 1;
			props.aulPropTag[0] = PR_ENTRYID;

			ULONG count = 0;
			CHECK_H(profSect->GetProps((LPSPropTagArray)&props, 0, &count, &vals), "GetProps");

			entryId.resize(vals[0].Value.bin.cb);
			entryId.assign(vals[0].Value.bin.lpb, vals[0].Value.bin.lpb + entryId.size());

			// Clean up
			profSect->Release();
			MAPIFreeBuffer(vals);
		}
		catch (...)
		{
			if (profSect) profSect->Release();
			MAPIFreeBuffer(vals);
			throw;
		}
	}

	void AllocateAccountKey()
	{
		wchar_t keyPath[MAX_PATH];

		// Open the accounts key
		swprintf_s(keyPath, ARRAYSIZE(keyPath), KEY_ACCOUNTS, 16, profileName.c_str());
		CHECK_L(RegOpenKey(HKEY_CURRENT_USER, keyPath, &hKeyAccounts), "OpenAccountsKey");

		// Get the NextAccountID value
		DWORD size = sizeof(accountId);
		CHECK_L(RegQueryValueEx(hKeyAccounts, VALUE_NEXT_ACCOUNT_ID, nullptr, nullptr, (LPBYTE)&accountId, &size), "GetNextAccountId");
		
		// Create the subkey
		swprintf_s(keyPath, ARRAYSIZE(keyPath), L"%.8X", accountId);
		CHECK_L(RegCreateKey(hKeyAccounts, keyPath, &hKeyNewAccount), "CreateAccountKey");
	}

	void WriteAccountKey(const wstring &name, const wstring &value)
	{
		CHECK_L(RegSetValueEx(hKeyNewAccount, name.c_str(), 0, REG_SZ, (LPBYTE)value.data(), (DWORD)value.size() * 2), "WriteAccountKey");
	}

	void WriteAccountKey(const wstring &name, const void *value, size_t size)
	{
		CHECK_L(RegSetValueEx(hKeyNewAccount, name.c_str(), 0, REG_BINARY, (LPBYTE)value, (DWORD)size), "WriteAccountKey");
	}

	void WriteAccountKey(const wstring &name, DWORD value)
	{
		CHECK_L(RegSetValueEx(hKeyNewAccount, name.c_str(), 0, REG_DWORD, (LPBYTE)&value, sizeof(value)), "WriteAccountKey");
	}

	class OlkHelper : public IOlkAccountHelper
	{
	private:
		long refCount;
		const Account &account;
		IUnknown* unkSession;
	public:
		OlkHelper(const Account &account, LPMAPISESSION session)
		:
		refCount(0),
		account(account),
		unkSession(nullptr)
		{
			CHECK_H(session->QueryInterface(IID_IUnknown, (LPVOID*)&unkSession), "Session::QueryInterface");
		}

		virtual HRESULT STDMETHODCALLTYPE QueryInterface(REFIID riid, _COM_Outptr_ void __RPC_FAR *__RPC_FAR *ppvObject) override
		{
			return S_OK;
		}

		virtual ULONG STDMETHODCALLTYPE AddRef(void) override
		{
			++refCount;
			return refCount;
		}

		virtual ULONG STDMETHODCALLTYPE Release(void) override
		{
			--refCount;
			return refCount;
		}

		virtual STDMETHODIMP PlaceHolder1(LPVOID) override
		{
			return S_OK;
		}

		virtual STDMETHODIMP GetIdentity(LPWSTR pwszIdentity, DWORD * pcch) override
		{
			if (!pcch)
				return E_INVALIDARG;

			HRESULT hRes = S_OK;

			if (account.profileName.size() > *pcch)
			{
				*pcch = (DWORD)account.profileName.size();
				return E_OUTOFMEMORY;
			}

			hRes = StringCchCopyW(pwszIdentity, *pcch, account.profileName.c_str());

			*pcch = (DWORD)account.profileName.size();

			return hRes;
		}

		virtual STDMETHODIMP GetMapiSession(LPUNKNOWN * ppmsess) override
		{
			CHECK_H(unkSession->QueryInterface(IID_IMAPISession, (LPVOID*)ppmsess), "GetMapiSession");
			return S_OK;
		}

		virtual STDMETHODIMP HandsOffSession() override
		{
			return S_OK;
		}

	};

	void CreateAccount()
	{
		AllocateAccountKey();

		// Write the values
		WriteAccountKey(L"Account Name", accountName);
		WriteAccountKey(L"Display Name", displayName);
		WriteAccountKey(L"EAS Server URL", server);
		WriteAccountKey(L"EAS User", username);
		WriteAccountKey(L"Email", email);
		WriteAccountKey(L"clsid", L"{ED475415-B0D6-11D2-8C3B-00104B2A6676}");

		vector<byte> encrypted = EncryptPassword(password, L"EAS Password");
		WriteAccountKey(L"EAS Password", &encrypted[0], encrypted.size());

		WriteAccountKey(L"Service UID", &service, sizeof(service));

		// Delivery Store EntryID
		WriteAccountKey(L"EAS Store EID", &entryId[0], entryId.size());

		// Mini uid
		GUID miniUid;
		CHECK_H(CoCreateGuid(&miniUid), "miniUid");
		WriteAccountKey(L"Mini UID", miniUid.Data1);
	}

	void AppendAccountId(const wchar_t *value)
	{
		byte buffer[4096];
		DWORD bufferSize = sizeof(buffer);
		CHECK_L(RegQueryValueEx(hKeyAccounts, value, nullptr, nullptr, buffer, &bufferSize), "QueryAccountId");

		if (bufferSize >= sizeof(buffer) - sizeof(DWORD))
			throw exception("AppendAccountId buffer too small");

		// Append the account id
		*(DWORD *)(&buffer[bufferSize]) = accountId;
		CHECK_L(RegSetValueEx(hKeyAccounts, value, 0, REG_BINARY, buffer, bufferSize + sizeof(DWORD)), "AppendAccountId");
	}

	void CommitAccountKey()
	{
		// Increment the account id
		DWORD nextAccountId = accountId + 1;
		CHECK_L(RegSetValueEx(hKeyAccounts, VALUE_NEXT_ACCOUNT_ID, 0, REG_DWORD, (LPBYTE)&nextAccountId, sizeof(nextAccountId)), "CommitAccountKey");

		// Add the account to the mail, store and addressbook entries
		AppendAccountId(KEY_OLKMAIL);
		AppendAccountId(KEY_OLKADDRESSBOOK);
		AppendAccountId(KEY_OLKSTORE);
	}

	std::vector<byte> EncryptPassword(const std::wstring &password, const wchar_t *descriptor)
	{
		const byte FLAG_PROTECT_DATA = 2;

		DATA_BLOB plainTextBlob;
		DATA_BLOB cipherTextBlob;

		int bytesSize = (int)(password.size() * sizeof(wchar_t));
		plainTextBlob.pbData = (BYTE*)password.data();
		plainTextBlob.cbData = bytesSize + 2;

		if (!CryptProtectData(&plainTextBlob, descriptor, nullptr, nullptr, nullptr,
			CRYPTPROTECT_UI_FORBIDDEN, &cipherTextBlob))
		{
			throw std::exception("Encryption failed.");
		}

		std::vector<byte> cipherText(cipherTextBlob.cbData + 1);
		memcpy(&cipherText[1], cipherTextBlob.pbData, cipherTextBlob.cbData);
		cipherText[0] = FLAG_PROTECT_DATA;
		LocalFree(cipherTextBlob.pbData);
		return cipherText;
	}

	void PatchMessageStore()
	{
		LPMAPISESSION session = nullptr;
		LPMDB msgStore = nullptr;
		IOlkAccount *account = nullptr;

		try
		{
			// Delete existing store
			DeleteFile(path.c_str());

			// Logon
			CHECK_H(MAPILogonEx(0, (LPTSTR)WideToString(profileName).c_str(), NULL, 0, &session), "MAPILogonEx");

#if 0
			OlkHelper helper(*this, session);
			CHECK_H(CoCreateInstance(CLSID_OlkAccountManager,
				NULL,
				CLSCTX_INPROC_SERVER,
				IID_IOlkAccountManager,
				(LPVOID*)&lpAccountManager), "IOLKAccountManager");
			CHECK_H(lpAccountManager->Init(&helper, 0), "IOLKAccountManager::Init");
			DWORD count;
			DWORD *order;
			CHECK_H(lpAccountManager->GetOrder(&CLSID_OlkMail, &count, &order), "IOLKAccountManager::GetOrder");
			_RPT1(_CRT_WARN, "ACCOUNTS: %d\n", count);

			// Create the registry entry for the account
			//CHECK_H(lpAccountManager->EnumerateAccounts(CLSID_OlkMail, ))
			ACCT_VARIANT var;
			var.dwType = PT_LONG;
			var.Val.dw = accountId;
			CHECK_H(lpAccountManager->FindAccount(PROP_ACCT_ID, &var, &account), "IOLKAccountManager::FindAccount");
			CHECK_H(lpAccountManager->SaveChanges(accountId, 0), "SaveChanges");
#endif

			// Delete existing store
			DeleteFile(path.c_str());

			// Open the msg store to finalise creation
			CHECK_H(session->OpenMsgStore(0, (ULONG)entryId.size(), (LPENTRYID)&entryId[0], nullptr,
				MDB_NO_DIALOG | MDB_WRITE | MAPI_DEFERRED_ERRORS, &msgStore), "OpenMsgStore");
		}
		catch (...)
		{
			if (account)
				account->Release();
			if (msgStore) 
				msgStore->Release();
			if (session)
			{
				session->Logoff(0, 0, 0);
				session->Release();
			}
			initializedMAPI = -1;
			throw;
		}

		// Clean up
		if (account)
			account->Release();
		if (msgStore) 
			msgStore->Release();
		session->Logoff(0, 0, 0);
		session->Release();
	}

};

int __cdecl wmain(int, wchar_t  **)
{
	// Parse the command line
	// Usage:
	Account account;

	// Main
	try
	{
		// Create the account
		account.Create();
	}
	catch (const CustomException &e)
	{
		if (!strcmp(e.what(), "AdminServices") && e.status == 0x80040111)
		{
			_RPTW1(_CRT_WARN, L"Profile does not exist: %ls\n", account.profileName.c_str());
			fwprintf(stderr, L"Profile does not exist: %s\n", account.profileName.c_str());
		}
		else
		{
			_RPTW1(_CRT_WARN, L"Exception: %ls\n", e.message.c_str());
			fwprintf(stderr, L"Exception: %s\n", e.message.c_str());
		}
		return 1;
	}
	catch(const exception &e)
	{
		_RPT1(_CRT_WARN, "Exception: %s\n", e.what());
		fprintf(stderr, "Exception: %s\n", e.what());
		return 2;
	}

	return 0;
}
