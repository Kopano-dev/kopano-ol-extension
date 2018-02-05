#include "EASAccount.h"

static const wstring R_ACCOUNT_NAME = L"Account Name";
static const wstring R_DISPLAY_NAME = L"Display Name";
static const wstring R_SERVER_URL = L"EAS Server URL";
static const wstring R_USERNAME = L"EAS User";
static const wstring R_EMAIL = L"Email";
static const wstring R_EMAIL_ORIGINAL = L"KOE Share For";
static const wstring R_PASSWORD = L"EAS Password";

struct Account
{
public:
	wstring profileName;
	wstring outlookVersion;
	wstring accountName;
	wstring displayName;
	wstring email;
	wstring emailOriginal;
	wstring server;
	wstring username;
	wstring password;
	vector<byte> encryptedPassword;
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
		DeterminePath();

		// Set up the account
		OpenProfileAdmin();
		EncryptPassword();
		CreateMessageService();
		GetEntryId();
		CreateAccount();
		CommitAccountKey();
		PatchMessageStore();
	}

	void LoadFromAccountId(const wstring &accountId)
	{
		OpenAccountKey(accountId);

		try
		{ 
			accountName = RegReadAccountKey(R_ACCOUNT_NAME);
			displayName = RegReadAccountKey(R_DISPLAY_NAME);
			email = RegReadAccountKey(R_EMAIL);
			server = RegReadAccountKey(R_SERVER_URL);
			username = RegReadAccountKey(R_USERNAME);
			encryptedPassword = RegReadAccountKeyBinary(R_PASSWORD);
		}
		// Clean up
		catch (...)
		{
			if (hKeyNewAccount)
			{
				RegCloseKey(hKeyNewAccount);
				hKeyNewAccount = nullptr;
			}
			throw;
		}
		if (hKeyNewAccount)
		{
			RegCloseKey(hKeyNewAccount);
			hKeyNewAccount = nullptr;
		}
	}
private:
	void DeterminePath()
	{
		// Determine the .ost path
		if (dataFolder.empty())
		{
			wchar_t szPath[MAX_PATH];
			CHECK_H(SHGetFolderPath(nullptr, CSIDL_LOCAL_APPDATA, nullptr, 0, szPath), "GetAppData");
			dataFolder = wstring(szPath) + L"\\Microsoft\\Outlook\\";
		}
		// Somehow it only works if there's a number in between parentheses
		path = dataFolder + email + L" - " + profileName + L"(1).ost";
	}

	void EncryptPassword()
	{
		// TODO: handle the case password is not set in registry
		if (encryptedPassword.empty())
			encryptedPassword = EncryptPassword(password, L"EAS Password");
	}

	void CheckInit()
	{
		#define DoCheckInit(field) do{ if (field.empty()) throw exception("Field " #field " not initialised"); } while(0)
		DoCheckInit(profileName);
		DoCheckInit(outlookVersion);
		DoCheckInit(accountName);
		DoCheckInit(displayName);
		DoCheckInit(email);
		DoCheckInit(server);
		DoCheckInit(username);
		if (encryptedPassword.empty())
			DoCheckInit(password);
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
		msprops[3].Value.bin.cb = (ULONG)encryptedPassword.size();
		msprops[3].Value.bin.lpb = &encryptedPassword[0];

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

	void OpenAccountsKey()
	{
		if (hKeyAccounts != nullptr)
			return;

		wchar_t keyPath[MAX_PATH];

		// Open the accounts key
		swprintf_s(keyPath, ARRAYSIZE(keyPath), KEY_ACCOUNTS, outlookVersion.c_str(), profileName.c_str());
		CHECK_L(RegOpenKey(HKEY_CURRENT_USER, keyPath, &hKeyAccounts), "OpenAccountsKey");
	}

	void OpenAccountKey(const wstring &accountId)
	{
		OpenAccountsKey();

		// Open the subkey
		CHECK_L(RegOpenKey(hKeyAccounts, accountId.c_str(), &hKeyNewAccount), "OpenAccountKey");
	}

	void AllocateAccountKey()
	{
		OpenAccountsKey();

		wchar_t keyPath[MAX_PATH];

		// Get the NextAccountID value
		DWORD size = sizeof(accountId);
		CHECK_L(RegQueryValueEx(hKeyAccounts, VALUE_NEXT_ACCOUNT_ID, nullptr, nullptr, (LPBYTE)&accountId, &size), "GetNextAccountId");
		
		// Create the subkey
		swprintf_s(keyPath, ARRAYSIZE(keyPath), L"%.8X", accountId);
		CHECK_L(RegCreateKey(hKeyAccounts, keyPath, &hKeyNewAccount), "CreateAccountKey");
	}

	wstring RegReadAccountKey(const wstring &name)
	{
		wchar_t buffer[4096];
		DWORD size = sizeof(buffer);
		CHECK_L(RegQueryValueEx(hKeyNewAccount, name.c_str(), nullptr, nullptr, (LPBYTE)buffer, &size), "RegReadAccountKey");
		return buffer;
	}

	vector<byte> RegReadAccountKeyBinary(const wstring &name)
	{
		vector<byte> buffer(4096);
		DWORD size = (DWORD)buffer.size();
		CHECK_L(RegQueryValueEx(hKeyNewAccount, name.c_str(), nullptr, nullptr, (LPBYTE)&buffer[0], &size), "RegReadAccountKey");
		buffer.resize(size);
		return buffer;
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
		WriteAccountKey(R_ACCOUNT_NAME, accountName);
		WriteAccountKey(R_DISPLAY_NAME, displayName);
		WriteAccountKey(R_SERVER_URL, server);
		WriteAccountKey(R_USERNAME, username);
		WriteAccountKey(R_EMAIL, email);

		if (!emailOriginal.empty())
			WriteAccountKey(R_EMAIL_ORIGINAL, emailOriginal);

		WriteAccountKey(L"clsid", L"{ED475415-B0D6-11D2-8C3B-00104B2A6676}");

		WriteAccountKey(R_PASSWORD, &encryptedPassword[0], encryptedPassword.size());

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

			if (entryId.size() == 0)
				throw exception("entryId not initialised");

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

int __cdecl wmain(int argc, wchar_t  **argv)
{
	Account account;
	// Main
	try
	{
		if (argc != 7)
		{
			fwprintf(stderr, L"EASAccount: <profile> <outlook version> <accountid> <username> <email> <display>\n");
			exit(3);
		}

		account.profileName = argv[1];
		account.outlookVersion = argv[2];
		account.LoadFromAccountId(argv[3]);
		wprintf(L"ADDING SHARE: %s#%s\n", account.username.data(), argv[4]);
		account.username = account.username + L"#" + argv[4];
		account.emailOriginal = account.email;
		account.email = argv[5];
		account.accountName = account.email;
		account.displayName = argv[6];

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
