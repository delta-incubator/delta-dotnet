// -----------------------------------------------------------------------------
// <summary>
// Error handling for Kernel read operations.
// </summary>
//
// <copyright company="The Delta Lake Project Authors">
// Copyright (2024) The Delta Lake Project Authors.  All rights reserved.
// Licensed under the Apache license. See LICENSE file in the project root for full license information.
// </copyright>
// -----------------------------------------------------------------------------

using System;
using DeltaLake.Kernel.Interop;

namespace DeltaLake.Kernel.Callbacks.Errors
{
    /// <summary>
    /// Represents an error that occurs during interaction with the delta kernel
    /// </summary>
    public class KernelException : Exception
    {
        /// <summary>
        ///  Initializes a new instance of the KernelException class.
        /// </summary>
        /// <param name="message">The message that describes the error</param>
        /// <param name="source">The error that caused this exception</param>
        internal unsafe KernelException(string? message, KernelReadError* source) : base(message)
        {
            ErrorCode = source->etype.etype;
            KernelMessage = source->Message;
        }

        /// <summary>
        ///  Initializes a new instance of the KernelException class.
        /// </summary>
        /// <param name="source">The error that caused this exception</param>
        internal unsafe KernelException(KernelReadError* source) : base(source->Message)
        {
            ErrorCode = source->etype.etype;
            KernelMessage = Message;
        }

        /// <summary>
        /// A unique error code coming from the kernel
        /// </summary>
        public KernelError ErrorCode { get; }

        /// <summary>
        /// The error message from the kernel process
        /// </summary>
        public string KernelMessage { get; }

        /// <summary>
        /// Creates a <see cref="KernelException"/> from an <see cref="EngineError"/> pointer
        /// and destroys the pointer afterward
        /// </summary>
        /// <param name="source">The incoming error. Consumed.</param>
        /// <param name="message">An optional message to add in addition to the kernel error</param>
        /// <returns></returns>
        internal unsafe static KernelException FromEngineError(EngineError* source, string? message)
        {
            if (message == null)
            {
                return KernelReadError.ProcessEngineError(source, error => new KernelException(error));
            }

            return KernelReadError.ProcessEngineError(source, error => new KernelException(message, error));
        }
    }
}
