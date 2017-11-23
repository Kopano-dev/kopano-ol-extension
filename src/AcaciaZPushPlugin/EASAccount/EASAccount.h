#ifndef __EASACCOUNT_MAIN_H__
#define __EASACCOUNT_MAIN_H__

#define USES_IID_IMsgServiceAdmin2
#define USES_IID_IMAPISession
#define MAPI_FORCE_ACCESS 0x00080000

#define PR_PROFILE_SECURE_MAILBOX PROP_TAG( PT_BINARY, 0x67F0)

#define PR_PST_CONFIG_FLAGS PROP_TAG(PT_LONG, 0x6770)

#define PR_PROFILE_OFFLINE_STORE_PATH_A PROP_TAG(PT_STRING8, 0x6610)
#define PR_PROFILE_OFFLINE_STORE_PATH_W PROP_TAG(PT_UNICODE, 0x6610)

#define PROP_ACCT_ID					PROP_TAG(PT_LONG, 0x1)

#define NOMINMAX 
#include <atlbase.h>

#include <algorithm>
#include <deque>
#include <exception>
#include <memory>
#include <string>
#include <vector>

#include <initguid.h>
// See README.txt if the build fails here
#include <MAPI.h>
#include <MAPIX.h>
#include <MAPIguid.h>
#include <MAPIAux.h>

#include <crtdbg.h>
#include <comdef.h>
#include <Shlobj.h>
#include <strsafe.h>

using namespace std;

static const wchar_t *KEY_ACCOUNTS = L"SOFTWARE\\Microsoft\\Office\\%d.0\\Outlook\\Profiles\\%s\\9375CFF0413111d3B88A00104B2A6676";
static const wchar_t *KEY_OLKMAIL = L"{ED475418-B0D6-11D2-8C3B-00104B2A6676}";
static const wchar_t *KEY_OLKADDRESSBOOK = L"{ED475419-B0D6-11D2-8C3B-00104B2A6676}";
static const wchar_t *KEY_OLKSTORE = L"{ED475420-B0D6-11D2-8C3B-00104B2A6676}";
static const wchar_t *KEY_LASTCHANGEVER = L"LastChangeVer";
static const wchar_t *VALUE_NEXT_ACCOUNT_ID = L"NextAccountID";

DEFINE_GUID(CLSID_OlkAccountManager, 0xed475410, 0xb0d6, 0x11d2, 0x8c, 0x3b, 0x0, 0x10, 0x4b, 0x2a, 0x66, 0x76);
DEFINE_GUID(IID_IOlkAccountManager, 0x9240a6cd, 0xaf41, 0x11d2, 0x8c, 0x3b, 0x0, 0x10, 0x4b, 0x2a, 0x66, 0x76);
DEFINE_GUID(CLSID_OlkMail, 0xed475418, 0xb0d6, 0x11d2, 0x8c, 0x3b, 0x0, 0x10, 0x4b, 0x2a, 0x66, 0x76);

typedef struct {
	DWORD	cb;
	BYTE * pb;
} ACCT_BIN;

typedef struct
{
	DWORD dwType;
	union
	{
		DWORD dw;
		WCHAR *pwsz;
		ACCT_BIN bin;
	} Val;
} ACCT_VARIANT;

interface IOlkErrorUnknown : IUnknown
{
	//GetLastError Gets a message string for the specified error.  
	virtual STDMETHODIMP GetLastError(HRESULT hr, LPWSTR* ppwszError);
};

interface IOlkAccountHelper : IUnknown
{
public:
	//Placeholder1 This member is a placeholder and is not supported.
	virtual STDMETHODIMP PlaceHolder1(LPVOID) = 0;

	//GetIdentity Gets the profile name of an account. 
	virtual STDMETHODIMP GetIdentity(LPWSTR pwszIdentity, DWORD * pcch) = 0;
	//GetMapiSession Gets the current MAPI session. 
	virtual STDMETHODIMP GetMapiSession(LPUNKNOWN * ppmsess) = 0;
	//HandsOffSession Releases the current MAPI session that has been created by 
	//IOlkAccountHelper::GetMapiSession. 
	virtual STDMETHODIMP HandsOffSession() = 0;
};

