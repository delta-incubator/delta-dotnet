using System;

namespace DeltaLake.Errors
{
    /// <summary>
    /// Represents a configuration error detected before interacting with the runtime
    /// </summary>
    public class DeltaConfigurationException : DeltaLakeException
    {

        /// <summary>
        ///  Initializes a new instance of the DeltaConfigurationException class.
        /// </summary>
        /// <param name="message">The message that describes the error</param>
        public DeltaConfigurationException(string? message)
            : base(message, 1000)
        {
        }

        /// <summary>
        ///  Initializes a new instance of the DeltaConfigurationException class
        ///  and a reference to the inner exception that is the cause of this exception
        /// </summary>
        /// <param name="message">The message that describes the error</param>
        /// <param name="innerException">The exception that is the cause of the current exception, or a null reference</param>
        public DeltaConfigurationException(string? message, Exception? innerException)
            : base(message, 1000, innerException)
        {
        }
    }
}