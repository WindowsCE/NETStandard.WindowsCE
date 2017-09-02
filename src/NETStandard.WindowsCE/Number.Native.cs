// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.
//
// File: Number.cpp
//

// References:
// - https://github.com/dotnet/coreclr/blob/54891e0650e69f08832f75a40dc102efc6115d38/src/classlibnative/bcltype/number.cpp
// - https://gist.github.com/pieceofsummer/a555baa83a3c6f71925dac9f2d8b2d86

#if NET35_CF
// Windows CE supports little-endian format only
// Ref: https://msdn.microsoft.com/en-us/library/ms905093.aspx
#define LITTLEENDIAN
#else
#define BIGENDIAN
#endif

using System.Diagnostics;

namespace System
{
    partial class Number
    {
        //
        // precomputed tables with powers of 10. These allows us to do at most
        // two Mul64 during the conversion. This is important not only
        // for speed, but also for precision because of Mul64 computes with 1 bit error.
        //

        static readonly ulong[] rgval64Power10 = {
            // powers of 10
            /*1*/ 0xa000000000000000UL,
            /*2*/ 0xc800000000000000UL,
            /*3*/ 0xfa00000000000000UL,
            /*4*/ 0x9c40000000000000UL,
            /*5*/ 0xc350000000000000UL,
            /*6*/ 0xf424000000000000UL,
            /*7*/ 0x9896800000000000UL,
            /*8*/ 0xbebc200000000000UL,
            /*9*/ 0xee6b280000000000UL,
            /*10*/ 0x9502f90000000000UL,
            /*11*/ 0xba43b74000000000UL,
            /*12*/ 0xe8d4a51000000000UL,
            /*13*/ 0x9184e72a00000000UL,
            /*14*/ 0xb5e620f480000000UL,
            /*15*/ 0xe35fa931a0000000UL,

            // powers of 0.1
            /*1*/ 0xcccccccccccccccdUL,
            /*2*/ 0xa3d70a3d70a3d70bUL,
            /*3*/ 0x83126e978d4fdf3cUL,
            /*4*/ 0xd1b71758e219652eUL,
            /*5*/ 0xa7c5ac471b478425UL,
            /*6*/ 0x8637bd05af6c69b7UL,
            /*7*/ 0xd6bf94d5e57a42beUL,
            /*8*/ 0xabcc77118461ceffUL,
            /*9*/ 0x89705f4136b4a599UL,
            /*10*/ 0xdbe6fecebdedd5c2UL,
            /*11*/ 0xafebff0bcb24ab02UL,
            /*12*/ 0x8cbccc096f5088cfUL,
            /*13*/ 0xe12e13424bb40e18UL,
            /*14*/ 0xb424dc35095cd813UL,
            /*15*/ 0x901d7cf73ab0acdcUL,
        };

        static readonly byte[] rgexp64Power10 = {
            // exponents for both powers of 10 and 0.1
            /*1*/ 4,
            /*2*/ 7,
            /*3*/ 10,
            /*4*/ 14,
            /*5*/ 17,
            /*6*/ 20,
            /*7*/ 24,
            /*8*/ 27,
            /*9*/ 30,
            /*10*/ 34,
            /*11*/ 37,
            /*12*/ 40,
            /*13*/ 44,
            /*14*/ 47,
            /*15*/ 50,
        };

