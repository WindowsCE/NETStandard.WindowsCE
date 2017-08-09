// Ref: https://github.com/NetTopologySuite/NetTopologySuite/blob/master/NetTopologySuite/Utilities/BitConverter.cs

namespace System
{
    public static class BitConverter2
    {
        public static readonly bool IsLittleEndian = BitConverter.IsLittleEndian;

        public static long DoubleToInt64Bits(double value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            long result = BitConverter.ToInt64(bytes, 0);
            return result;
        }

        public static byte[] GetBytes(bool value)
            => BitConverter.GetBytes(value);

        public static byte[] GetBytes(char value)
            => BitConverter.GetBytes(value);

        public static byte[] GetBytes(double value)
            => BitConverter.GetBytes(value);

        public static byte[] GetBytes(short value)
            => BitConverter.GetBytes(value);

        public static byte[] GetBytes(int value)
            => BitConverter.GetBytes(value);

        public static byte[] GetBytes(long value)
            => BitConverter.GetBytes(value);

        public static byte[] GetBytes(float value)
            => BitConverter.GetBytes(value);

        [CLSCompliant(false)]
        public static byte[] GetBytes(ushort value)
            => BitConverter.GetBytes(value);

        [CLSCompliant(false)]
        public static byte[] GetBytes(uint value)
            => BitConverter.GetBytes(value);

        [CLSCompliant(false)]
        public static byte[] GetBytes(ulong value)
            => BitConverter.GetBytes(value);

        public static double Int64BitsToDouble(long value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            double result = BitConverter.ToDouble(bytes, 0);
            return result;
        }

        public static bool ToBoolean(byte[] value, int startIndex)
            => BitConverter.ToBoolean(value, startIndex);

        public static char ToChar(byte[] value, int startIndex)
            => BitConverter.ToChar(value, startIndex);

        public static double ToDouble(byte[] value, int startIndex)
            => BitConverter.ToDouble(value, startIndex);

        public static short ToInt16(byte[] value, int startIndex)
            => BitConverter.ToInt16(value, startIndex);

        public static int ToInt32(byte[] value, int startIndex)
            => BitConverter.ToInt32(value, startIndex);

        public static long ToInt64(byte[] value, int startIndex)
            => BitConverter.ToInt64(value, startIndex);

        public static float ToSingle(byte[] value, int startIndex)
            => BitConverter.ToSingle(value, startIndex);

        public static string ToString(byte[] value)
            => BitConverter.ToString(value);

        public static string ToString(byte[] value, int startIndex)
            => BitConverter.ToString(value, startIndex);

        public static string ToString(byte[] value, int startIndex, int length)
            => BitConverter.ToString(value, startIndex, length);

        [CLSCompliant(false)]
        public static ushort ToUInt16(byte[] value, int startIndex)
            => BitConverter.ToUInt16(value, startIndex);

        [CLSCompliant(false)]
        public static uint ToUInt32(byte[] value, int startIndex)
            => BitConverter.ToUInt32(value, startIndex);

        [CLSCompliant(false)]
        public static ulong ToUInt64(byte[] value, int startIndex)
            => BitConverter.ToUInt64(value, startIndex);
    }
}
