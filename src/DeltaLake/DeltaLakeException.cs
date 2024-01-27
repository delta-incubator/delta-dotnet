using System;
using DeltaLake.Bridge;

namespace DeltaLake
{
    /// <summary>
    /// Represents an error that occurs during interaction with the underlying runtime
    /// </summary>
    public class DeltaLakeException : Exception
    {
        /// <summary>
        ///  Initializes a new instance of the DeltaLakeException class.
        /// </summary>
        /// <param name="message">The message that describes the error</param>
        /// <param name="code">The code that describes the error</param>
        public DeltaLakeException(string? message, int code) : base(message)
        {
            ErrorCode = code;
        }


        /// <summary>
        ///  Initializes a new instance of the DeltaLakeException class
        ///  and a reference to the inner exception that is the cause of this exception
        /// </summary>
        /// <param name="message">The message that describes the error</param>
        /// <param name="code">The code that describes the error</param>
        /// <param name="innerException">The exception that is the cause of the current exception, or a null reference</param>
        public DeltaLakeException(string? message, int code, Exception? innerException)
        : base(message, innerException)
        {
            ErrorCode = code;
        }

        /// <summary>
        /// A unique error code coming from the delta lake runtime
        /// </summary>
        public int ErrorCode { get; }

        /// <summary>
        /// Consumes and frees the <see cref="Bridge.Interop.DeltaTableError"/>
        /// </summary>
        /// <param name="runtime">Pointer to the unmanaged runtime</param>
        /// <param name="error">Pointer to the unmanaged error</param>
        /// <returns>A new DeltaLakeException instance</returns>
        /// <exception cref="ArgumentNullException">Throw when error is null</exception>
        internal unsafe static DeltaLakeException FromDeltaTableError(Bridge.Interop.Runtime* runtime, Bridge.Interop.DeltaTableError* error)
        {
            if (error == null)
            {
                throw new ArgumentNullException(nameof(error));
            }

            try
            {
                var message = ByteArrayRef.StrictUTF8.GetString(error->error.data, (int)error->error.size);
                return new DeltaLakeException(message, (int)error->code);
            }
            finally
            {
                Bridge.Interop.Methods.error_free(runtime, error);
            }
        }
    }
}