        static readonly ulong[] rgval64Power10By16 = {
            // powers of 10^16
            /*1*/ 0x8e1bc9bf04000000UL,
            /*2*/ 0x9dc5ada82b70b59eUL,
            /*3*/ 0xaf298d050e4395d6UL,
            /*4*/ 0xc2781f49ffcfa6d4UL,
            /*5*/ 0xd7e77a8f87daf7faUL,
            /*6*/ 0xefb3ab16c59b14a0UL,
            /*7*/ 0x850fadc09923329cUL,
            /*8*/ 0x93ba47c980e98cdeUL,
            /*9*/ 0xa402b9c5a8d3a6e6UL,
            /*10*/ 0xb616a12b7fe617a8UL,
            /*11*/ 0xca28a291859bbf90UL,
            /*12*/ 0xe070f78d39275566UL,
            /*13*/ 0xf92e0c3537826140UL,
            /*14*/ 0x8a5296ffe33cc92cUL,
            /*15*/ 0x9991a6f3d6bf1762UL,
            /*16*/ 0xaa7eebfb9df9de8aUL,
            /*17*/ 0xbd49d14aa79dbc7eUL,
            /*18*/ 0xd226fc195c6a2f88UL,
            /*19*/ 0xe950df20247c83f8UL,
            /*20*/ 0x81842f29f2cce373UL,
            /*21*/ 0x8fcac257558ee4e2UL,

            // powers of 0.1^16
            /*1*/ 0xe69594bec44de160UL,
            /*2*/ 0xcfb11ead453994c3UL,
            /*3*/ 0xbb127c53b17ec165UL,
            /*4*/ 0xa87fea27a539e9b3UL,
            /*5*/ 0x97c560ba6b0919b5UL,
            /*6*/ 0x88b402f7fd7553abUL,
            /*7*/ 0xf64335bcf065d3a0UL,
            /*8*/ 0xddd0467c64bce4c4UL,
            /*9*/ 0xc7caba6e7c5382edUL,
            /*10*/ 0xb3f4e093db73a0b7UL,
            /*11*/ 0xa21727db38cb0053UL,
            /*12*/ 0x91ff83775423cc29UL,
            /*13*/ 0x8380dea93da4bc82UL,
            /*14*/ 0xece53cec4a314f00UL,
            /*15*/ 0xd5605fcdcf32e217UL,
            /*16*/ 0xc0314325637a1978UL,
            /*17*/ 0xad1c8eab5ee43ba2UL,
            /*18*/ 0x9becce62836ac5b0UL,
            /*19*/ 0x8c71dcd9ba0b495cUL,
            /*20*/ 0xfd00b89747823938UL,
            /*21*/ 0xe3e27a444d8d991aUL,
        };

        static readonly ushort[] rgexp64Power10By16 = {
            // exponents for both powers of 10^16 and 0.1^16
            /*1*/ 54,
            /*2*/ 107,
            /*3*/ 160,
            /*4*/ 213,
            /*5*/ 266,
            /*6*/ 319,
            /*7*/ 373,
            /*8*/ 426,
            /*9*/ 479,
            /*10*/ 532,
            /*11*/ 585,
            /*12*/ 638,
            /*13*/ 691,
            /*14*/ 745,
            /*15*/ 798,
            /*16*/ 851,
            /*17*/ 904,
            /*18*/ 957,
            /*19*/ 1010,
            /*20*/ 1064,
            /*21*/ 1117,
        };

        private unsafe static void NumberToDouble(ref NumberBuffer number, ref double value)
        {
            ulong val;
            char* src = number.digits;
            Debug.Assert(src != null, "");

            int total = WCharLen(src);
            int remaining = total;

            // skip the leading zeros
            while (*src == '0')
            {
                remaining--;
                src++;
            }

            if (remaining == 0)
            {
                val = 0;
                goto done;
            }

            int count = remaining < 9 ? remaining : 9;
            remaining -= count;

            val = (ulong)DigitsToInt(src, count);

            if (remaining > 0)
            {
                count = remaining < 9 ? remaining : 9;
                remaining -= count;

                // get the denormalized power of 10
                uint mult = (uint)(rgval64Power10[count - 1] >> (64 - rgexp64Power10[count - 1]));
                val = (val * mult) + (ulong)DigitsToInt(src + 9, count);
            }

            int scale = number.scale - (total - remaining);
            int absscale = scale < 0 ? scale * -1 : scale;
            if (absscale >= 22 * 16)
            {
                // overflow / underflow
                val = scale > 0 ? 0x7FF0000000000000UL : 0UL;
                goto done;
            }

            int exp = 64;

            // normalize the mantissa
            if ((val & 0xFFFFFFFF00000000UL) == 0) { val <<= 32; exp -= 32; }
            if ((val & 0xFFFF000000000000UL) == 0) { val <<= 16; exp -= 16; }
            if ((val & 0xFF00000000000000UL) == 0) { val <<= 8; exp -= 8; }
            if ((val & 0xF000000000000000UL) == 0) { val <<= 4; exp -= 4; }
            if ((val & 0xC000000000000000UL) == 0) { val <<= 2; exp -= 2; }
            if ((val & 0x8000000000000000UL) == 0) { val <<= 1; exp -= 1; }

            int index = absscale & 15;
            if (index != 0)
            {
                int multexp = rgexp64Power10[index - 1];
                // the exponents are shared between the inverted and regular table
                exp += scale < 0 ? (-multexp + 1) : multexp;

                ulong multval = rgval64Power10[index + (scale < 0 ? 15 : 0) - 1];
                val = Mul64Lossy(val, multval, ref exp);
            }

            index = absscale >> 4;
            if (index != 0)
            {
                int multexp = rgexp64Power10By16[index - 1];
                // the exponents are shared between the inverted and regular table
                exp += scale < 0 ? (-multexp + 1) : multexp;

                ulong multval = rgval64Power10By16[index + (scale < 0 ? 21 : 0) - 1];
                val = Mul64Lossy(val, multval, ref exp);
            }

            // round & scale down
            if ((val & (1 << 10)) != 0)
            {
                // IEEE round to even
                ulong tmp = val + ((1UL << 10) - 1) + (((uint)val >> 11) & 1);
                if (tmp < val)
                {
                    // overflow
                    tmp = (tmp >> 1) | 0x8000000000000000UL;
                    exp++;
                }
                val = tmp;
            }

            // return the exponent to a biased state
            exp += 0x3FE;

            // handle overflow, underflow, "Epsilon - 1/2 Epsilon", denormalized, and the normal case
            if (exp <= 0)
            {
                if (exp == -52 && (val >= 0x8000000000000058UL))
                {
                    // round X where {Epsilon > X >= 2.470328229206232730000000E-324} up to Epsilon (instead of down to zero)
                    val = 0x0000000000000001UL;
                }
                else if (exp <= -52)
                {
                    // underflow
                    val = 0;
                }
                else
                {
                    // denormalized
                    val >>= (-exp + 11 + 1);
                }
            }
            else if (exp >= 0x7ff)
            {
                // overflow
                val = 0x7FF0000000000000UL;
            }
            else
            {
                // normal positive exponent case
                val = ((ulong)exp << 52) + ((val >> 11) & 0x000FFFFFFFFFFFFFUL);
            }

        done:
            if (number.sign)
                val |= 0x8000000000000000UL;

            value = UInt64ToDouble(val);
        }

