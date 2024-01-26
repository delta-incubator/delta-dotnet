using System;
using System.Buffers;
using System.Runtime.InteropServices;

namespace DeltaLake.Bridge
{
    internal sealed unsafe class UnmanagedMemoryManager<T> : MemoryManager<T>
        where T : unmanaged
    {
        private readonly T* _pointer;

        private readonly int _length;

        /// <summary>
        /// Create a new UnmanagedMemoryManager instance at the given pointer and size
        /// </summary>
        /// <remarks>It is assumed that the span provided is already unmanaged or externally pinned</remarks>
        public UnmanagedMemoryManager(Span<T> span)
        {
            fixed (T* ptr = &MemoryMarshal.GetReference(span))
            {
                _pointer = ptr;
                _length = span.Length;
            }
        }

        /// <summary>
        /// Create a new UnmanagedMemoryManager instance at the given pointer and size
        /// </summary>
        public UnmanagedMemoryManager(T* pointer, int length)
        {
#pragma warning disable CA1512 // Use ArgumentOutOfRangeException throw helper
            if (length < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(length));
            }
#pragma warning restore CA1512 // Use ArgumentOutOfRangeException throw helper

            _pointer = pointer;
            _length = length;
        }

        /// <summary>
        /// Create a new UnmanagedMemoryManager instance at the given pointer and size
        /// </summary>
        public UnmanagedMemoryManager(IntPtr pointer, int length) : this((T*)pointer.ToPointer(), length) { }

        /// <summary>
        /// Obtains a span that represents the region
        /// </summary>
        public override Span<T> GetSpan() => new Span<T>(_pointer, _length);

        /// <summary>
        /// Provides access to a pointer that represents the data (note: no actual pin occurs)
        /// </summary>
        public override MemoryHandle Pin(int elementIndex = 0)
        {
            if (elementIndex < 0 || elementIndex > _length)
            {
                throw new ArgumentOutOfRangeException(nameof(elementIndex));
            }

            return new MemoryHandle(_pointer + elementIndex);
        }

        /// <summary>
        /// Has no effect
        /// </summary>
        public override void Unpin() { }

        /// <summary>
        /// Releases all resources associated with this object
        /// </summary>
        protected override void Dispose(bool disposing) { }
    }
}