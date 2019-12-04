namespace System.Runtime.CompilerServices
{
    [CLSCompliant(false)]
    public static class Unsafe
    {
        [MethodImpl(MethodImplOptions.ForwardRef)]
        public static extern unsafe T Read<T>(void* source);

        [MethodImpl(MethodImplOptions.ForwardRef)]
        public static extern unsafe T ReadUnaligned<T>(void* source);

        [MethodImpl(MethodImplOptions.ForwardRef)]
        public static extern T ReadUnaligned<T>(ref byte source);

        [MethodImpl(MethodImplOptions.ForwardRef)]
        public static extern unsafe void Write<T>(void* destination, T value);

        [MethodImpl(MethodImplOptions.ForwardRef)]
        public static extern unsafe void WriteUnaligned<T>(void* destination, T value);

        [MethodImpl(MethodImplOptions.ForwardRef)]
        public static extern void WriteUnaligned<T>(ref byte destination, T value);

        [MethodImpl(MethodImplOptions.ForwardRef)]
        public static extern unsafe void Copy<T>(void* destination, ref T source);

        [MethodImpl(MethodImplOptions.ForwardRef)]
        public static extern unsafe void Copy<T>(ref T destination, void* source);

        [MethodImpl(MethodImplOptions.ForwardRef)]
        public static extern unsafe void* AsPointer<T>(ref T value);

        [MethodImpl(MethodImplOptions.ForwardRef)]
        public static extern int SizeOf<T>();

        [MethodImpl(MethodImplOptions.ForwardRef)]
        public static extern unsafe void CopyBlock(void* destination, void* source, uint byteCount);

        [MethodImpl(MethodImplOptions.ForwardRef)]
        public static extern void CopyBlock(ref byte destination, ref byte source, uint byteCount);

        [MethodImpl(MethodImplOptions.ForwardRef)]
        public static extern unsafe void CopyBlockUnaligned(void* destination, void* source, uint byteCount);

        [MethodImpl(MethodImplOptions.ForwardRef)]
        public static extern void CopyBlockUnaligned(ref byte destination, ref byte source, uint byteCount);

        [MethodImpl(MethodImplOptions.ForwardRef)]
        public static extern unsafe void InitBlock(void* startAddress, byte value, uint byteCount);

        [MethodImpl(MethodImplOptions.ForwardRef)]
        public static extern void InitBlock(ref byte startAddress, byte value, uint byteCount);

        [MethodImpl(MethodImplOptions.ForwardRef)]
        public static extern unsafe void InitBlockUnaligned(void* startAddress, byte value, uint byteCount);

        [MethodImpl(MethodImplOptions.ForwardRef)]
        public static extern void InitBlockUnaligned(ref byte startAddress, byte value, uint byteCount);

        [MethodImpl(MethodImplOptions.ForwardRef)]
        public static extern T As<T>(object o) where T : class;

        [MethodImpl(MethodImplOptions.ForwardRef)]
        public static extern unsafe ref T AsRef<T>(void* source);

        [MethodImpl(MethodImplOptions.ForwardRef)]
        public static extern ref T AsRef<T>(in T source);

        [MethodImpl(MethodImplOptions.ForwardRef)]
        public static extern ref TTo As<TFrom, TTo>(ref TFrom source);

        [MethodImpl(MethodImplOptions.ForwardRef)]
        public static extern ref T Add<T>(ref T source, int elementOffset);

        [MethodImpl(MethodImplOptions.ForwardRef)]
        public static extern unsafe void* Add<T>(void* source, int elementOffset);

        [MethodImpl(MethodImplOptions.ForwardRef)]
        public static extern unsafe ref T Add<T>(ref T source, IntPtr elementOffset);

        [MethodImpl(MethodImplOptions.ForwardRef)]
        public static extern ref T AddByteOffset<T>(ref T source, IntPtr byteOffset);

        [MethodImpl(MethodImplOptions.ForwardRef)]
        public static extern ref T Subtract<T>(ref T source, int elementOffset);

        [MethodImpl(MethodImplOptions.ForwardRef)]
        public static extern unsafe void* Subtract<T>(void* source, int elementOffset);

        [MethodImpl(MethodImplOptions.ForwardRef)]
        public static extern unsafe ref T Subtract<T>(ref T source, IntPtr elementOffset);

        [MethodImpl(MethodImplOptions.ForwardRef)]
        public static extern ref T SubtractByteOffset<T>(ref T source, IntPtr byteOffset);

        [MethodImpl(MethodImplOptions.ForwardRef)]
        public static extern IntPtr ByteOffset<T>(ref T origin, ref T target);

        [MethodImpl(MethodImplOptions.ForwardRef)]
        public static extern bool AreSame<T>(ref T left, ref T right);

        [MethodImpl(MethodImplOptions.ForwardRef)]
        public static extern bool IsAddressGreaterThan<T>(ref T left, ref T right);

        [MethodImpl(MethodImplOptions.ForwardRef)]
        public static extern bool IsAddressLessThan<T>(ref T left, ref T right);
    }
}
