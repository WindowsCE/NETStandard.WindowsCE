using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Globalization;

namespace Console.Tests
{
    class Program
    {
        static void Main(string[] args)
        {
            System.Console.WriteLine("Test");

            foreach (var fact in Format_TestData())
                Format((Type)fact[0], fact[1], (string)fact[2], (string)fact[3]);
        }

        private static void Format(Type enumType, object value, string format, string expected)
        {
            // Format string is case insensitive
            var result = Enum2.Format(enumType, value, format.ToUpperInvariant());
            result = Enum2.Format(enumType, value, format.ToLowerInvariant());
        }

        private static IEnumerable<object[]> Format_TestData()
        {
            // Format: D
            yield return new object[] { typeof(SimpleEnum), 1, "D", "1" };

            // Format: X
            yield return new object[] { typeof(SimpleEnum), SimpleEnum.Red, "X", "00000001" };
            yield return new object[] { typeof(SimpleEnum), 1, "X", "00000001" };

            // Format: F
            yield return new object[] { typeof(SimpleEnum), 1, "F", "Red" };
        }

        public enum SimpleEnum
        {
            Red = 1,
            Blue = 2,
            Green = 3,
            Green_a = 3,
            Green_b = 3,
            B = 4
        }
    }
}
