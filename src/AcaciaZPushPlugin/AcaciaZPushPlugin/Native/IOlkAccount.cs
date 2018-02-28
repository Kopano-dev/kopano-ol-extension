using Acacia.Native.MAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Acacia.Native
{
    [StructLayout(LayoutKind.Explicit)]
    unsafe public struct ACCT_VARIANT
    {
        [FieldOffset(0), MarshalAs(UnmanagedType.U4)]
        public uint dwType;

        [FieldOffset(8)]
        public char* lpszW;

        [FieldOffset(8), MarshalAs(UnmanagedType.U4)]
        public uint dw;
    }

    [ComImport]
    [Guid("9240a6d2-af41-11d2-8c3b-00104b2a6676")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    unsafe public interface IOlkAccount
    {
        // IOlkErrorUnknown
        void IOlkErrorUnknown_GetLastError();

        void IOlkAccount_Placeholder1();
        void IOlkAccount_Placeholder2();
        void IOlkAccount_Placeholder3();
        void IOlkAccount_Placeholder4();
        void IOlkAccount_Placeholder5();
        void IOlkAccount_Placeholder6();

        void GetAccountInfo(Guid* pclsidType, int* pcCategories, Guid** prgclsidCategory);
        void GetProp(PropTag dwProp, ACCT_VARIANT *pVar);
        void SetProp(PropTag dwProp, ACCT_VARIANT *pVar);

        void IOlkAccount_Placeholder7();
        void IOlkAccount_Placeholder8();
        void IOlkAccount_Placeholder9();

        void FreeMemory(byte* pv);

        void IOlkAccount_Placeholder10();

        void SaveChanges(uint dwFlags);
    }
}