interface IOlkAccount : IOlkErrorUnknown
{
public:
	//Placeholder member Not supported or documented. 
	virtual STDMETHODIMP PlaceHolder1();
	//Placeholder member Not supported or documented. 
	virtual STDMETHODIMP PlaceHolder2();
	//Placeholder member Not supported or documented. 
	virtual STDMETHODIMP PlaceHolder3();
	//Placeholder member Not supported or documented. 
	virtual STDMETHODIMP PlaceHolder4();
	//Placeholder member Not supported or documented. 
	virtual STDMETHODIMP PlaceHolder5();
	//Placeholder member Not supported or documented. 
	virtual STDMETHODIMP PlaceHolder6();

	//GetAccountInfo Gets the type and categories of the specified account. 
	virtual STDMETHODIMP GetAccountInfo(CLSID* pclsidType, DWORD* pcCategories, CLSID** prgclsidCategory);
	//GetProp Gets the value of the specified account property. See the Properties table below. 
	virtual STDMETHODIMP GetProp(DWORD dwProp, ACCT_VARIANT* pVar);
	//SetProp Sets the value of the specified account property. See the Properties table below. 
	virtual STDMETHODIMP SetProp(DWORD dwProp, ACCT_VARIANT* pVar);

	//Placeholder member Not supported or documented. 
	virtual STDMETHODIMP PlaceHolder7();
	//Placeholder member Not supported or documented. 
	virtual STDMETHODIMP PlaceHolder8();
	//Placeholder member Not supported or documented. 
	virtual STDMETHODIMP PlaceHolder9();

	//FreeMemory Frees memory allocated by the IOlkAccount interface. 
	virtual STDMETHODIMP FreeMemory(BYTE* pv);

	//Placeholder member Not supported or documented. 
	virtual STDMETHODIMP PlaceHolder10();

	//SaveChanges Saves changes to the specified account. 
	virtual STDMETHODIMP SaveChanges(DWORD dwFlags);
};


interface IOlkAccountNotify : IOlkErrorUnknown
{
public:
	//Notify Notifies the client of changes to the specified account. 
	STDMETHODIMP Notify(DWORD dwNotify, DWORD dwAcctID, DWORD dwFlags);
};

interface IOlkEnum : IUnknown
{
public:
	//GetCount  Gets the number of accounts in the enumerator. 
	virtual STDMETHODIMP GetCount(DWORD *pulCount);
	//Reset Resets the enumerator to the beginning. 
	virtual STDMETHODIMP Reset();
	//GetNext Gets the next account in the enumerator. 
	virtual STDMETHODIMP GetNext(LPUNKNOWN* ppunk);
	//Skip Skips a specified number of accounts in the enumerator. 
	virtual STDMETHODIMP Skip(DWORD cSkip);
};


interface IOlkAccountManager : IOlkErrorUnknown
{
public:
	//Init Initializes the account manager for use. 
	virtual STDMETHODIMP Init(IOlkAccountHelper* pAcctHelper, DWORD dwFlags);

	//Placeholder member Not supported or documented 
	//virtual STDMETHODIMP PlaceHolder1();
	//DisplayAccountList Displays the account list wizard
	virtual STDMETHODIMP DisplayAccountList(
		HWND hwnd,
		DWORD dwFlags,
		LPCWSTR lpwszReserved, // Not used
		DWORD dwReserved, // Not used
		const CLSID * pclsidReserved1, // Not used
		const CLSID * pclsidReserved2); // Not used


										//Placeholder member Not supported or documented 
	virtual STDMETHODIMP PlaceHolder2();
	//Placeholder member Not supported or documented 
	virtual STDMETHODIMP PlaceHolder3();
	//Placeholder member Not supported or documented 
	virtual STDMETHODIMP PlaceHolder4();
	//Placeholder member Not supported or documented 
	virtual STDMETHODIMP PlaceHolder5();
	//Placeholder member Not supported or documented 
	virtual STDMETHODIMP PlaceHolder6();

