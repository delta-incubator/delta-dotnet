using System;
using DeltaLake.Bridge;

namespace DeltaLake.Errors
{
    /// <summary>
    /// Represents an error that occurs during interaction with the underlying runtime
    /// </summary>
    public class DeltaRuntimeException : DeltaLakeException
    {
        /// <summary>
        ///  Initializes a new instance of the DeltaRuntimeException class.
        /// </summary>
        /// <param name="message">The message that describes the error</param>
        /// <param name="code">The code that describes the error</param>
        public DeltaRuntimeException(string? message, int code)
            : base(message, code)
        {
        }


        /// <summary>
        ///  Initializes a new instance of the DeltaRuntimeException class
        ///  and a reference to the inner exception that is the cause of this exception
        /// </summary>
        /// <param name="message">The message that describes the error</param>
        /// <param name="code">The code that describes the error</param>
        /// <param name="innerException">The exception that is the cause of the current exception, or a null reference</param>
        public DeltaRuntimeException(string? message, int code, Exception? innerException)
        : base(message, code, innerException)
        {
        }

        /// <summary>
        /// Consumes and frees the <see cref="Bridge.Interop.DeltaTableError"/>
        /// </summary>
        /// <param name="runtime">Pointer to the unmanaged runtime</param>
        /// <param name="error">Pointer to the unmanaged error</param>
        /// <returns>A new DeltaRuntimeException instance</returns>
        /// <exception cref="ArgumentNullException">Throw when error is null</exception>
        internal unsafe static DeltaRuntimeException FromDeltaTableError(Bridge.Interop.Runtime* runtime, Bridge.Interop.DeltaTableError* error)
        {
            if (error == null)
            {
                throw new ArgumentNullException(nameof(error));
            }

            try
            {
                var message = ByteArrayRef.StrictUTF8.GetString(error->error.data, (int)error->error.size);
                return new DeltaRuntimeException(message, (int)error->code);
            }
            finally
            {
                Bridge.Interop.Methods.error_free(runtime, error);
            }
        }
    }
}