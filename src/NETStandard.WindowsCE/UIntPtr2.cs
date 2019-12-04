namespace System
{
    [CLSCompliant(false)]
    public static class UIntPtr2
    {
        public static readonly UIntPtr Zero = UIntPtr.Zero;

        public static int Size => UIntPtr.Size;

        public static UIntPtr Add(UIntPtr pointer, int offset) => new UIntPtr(pointer.ToUInt32() + (uint)offset);

        public static UIntPtr Subtract(UIntPtr pointer, int offset) => new UIntPtr(pointer.ToUInt32() - (uint)offset);
    }
}
