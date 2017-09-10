// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.
//
// File: decimal.cpp
//

// References:
// - https://github.com/dotnet/coreclr/blob/master/src/classlibnative/bcltype/decimal.cpp
// - https://github.com/dotnet/corert/blob/master/src/System.Private.CoreLib/src/System/Decimal.DecCalc.cs

using System.Diagnostics;

namespace System
{
    partial class Number
    {
        private const int DECIMAL_PRECISION = 29;

        private static unsafe bool NumberBufferToDecimal(NumberBuffer number, ref decimal value)
        {
            uint low = 0, mid = 0, high = 0;
            byte scale;
            char* p = number.digits;
            Debug.Assert(p != null, "");
            int e = number.scale;
            if (*p == 0)
            {
                // To avoid risking an app-compat issue with pre 4.5 (where some app was illegally using Reflection to examine the internal scale bits), we'll only force
                // the scale to 0 if the scale was previously positive
                if (e > 0)
                    e = 0;
            }
            else
            {
                if (e > DECIMAL_PRECISION)
                    return false;

                while ((e > 0 || (*p != 0 && e > -28)) &&
                    (high < 0x19999999 || (high == 0x19999999 &&
                    (mid < 0x99999999 || (mid == 0x99999999 &&
                    (low < 0x99999999 || (low == 0x99999999 && *p <= '5')))))))
                {
                    DecMul10(ref low, ref mid, ref high);
                    if (*p != 0)
                        DecAddInt32(ref low, ref mid, ref high, (uint)(*p++ - '0'));
                    e--;
                }
                if (*p++ >= '5')
                {
                    bool round = true;
                    if (*(p - 1) == '5' && *(p - 2) % 2 == 0)
                    {
                        // Check if previous digit is even, only if the when we are unsure whether hows to do Banker's rounding
                        // For digits > 5 we will be roundinp up anyway.
                        int count = 20; // Look at the next 20 digits to check to round
                        while (*p == '0' && count != 0)
                        {
                            p++;
                            count--;
                        }
                        if (*p == '\0' || count == 0)
                            round = false;  // Do nothing
                    }

                    if (round)
                    {
                        DecAddInt32(ref low, ref mid, ref high, 1);
                        if ((high | mid | low) == 0)
                        {
                            high = 0x19999999;
                            mid = 0x99999999;
                            low = 0x9999999A;
                            e++;
                        }
                    }
                }
            }

            if (e > 0)
                return false;

            if (e <= -DECIMAL_PRECISION)
            {
                // Parsing a large scale zero can give you more precision than fits in the decimal.
                // This should only happen for actual zeros or very small numbers that round to zero.
                high = 0;
                low = 0;
                mid = 0;
                scale = DECIMAL_PRECISION - 1;
            }
            else
            {
                scale = unchecked((byte)-e);
            }

            unchecked
            {
                var d = new decimal((int)low, (int)mid, (int)high, number.sign, scale);
                value = d;
            }

            return true;
        }

        private static void DecAddInt32(ref uint low, ref uint mid, ref uint high, uint i)
        {
            if (D32AddCarry(ref low, i))
            {
                if (D32AddCarry(ref mid, 1))
                    D32AddCarry(ref high, 1);
            }
        }

        private static bool D32AddCarry(ref uint value, uint i)
        {
            uint v = value;
            uint sum = v + i;
            value = sum;
            return (sum < v) || (sum < i);
        }

        private static void DecMul10(ref uint low, ref uint mid, ref uint high)
        {
            uint dLow = low, dMid = mid, dHigh = high;
            DecShiftLeft(ref low, ref mid, ref high);
            DecShiftLeft(ref low, ref mid, ref high);
            DecAdd(ref low, ref mid, ref high, dLow, dMid, dHigh);
            DecShiftLeft(ref low, ref mid, ref high);
        }

        private static void DecShiftLeft(ref uint low, ref uint mid, ref uint high)
        {
            uint c0 = (low & 0x80000000) != 0 ? 1u : 0u;
            uint c1 = (mid & 0x80000000) != 0 ? 1u : 0u;
            low = low << 1;
            mid = (mid << 1) | c0;
            high = (high << 1) | c1;
        }

        private static void DecAdd(ref uint low, ref uint mid, ref uint high, uint dLow, uint dMid, uint dHigh)
        {
            if (D32AddCarry(ref low, dLow))
            {
                if (D32AddCarry(ref mid, 1))
                    D32AddCarry(ref high, 1);
            }

            if (D32AddCarry(ref mid, dMid))
                D32AddCarry(ref high, 1);

            D32AddCarry(ref high, dHigh);
        }
    }
}
