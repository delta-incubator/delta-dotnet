// -----------------------------------------------------------------------------
// <summary>
// Converts FFI Arrow Schema into CArrowSchema.
// </summary>
//
// <copyright company="The Delta Lake Project Authors">
// Copyright (2024) The Delta Lake Project Authors.  All rights reserved.
// Licensed under the Apache license. See LICENSE file in the project root for full license information.
// </copyright>
// -----------------------------------------------------------------------------

using Apache.Arrow;
using Apache.Arrow.C;
using DeltaLake.Kernel.Interop;

namespace DeltaLake.Kernel.Arrow.Converters
{
    /// <summary>
    /// Static methods to convert <see cref="FFI_ArrowSchema"/> schema to <see
    /// cref="CArrowSchema"/>.
    /// </summary>
    /// <remarks>
    /// <see cref="CArrowSchema"/> release property is internal for some reason,
    /// but that's not the case for <see cref="FFI_ArrowSchema"/>. To do the
    /// conversion, we need to initialize it with a known type and then
    /// overwrite it.
    ///
    /// TOOD: Need to check with the Arrow C# authors why they do stuff like
    /// this that makes life hard, i.e. why is release the only property that's
    /// internal, without a setter?
    ///
    /// We can also check with the Delta Kernel team if there's a better way to
    /// do this.
    /// </remarks>
    internal static class ArrowFfiSchemaConverter
    {
        internal static unsafe CArrowSchema* ConvertFFISchema(FFI_ArrowSchema* ffiSchema, CArrowSchema* cSchema)
        {
            var int32Type = new Apache.Arrow.Types.Int32Type();
            CArrowSchemaExporter.ExportType(int32Type, cSchema);

            cSchema->format = (byte*)ffiSchema->format;
            cSchema->name = (byte*)ffiSchema->name;
            cSchema->metadata = (byte*)ffiSchema->metadata;
            cSchema->flags = ffiSchema->flags;
            cSchema->n_children = ffiSchema->n_children;
            cSchema->children = (CArrowSchema**)ffiSchema->children;
            cSchema->dictionary = (CArrowSchema*)ffiSchema->dictionary;
            cSchema->private_data = ffiSchema->private_data;

            return cSchema;
        }

        internal static unsafe CArrowArray* ConvertFFIArray(FFI_ArrowArray* ffiArray, CArrowArray* cArray)
        {
            IArrowArray arrowArray = new Int32Array.Builder().Append(1).Build();
            CArrowArrayExporter.ExportArray(arrowArray, cArray);

            cArray->length = ffiArray->length;
            cArray->null_count = ffiArray->null_count;
            cArray->offset = ffiArray->offset;
            cArray->n_buffers = ffiArray->n_buffers;
            cArray->n_children = ffiArray->n_children;
            cArray->buffers = (byte**)ffiArray->buffers;
            cArray->children = (CArrowArray**)ffiArray->children;
            cArray->dictionary = (CArrowArray*)ffiArray->dictionary;
            cArray->private_data = ffiArray->private_data;

            return cArray;
        }
    }
}
