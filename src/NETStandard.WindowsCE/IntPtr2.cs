namespace System
{
    public static class IntPtr2
    {
        public static readonly IntPtr Zero = IntPtr.Zero;

        public static int Size => IntPtr.Size;

        public static IntPtr Add(IntPtr pointer, int offset) => new IntPtr(pointer.ToInt32() + offset);

        public static IntPtr Subtract(IntPtr pointer, int offset) => new IntPtr(pointer.ToInt32() - offset);
    }
}
