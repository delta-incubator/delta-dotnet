using System;
#pragma warning disable IDE0005 // Using directive is unnecessary.
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Text;
#pragma warning restore IDE0005 // Using directive is unnecessary.

namespace DeltaLake.Extensions
{
    internal static class MarshalExtensions
    {
#if !NETCOREAPP
        public static unsafe string? PtrToStringUTF8(IntPtr intPtr)
        {
            if (intPtr == IntPtr.Zero)
            {
                return null;
            }

            byte* source = (byte*)intPtr;
            int length = 0;

            while (source[length] != 0)
            {
                length++;
            }

            return PtrToStringUTF8(intPtr, length);
        }

        public static unsafe string? PtrToStringUTF8(IntPtr intPtr, int length)
        {
            if (intPtr == IntPtr.Zero)
            {
                return null;
            }

            byte[] bytes = new byte[length];
            Marshal.Copy(intPtr, bytes, 0, length);

            return Encoding.UTF8.GetString(bytes);
        }

        public static unsafe IntPtr StringToCoTaskMemUTF8(string? s)
        {
            if (s is null)
            {
                return IntPtr.Zero;
            }

            int nb = Encoding.UTF8.GetMaxByteCount(s.Length);

            IntPtr pMem = Marshal.AllocHGlobal(nb + 1);

            int nbWritten;
            byte* pbMem = (byte*)pMem;

            fixed (char* firstChar = s)
            {
                nbWritten = Encoding.UTF8.GetBytes(firstChar, s.Length, pbMem, nb);
            }

            pbMem[nbWritten] = 0;

            return pMem;
        }
#else
        public static unsafe string? PtrToStringUTF8(IntPtr intPtr)
        {
            return Marshal.PtrToStringUTF8(intPtr);
        }

        public static IntPtr StringToCoTaskMemUTF8(string? s)
        {
            return Marshal.StringToCoTaskMemUTF8(s);
        }

        public static unsafe string? PtrToStringUTF8(IntPtr intPtr, int length)
        {
            return Marshal.PtrToStringUTF8(intPtr, length);
        }
#endif
    }
}

