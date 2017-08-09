using System;
using System.Globalization;

#if NET35_CF
namespace System
#else
namespace Mock.System
#endif
{
    public static class DateTime2
    {
        public static readonly DateTime MaxValue = DateTime.MaxValue;
        public static readonly DateTime MinValue = DateTime.MinValue;

        // Number of 100ns ticks per time unit
        private const long TicksPerMillisecond = 10000;
        private const long TicksPerSecond = TicksPerMillisecond * 1000;
        private const long TicksPerMinute = TicksPerSecond * 60;
        private const long TicksPerHour = TicksPerMinute * 60;
        private const long TicksPerDay = TicksPerHour * 24;

        // Number of days in a non-leap year
        private const int DaysPerYear = 365;
        // Number of days in 4 years
        private const int DaysPer4Years = DaysPerYear * 4 + 1;       // 1461
        // Number of days in 100 years
        private const int DaysPer100Years = DaysPer4Years * 25 - 1;  // 36524
        // Number of days in 400 years
        private const int DaysPer400Years = DaysPer100Years * 4 + 1; // 146097

        // Number of days from 1/1/0001 to 12/31/1600
        private const int DaysTo1601 = DaysPer400Years * 4;          // 584388
        // Number of days from 1/1/0001 to 12/30/1899
        private const int DaysTo1899 = DaysPer400Years * 4 + DaysPer100Years * 3 - 367;
        // Number of days from 1/1/0001 to 12/31/1969
        internal const int DaysTo1970 = DaysPer400Years * 4 + DaysPer100Years * 3 + DaysPer4Years * 17 + DaysPerYear; // 719,162
        // Number of days from 1/1/0001 to 12/31/9999
        private const int DaysTo10000 = DaysPer400Years * 25 - 366;  // 3652059

        internal const long MinTicks = 0;
        internal const long MaxTicks = DaysTo10000 * TicksPerDay - 1;

        private const ulong TicksMask = 0x3FFFFFFFFFFFFFFF;
        private const ulong FlagsMask = 0xC000000000000000;
        private const ulong LocalMask = 0x8000000000000000;
        private const long TicksCeiling = 0x4000000000000000;

        public static DateTime Now
            => DateTime.Now;

        public static DateTime Today
            => DateTime.Today;

        public static DateTime UtcNow
            => DateTime.UtcNow;

        public static int Compare(DateTime t1, DateTime t2)
            => DateTime.Compare(t1, t2);

        public static int DaysInMonth(int year, int month)
            => DateTime.DaysInMonth(year, month);

        public static bool Equals(DateTime t1, DateTime t2)
            => DateTime.Equals(t1, t2);

        [Obsolete(Consts.PlatformNotSupportedDescription)]
        public static DateTime FromBinary(long dateData)
        {
            throw new PlatformNotSupportedException();
            //if ((dateData & (unchecked((long)LocalMask))) != 0)
            //{
            //    // Local times need to be adjusted as you move from one time zone to another, 
            //    // just as they are when serializing in text. As such the format for local times
            //    // changes to store the ticks of the UTC time, but with flags that look like a 
            //    // local date.
            //    long ticks = dateData & (unchecked((long)TicksMask));
            //    // Negative ticks are stored in the top part of the range and should be converted back into a negative number
            //    if (ticks > TicksCeiling - TicksPerDay)
            //    {
            //        ticks = ticks - TicksCeiling;
            //    }
            //    // Convert the ticks back to local. If the UTC ticks are out of range, we need to default to
            //    // the UTC offset from MinValue and MaxValue to be consistent with Parse. 
            //    bool isAmbiguousLocalDst = false;
            //    long offsetTicks;
            //    if (ticks < MinTicks)
            //    {
            //        offsetTicks = TimeZoneInfo.GetLocalUtcOffset(DateTime.MinValue, TimeZoneInfoOptions.NoThrowOnInvalidTime).Ticks;
            //    }
            //    else if (ticks > MaxTicks)
            //    {
            //        offsetTicks = TimeZoneInfo.GetLocalUtcOffset(DateTime.MaxValue, TimeZoneInfoOptions.NoThrowOnInvalidTime).Ticks;
            //    }
            //    else
            //    {
            //        // Because the ticks conversion between UTC and local is lossy, we need to capture whether the 
            //        // time is in a repeated hour so that it can be passed to the DateTime constructor.
            //        DateTime utcDt = new DateTime(ticks, DateTimeKind.Utc);
            //        Boolean isDaylightSavings = false;
            //        offsetTicks = TimeZoneInfo.GetUtcOffsetFromUtc(utcDt, TimeZoneInfo.Local, out isDaylightSavings, out isAmbiguousLocalDst).Ticks;
            //    }
            //    ticks += offsetTicks;
            //    // Another behaviour of parsing is to cause small times to wrap around, so that they can be used
            //    // to compare times of day
            //    if (ticks < 0)
            //    {
            //        ticks += TicksPerDay;
            //    }
            //    if (ticks < MinTicks || ticks > MaxTicks)
            //    {
            //        throw new ArgumentException(Environment.GetResourceString("Argument_DateTimeBadBinaryData"), nameof(dateData));
            //    }
            //    return new DateTime(ticks, DateTimeKind.Local, isAmbiguousLocalDst);
            //}
            //else
            //{
            //    return FromBinaryRaw(dateData);
            //}
        }

        // A version of ToBinary that uses the real representation and does not adjust local times. This is needed for
        // scenarios where the serialized data must maintain compatibility
        //private static DateTime FromBinaryRaw(long dateData)
        //{
        //    long ticks = dateData & (long)TicksMask;
        //    if (ticks < MinTicks || ticks > MaxTicks)
        //        throw new ArgumentException("Invalid binary data for DateTime", nameof(dateData));

        //    return new DateTime((ulong)dateData);
        //}

        public static DateTime FromFileTime(long fileTime)
            => DateTime.FromFileTime(fileTime);

        public static DateTime FromFileTimeUtc(long fileTime)
            => DateTime.FromFileTimeUtc(fileTime);

        public static bool IsLeapYear(int year)
            => DateTime.IsLeapYear(year);

        public static DateTime Parse(string s)
            => DateTime.Parse(s);

        public static DateTime Parse(string s, IFormatProvider provider)
            => DateTime.Parse(s, provider);

        public static DateTime Parse(string s, IFormatProvider provider, DateTimeStyles styles)
            => DateTime.Parse(s, provider, styles);

        public static DateTime ParseExact(string s, string format, IFormatProvider provider)
            => DateTime.ParseExact(s, format, provider);

        public static DateTime ParseExact(string s, string format, IFormatProvider provider, DateTimeStyles style)
            => DateTime.ParseExact(s, format, provider, style);

        public static DateTime ParseExact(string s, string[] formats, IFormatProvider provider, DateTimeStyles style)
            => DateTime.ParseExact(s, formats, provider, style);

        public static DateTime SpecifyKind(DateTime value, DateTimeKind kind)
            => DateTime.SpecifyKind(value, kind);

        [Obsolete(Consts.PlatformNotSupportedDescription)]
        public static long ToBinary(this DateTime dt)
        {
            throw new PlatformNotSupportedException();
            //if (dt.Kind == DateTimeKind.Local)
            //{
            //    // Local times need to be adjusted as you move from one time zone to another, 
            //    // just as they are when serializing in text. As such the format for local times
            //    // changes to store the ticks of the UTC time, but with flags that look like a 
            //    // local date.

            //    // To match serialization in text we need to be able to handle cases where
            //    // the UTC value would be out of range. Unused parts of the ticks range are
            //    // used for this, so that values just past max value are stored just past the
            //    // end of the maximum range, and values just below minimum value are stored
            //    // at the end of the ticks area, just below 2^62.
            //    TimeSpan offset = TimeZoneInfo.GetLocalUtcOffset(dt, TimeZoneInfoOptions.NoThrowOnInvalidTime);
            //    long ticks = dt.Ticks;
            //    long storedTicks = ticks - offset.Ticks;
            //    if (storedTicks < 0)
            //    {
            //        storedTicks = TicksCeiling + storedTicks;
            //    }
            //    return storedTicks | (unchecked((long)LocalMask));
            //}
            //else
            //{
            //    return (long)dateData;
            //}
        }
        public static bool TryParse(string s, out DateTime result)
        {
            bool retVal = false;
            try
            {
                result = DateTime.Parse(s);
                retVal = true;
            }
            catch (FormatException) { result = DateTime.MinValue; }
            catch (InvalidCastException) { result = DateTime.MinValue; }

            return retVal;
        }

        public static bool TryParse(string s, IFormatProvider provider, DateTimeStyles styles, out DateTime result)
        {
            bool retVal = false;
            try
            {
                result = DateTime.Parse(s, provider, styles);
                retVal = true;
            }
            catch (FormatException) { result = DateTime.MinValue; }
            catch (InvalidCastException) { result = DateTime.MinValue; }

            return retVal;
        }

        public static bool TryParseExact(string s, string format, IFormatProvider provider, DateTimeStyles style, out DateTime result)
        {
            bool retVal = false;
            try
            {
                result = DateTime.ParseExact(s, format, provider, style);
                retVal = true;
            }
            catch (FormatException) { result = DateTime.MinValue; }
            catch (InvalidCastException) { result = DateTime.MinValue; }

            return retVal;
        }

        public static bool TryParseExact(string s, string[] formats, IFormatProvider provider, DateTimeStyles style, out DateTime result)
        {
            bool retVal = false;
            try
            {
                result = DateTime.ParseExact(s, formats, provider, style);
                retVal = true;
            }
            catch (FormatException) { result = DateTime.MinValue; }
            catch (InvalidCastException) { result = DateTime.MinValue; }

            return retVal;
        }
    }
}
