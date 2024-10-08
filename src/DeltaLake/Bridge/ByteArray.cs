using System;
using System.Runtime.InteropServices;

namespace DeltaLake.Bridge
{
    /// <summary>
    /// Representation of a byte array owned by Core.
    /// </summary>
    internal sealed class ByteArray : SafeHandle
    {
        private readonly Runtime runtime;
        private readonly unsafe Interop.ByteArray* byteArray;

        /// <summary>
        /// Initializes a new instance of the <see cref="ByteArray"/> class.
        /// </summary>
        /// <param name="runtime">Runtime to use to free the byte array.</param>
        /// <param name="byteArray">Byte array pointer.</param>
        public unsafe ByteArray(Runtime runtime, Interop.ByteArray* byteArray)
            : base((IntPtr)byteArray, true)
        {
            this.runtime = runtime;
            this.byteArray = byteArray;
        }

        /// <inheritdoc/>
        public override unsafe bool IsInvalid => false;

        /// <summary>
        /// Convert the byte array to a UTF8 string.
        /// </summary>
        /// <returns>Converted string.</returns>
        public string ToUTF8()
        {
            unsafe
            {
                return ByteArrayRef.StrictUTF8.GetString(byteArray->data, (int)byteArray->size);
            }
        }

        /// <summary>
        /// Copy the byte array to a new byte array.
        /// </summary>
        /// <returns>The new byte array.</returns>
        public byte[] ToByteArray()
        {
            unsafe
            {
                var bytes = new byte[(int)byteArray->size];
                Marshal.Copy((IntPtr)byteArray->data, bytes, 0, (int)byteArray->size);
                return bytes;
            }
        }

        /// <inheritdoc/>
        protected override unsafe bool ReleaseHandle()
        {
            runtime.FreeByteArray(byteArray);
            return true;
        }
    }
}