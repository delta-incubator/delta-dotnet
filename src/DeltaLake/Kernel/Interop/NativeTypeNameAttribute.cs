// -----------------------------------------------------------------------------
// <summary>
// This was taken when running generating the FFI -> C# with
//
//  "--config generate-helper-types" passed into CLangSharp.
// 
// The problem with doing it in one file, is CLangSharp generates
// a bunch of inline using statements, which doesn't compile.
// </summary>
//
// <copyright company="The Delta Lake Project Authors">
// Copyright (2024) The Delta Lake Project Authors.  All rights reserved.
// Licensed under the Apache license. See LICENSE file in the project root for full license information.
// </copyright>
// -----------------------------------------------------------------------------

using System;
using System.Diagnostics;

namespace DeltaLake.Kernel.Interop
{
    /// <summary>
    /// Defines the type of a member as it was used in the native signature.
    ///  </summary>
    [AttributeUsage(
        AttributeTargets.Struct
            | AttributeTargets.Enum
            | AttributeTargets.Property
            | AttributeTargets.Field
            | AttributeTargets.Parameter
            | AttributeTargets.ReturnValue,
        AllowMultiple = false,
        Inherited = true
    )]
    [Conditional("DEBUG")]
    internal sealed partial class NativeTypeNameAttribute : Attribute
    {
        private readonly string _name;

        /// <summary>
        /// Initializes a new instance of the <see cref="NativeTypeNameAttribute" /> class.
        /// </summary>
        /// <param name="name"> The name of the type that was used in the native signature. </param>
        public NativeTypeNameAttribute(string name)
        {
            _name = name;
        }

        /// <summary>
        /// Gets the name of the type that was used in the native signature
        /// </summary>
        public string Name => _name;
    }
}