        private static bool NumberBufferToDouble(ref NumberBuffer number, ref double value)
        {
            double d = 0d;
            NumberToDouble(ref number, ref d);
            var decomposeDouble = new FPDOUBLE(d);
            int e = decomposeDouble.exp;
            ulong fmnt = decomposeDouble.mantissa;
            if (e == 0x7ff)
                return false;

            if (e == 0 && fmnt == 0)
                d = 0d;

            value = d;
            return true;
        }

        private static unsafe int WCharLen(char* p)
        {
            int total = 0;
            while (*p != '\0')
            {
                total++;
                p++;
            }

            return total;
        }

        private static unsafe int DigitsToInt(char* p, int count)
        {
            char* end = p + count;
            int res = *p - '0';
            for (p = p + 1; p < end; p++)
                res = 10 * res + *p - '0';

            return res;
        }

        // multiply two numbers in the internal integer representation
        private static unsafe ulong Mul64Lossy(ulong a, ulong b, ref int pexp)
        {
            // it's ok to losse some precision here - Mul64 will be called
            // at most twice during the conversion, so the error won't propagate
            // to any of the 53 significant bits of the result
            ulong a_hi = (a >> 32); uint a_lo = (uint)a;
            ulong b_hi = (b >> 32); uint b_lo = (uint)b;

            var result = a_hi * b_hi;

            // save some multiplications if lo-parts aren't big enough to produce carry
            // (hi-parts will be always big enough, since a and b are normalized)

            if ((b_lo & 0xFFFF0000) != 0)
                result += (a_hi * b_lo) >> 32;

            if ((a_lo & 0xFFFF0000) != 0)
                result += (a_lo * b_hi) >> 32;

            // normalize
            if ((result & 0x8000000000000000UL) == 0) { result <<= 1; pexp--; }

            return result;
        }

        private static unsafe ulong DoubleToUInt64(double d)
            => *((ulong*)&d);

        private static unsafe double UInt64ToDouble(ulong i)
            => *((double*)&i);

        struct FPDOUBLE
        {
            private readonly ulong value;

            public FPDOUBLE(double value)
            {
                this.value = DoubleToUInt64(value);
            }

            public int sign
                => (int)(value >> 63);

            public int exp
                => (int)((value >> 52) & 0x7ffUL);

            public ulong mantissa
                => value & 0xfffffffffffffUL;
        }

        //private static ulong DoubleToUInt64(double d)
        //    => new DoubleUInt64Union { Double = d }.UInt64;

        //private static double UInt64ToDouble(ulong i)
        //    => new DoubleUInt64Union { UInt64 = i }.Double;

        //// https://stackoverflow.com/a/4475653/1028452
        //[Runtime.InteropServices.StructLayout(Runtime.InteropServices.LayoutKind.Explicit)]
        //struct DoubleUInt64Union
        //{
        //    [Runtime.InteropServices.FieldOffset(0)]
        //    public ulong UInt64;
        //    [Runtime.InteropServices.FieldOffset(0)]
        //    public double Double;
        //}
    }
}