	//FindAccount Finds an account by property value. 
	virtual STDMETHODIMP FindAccount(DWORD dwProp, ACCT_VARIANT* pVar, IOlkAccount** ppAccount);

	//Placeholder member Not supported or documented 
	virtual STDMETHODIMP PlaceHolder7();
	//Placeholder member Not supported or documented 
	virtual STDMETHODIMP PlaceHolder8();
	//Placeholder member Not supported or documented 
	virtual STDMETHODIMP PlaceHolder9();

	//DeleteAccount Deletes the specified account. 
	virtual STDMETHODIMP DeleteAccount(DWORD dwAcctID);

	//Placeholder member Not supported or documented 
	virtual STDMETHODIMP PlaceHolder10();

	//SaveChanges Saves changes to the specified account. 
	virtual STDMETHODIMP SaveChanges(DWORD dwAcctID, DWORD dwFlags);
	//GetOrder Gets the ordering of the specified category of accounts. 
	virtual STDMETHODIMP GetOrder(const CLSID* pclsidCategory, DWORD* pcAccts, DWORD* prgAccts[]);
	//SetOrder Modifies the ordering of the specified category of accounts. 
	virtual STDMETHODIMP SetOrder(const CLSID* pclsidCategory, DWORD* pcAccts, DWORD* prgAccts[]);
	//EnumerateAccounts Gets an enumerator for the accounts of the specific category and type. 
	virtual STDMETHODIMP EnumerateAccounts(const CLSID* pclsidCategory, const CLSID* pclsidType, DWORD dwFlags, IOlkEnum** ppEnum);

	//Placeholder member Not supported or documented 
	virtual STDMETHODIMP PlaceHolder11();
	//Placeholder member Not supported or documented 
	virtual STDMETHODIMP PlaceHolder12();

	//FreeMemory Frees memory allocated by the IOlkAccountManager interface. 
	virtual STDMETHODIMP FreeMemory(BYTE* pv);
	//Advise Registers an account for notifications sent by the account manager. 
	virtual STDMETHODIMP Advise(IOlkAccountNotify* pNotify, DWORD* pdwCookie);
	//Unadvise Unregisters an account for notifications sent by the account manager. 
	virtual STDMETHODIMP Unadvise(DWORD* pdwCookie);

	//Placeholder member Not supported or documented 
	virtual STDMETHODIMP PlaceHolder13();
	//Placeholder member Not supported or documented 
	virtual STDMETHODIMP PlaceHolder14();
	//Placeholder member Not supported or documented 
	virtual STDMETHODIMP PlaceHolder15();
};

class CustomException : public exception
{
public:
	const LONG status;
	wstring message;

	CustomException(LONG status, const char *ident, const wchar_t *message)
		:
		exception(ident),
		status(status)
	{
		wchar_t buffer[0x10000];
		wnsprintf(buffer, sizeof(buffer), L"%.8X: %hs: %s", status, ident, message);
		this->message = buffer;
	}

	CustomException(LONG status, const char *ident)
		:
		exception(ident),
		status(status)
	{
		LPSTR messageBuffer = nullptr;
		size_t size = FormatMessageA(FORMAT_MESSAGE_ALLOCATE_BUFFER | FORMAT_MESSAGE_FROM_SYSTEM | FORMAT_MESSAGE_IGNORE_INSERTS,
			NULL, status, MAKELANGID(LANG_NEUTRAL, SUBLANG_DEFAULT), (LPSTR)&messageBuffer, 0, NULL);

		wchar_t buffer[0x10000];
		wnsprintf(buffer, sizeof(buffer), L"%.8X: %hs: %hs", status, ident, messageBuffer);
		LocalFree(messageBuffer);
		this->message = buffer;
	}
};

#endif /* __EASACCOUNT_MAIN_H__ */