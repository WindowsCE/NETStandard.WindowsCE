// Ref: https://github.com/conceptdev/Facebook/blob/master/iOS/Newtonsoft.Json/DateTimeOffset.cs
// Ref: https://github.com/dotnet/coreclr/blob/master/src/mscorlib/src/System/DateTimeOffset.cs
using System.Globalization;
using System.Runtime.Serialization;

using System;

#if NET35_CF
namespace System
#else
namespace Mock.System
#endif
{
    [Serializable]
    public struct DateTimeOffset
        : IComparable, IFormattable,
        IComparable<DateTimeOffset>, IEquatable<DateTimeOffset>,
        ISerializable, IDeserializationCallback
    {
        private DateTime _dateTime;
        private short _offsetMinutes;
        internal const long MaxOffset = 0x7558bdb000L;
        public static readonly DateTimeOffset MaxValue;
        internal const long MinOffset = -504000000000L;
        public static readonly DateTimeOffset MinValue;

        static DateTimeOffset()
        {
            MinValue = new DateTimeOffset(0L, TimeSpan.Zero);
            MaxValue = new DateTimeOffset(0x2bca2875f4373fffL, TimeSpan.Zero);
        }

        public DateTimeOffset(DateTime dateTime)
        {
            TimeSpan utcOffset;
            if (dateTime.Kind != DateTimeKind.Utc)
            {
                utcOffset = TimeZone.CurrentTimeZone.GetUtcOffset(dateTime);
            }
            else
            {
                utcOffset = new TimeSpan(0L);
            }
            _offsetMinutes = ValidateOffset(utcOffset);
            _dateTime = ValidateDate(dateTime, utcOffset);
        }

        public DateTimeOffset(DateTime dateTime, TimeSpan offset)
        {
            if (dateTime.Kind == DateTimeKind.Local)
            {
                if (offset != TimeZone.CurrentTimeZone.GetUtcOffset(dateTime))
                {
                    throw new ArgumentException("The UTC Offset of the local dateTime parameter does not match the offset argument.", "offset");
                }
            }
            else if ((dateTime.Kind == DateTimeKind.Utc) && (offset != TimeSpan.Zero))
            {
                throw new ArgumentException("The UTC Offset for Utc DateTime instances must be 0.", "offset");
            }
            _offsetMinutes = ValidateOffset(offset);
            _dateTime = ValidateDate(dateTime, offset);
        }

        public DateTimeOffset(long ticks, TimeSpan offset)
        {
            _offsetMinutes = ValidateOffset(offset);
            DateTime dateTime = new DateTime(ticks);
            _dateTime = ValidateDate(dateTime, offset);
        }

        public DateTimeOffset(int year, int month, int day, int hour, int minute, int second, TimeSpan offset)
        {
            _offsetMinutes = ValidateOffset(offset);
            _dateTime = ValidateDate(new DateTime(year, month, day, hour, minute, second), offset);
        }

        public DateTimeOffset(int year, int month, int day, int hour, int minute, int second, int millisecond, TimeSpan offset)
        {
            _offsetMinutes = ValidateOffset(offset);
            _dateTime = ValidateDate(new DateTime(year, month, day, hour, minute, second, millisecond), offset);
        }

        public DateTimeOffset(int year, int month, int day, int hour, int minute, int second, int millisecond, Calendar calendar, TimeSpan offset)
        {
            _offsetMinutes = ValidateOffset(offset);
            _dateTime = ValidateDate(new DateTime(year, month, day, hour, minute, second, millisecond, calendar), offset);
        }

        public DateTimeOffset Add(TimeSpan timeSpan)
        {
            return new DateTimeOffset(ClockDateTime.Add(timeSpan), Offset);
        }

        public DateTimeOffset AddDays(double days)
        {
            return new DateTimeOffset(ClockDateTime.AddDays(days), Offset);
        }

        public DateTimeOffset AddHours(double hours)
        {
            return new DateTimeOffset(ClockDateTime.AddHours(hours), Offset);
        }

        public DateTimeOffset AddMilliseconds(double milliseconds)
        {
            return new DateTimeOffset(ClockDateTime.AddMilliseconds(milliseconds), Offset);
        }

        public DateTimeOffset AddMinutes(double minutes)
        {
            return new DateTimeOffset(ClockDateTime.AddMinutes(minutes), Offset);
        }

        public DateTimeOffset AddMonths(int months)
        {
            return new DateTimeOffset(ClockDateTime.AddMonths(months), Offset);
        }

        public DateTimeOffset AddSeconds(double seconds)
        {
            return new DateTimeOffset(ClockDateTime.AddSeconds(seconds), Offset);
        }

        public DateTimeOffset AddTicks(long ticks)
        {
            return new DateTimeOffset(ClockDateTime.AddTicks(ticks), Offset);
        }

        public DateTimeOffset AddYears(int years)
        {
            return new DateTimeOffset(ClockDateTime.AddYears(years), Offset);
        }

        public static int Compare(DateTimeOffset first, DateTimeOffset second)
        {
            return DateTime.Compare(first.UtcDateTime, second.UtcDateTime);
        }

        public int CompareTo(DateTimeOffset other)
        {
            DateTime utcDateTime = other.UtcDateTime;
            DateTime time2 = UtcDateTime;
            if (time2 > utcDateTime)
            {
                return 1;
            }
            if (time2 < utcDateTime)
            {
                return -1;
            }
            return 0;
        }

        public bool Equals(DateTimeOffset other)
        {
            return UtcDateTime.Equals(other.UtcDateTime);
        }

        public override bool Equals(object obj)
        {
            if (obj is DateTimeOffset)
            {
                DateTimeOffset offset = (DateTimeOffset)obj;
                return UtcDateTime.Equals(offset.UtcDateTime);
            }
            return false;
        }

        public static bool Equals(DateTimeOffset first, DateTimeOffset second)
        {
            return DateTime.Equals(first.UtcDateTime, second.UtcDateTime);
        }

        public bool EqualsExact(DateTimeOffset other)
        {
            return (((ClockDateTime == other.ClockDateTime) && (Offset == other.Offset)) && (ClockDateTime.Kind == other.ClockDateTime.Kind));
        }

        public static DateTimeOffset FromFileTime(long fileTime)
        {
            return new DateTimeOffset(DateTime.FromFileTime(fileTime));
        }

        public override int GetHashCode()
        {
            return UtcDateTime.GetHashCode();
        }

        public static DateTimeOffset operator +(DateTimeOffset dateTimeTz, TimeSpan timeSpan)
        {
            return new DateTimeOffset(dateTimeTz.ClockDateTime + timeSpan, dateTimeTz.Offset);
        }

        public static bool operator ==(DateTimeOffset left, DateTimeOffset right)
        {
            return (left.UtcDateTime == right.UtcDateTime);
        }

        public static bool operator >(DateTimeOffset left, DateTimeOffset right)
        {
            return (left.UtcDateTime > right.UtcDateTime);
        }

        public static bool operator >=(DateTimeOffset left, DateTimeOffset right)
        {
            return (left.UtcDateTime >= right.UtcDateTime);
        }

        public static implicit operator DateTimeOffset(DateTime dateTime)
        {
            return new DateTimeOffset(dateTime);
        }

        public static bool operator !=(DateTimeOffset left, DateTimeOffset right)
        {
            return (left.UtcDateTime != right.UtcDateTime);
        }

        public static bool operator <(DateTimeOffset left, DateTimeOffset right)
        {
            return (left.UtcDateTime < right.UtcDateTime);
        }

        public static bool operator <=(DateTimeOffset left, DateTimeOffset right)
        {
            return (left.UtcDateTime <= right.UtcDateTime);
        }

        public static TimeSpan operator -(DateTimeOffset left, DateTimeOffset right)
        {
            return (TimeSpan)(left.UtcDateTime - right.UtcDateTime);
        }

        public static DateTimeOffset operator -(DateTimeOffset dateTimeTz, TimeSpan timeSpan)
        {
            return new DateTimeOffset(dateTimeTz.ClockDateTime - timeSpan, dateTimeTz.Offset);
        }

        public static DateTimeOffset Parse(string input)
        {
            return new DateTimeOffset(DateTime.Parse(input, DateTimeFormatInfo.CurrentInfo, DateTimeStyles.None));
        }

        public static DateTimeOffset Parse(string input, IFormatProvider formatProvider)
        {
            return Parse(input, formatProvider, DateTimeStyles.None);
        }

        public static DateTimeOffset Parse(string input, IFormatProvider formatProvider, DateTimeStyles styles)
        {
            styles = ValidateStyles(styles, "styles");
            return new DateTimeOffset(DateTime.Parse(input, DateTimeFormatInfo.GetInstance(formatProvider), styles));
        }

        public static DateTimeOffset ParseExact(string input, string format, IFormatProvider formatProvider)
        {
            return ParseExact(input, format, formatProvider, DateTimeStyles.None);
        }

        public static DateTimeOffset ParseExact(string input, string format, IFormatProvider formatProvider, DateTimeStyles styles)
        {
            styles = ValidateStyles(styles, "styles");
            return new DateTimeOffset(DateTime.ParseExact(input, format, DateTimeFormatInfo.GetInstance(formatProvider), styles));
        }

        public static DateTimeOffset ParseExact(string input, string[] formats, IFormatProvider formatProvider, DateTimeStyles styles)
        {
            styles = ValidateStyles(styles, "styles");
            return new DateTimeOffset(DateTime.ParseExact(input, formats, DateTimeFormatInfo.GetInstance(formatProvider), styles));
        }

        public TimeSpan Subtract(DateTimeOffset value)
        {
            return UtcDateTime.Subtract(value.UtcDateTime);
        }

        public DateTimeOffset Subtract(TimeSpan value)
        {
            return new DateTimeOffset(ClockDateTime.Subtract(value), Offset);
        }

        int IComparable.CompareTo(object obj)
        {
            if (obj == null)
            {
                return 1;
            }
            if (!(obj is DateTimeOffset))
            {
                throw new ArgumentException("Object must be of type DateTimeOffset.");
            }
            DateTimeOffset offset = (DateTimeOffset)obj;
            DateTime utcDateTime = offset.UtcDateTime;
            DateTime time2 = UtcDateTime;
            if (time2 > utcDateTime)
            {
                return 1;
            }
            if (time2 < utcDateTime)
            {
                return -1;
            }
            return 0;
        }

        public long ToFileTime()
        {
            return UtcDateTime.ToFileTime();
        }

        public DateTimeOffset ToLocalTime()
        {
            return new DateTimeOffset(UtcDateTime.ToLocalTime());
        }

        public DateTimeOffset ToOffset(TimeSpan offset)
        {
            DateTime time = _dateTime + offset;
            return new DateTimeOffset(time.Ticks, offset);
        }

        public override string ToString()
        {
            return ToString("G K", DateTimeFormatInfo.CurrentInfo);
        }

        public string ToString(string format)
        {
            return ToString(format, DateTimeFormatInfo.CurrentInfo);
        }

        public string ToString(IFormatProvider formatProvider)
        {
            return ToString("G K", DateTimeFormatInfo.CurrentInfo);
        }

        public string ToString(string format, IFormatProvider formatProvider)
        {
            if (!string.IsNullOrEmpty(format))
            {
                format = format.Replace("K", "zzz");
                format = format.Replace("zzz", Offset.Hours.ToString("+00;-00", CultureInfo.InvariantCulture) + ":" + Offset.Minutes.ToString("00;00", CultureInfo.InvariantCulture));
                format = format.Replace("zz", Offset.Hours.ToString("+00;-00", CultureInfo.InvariantCulture));
                format = format.Replace("z", Offset.Hours.ToString("+0;-0", CultureInfo.InvariantCulture));
            }

            return ClockDateTime.ToString(format, formatProvider);
        }

        public DateTimeOffset ToUniversalTime()
        {
            return new DateTimeOffset(UtcDateTime);
        }

        private static DateTime ValidateDate(DateTime dateTime, TimeSpan offset)
        {
            long ticks = dateTime.Ticks - offset.Ticks;
            if ((ticks < 0L) || (ticks > 0x2bca2875f4373fffL))
            {
                throw new ArgumentOutOfRangeException("offset", "The UTC time represented when the offset is applied must be between year 0 and 10,000.");
            }
            return new DateTime(ticks, DateTimeKind.Unspecified);
        }

        private static short ValidateOffset(TimeSpan offset)
        {
            long ticks = offset.Ticks;
            if ((ticks % 0x23c34600L) != 0L)
            {
                throw new ArgumentException("Offset must be specified in whole minutes.", "offset");
            }
            if ((ticks < -504000000000L) || (ticks > 0x7558bdb000L))
            {
                throw new ArgumentOutOfRangeException("offset", "Offset must be within plus or minus 14 hours.");
            }
            return (short)(offset.Ticks / 0x23c34600L);
        }

        private static DateTimeStyles ValidateStyles(DateTimeStyles style, string parameterName)
        {
            DateTimeStyles result;
            int error;
            if (InternalTryValidateStyles(style, out result, out error))
                return result;

            switch (error)
            {
                case 1:
                    throw new ArgumentException("An undefined DateTimeStyles value is being used.", parameterName);
                case 2:
                    throw new ArgumentException("The DateTimeStyles values AssumeLocal and AssumeUniversal cannot be used together.", parameterName);
                case 3:
                    throw new ArgumentException("The DateTimeStyles value 'NoCurrentDateDefault' is not allowed when parsing DateTimeOffset.", parameterName);
                default:
                    throw new ArgumentException("Unexpected error validating DateTimeStyles value");
            }
        }

        private static bool InternalTryValidateStyles(DateTimeStyles style, out DateTimeStyles result, out int error)
        {
            result = 0;
            if ((style & ~(DateTimeStyles.RoundtripKind | DateTimeStyles.AssumeUniversal | DateTimeStyles.AssumeLocal | DateTimeStyles.AdjustToUniversal | DateTimeStyles.NoCurrentDateDefault | DateTimeStyles.AllowWhiteSpaces)) != DateTimeStyles.None)
            {
                error = 1;
                return false;
            }
            if (((style & DateTimeStyles.AssumeLocal) != DateTimeStyles.None) && ((style & DateTimeStyles.AssumeUniversal) != DateTimeStyles.None))
            {
                error = 2;
                return false;
            }
            if ((style & DateTimeStyles.NoCurrentDateDefault) != DateTimeStyles.None)
            {
                error = 3;
                return false;
            }

            style &= ~DateTimeStyles.RoundtripKind;
            style &= ~DateTimeStyles.AssumeLocal;
            result = style;
            error = 0;
            return true;
        }

        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
                throw new ArgumentNullException(nameof(info));

            info.AddValue("DateTime", _dateTime);
            info.AddValue("OffsetMinutes", _offsetMinutes);
        }

        void IDeserializationCallback.OnDeserialization(object sender)
        {
            try
            {
                _offsetMinutes = ValidateOffset(Offset);
                _dateTime = ValidateDate(ClockDateTime, Offset);
            }
            catch (ArgumentException e)
            {
                throw new SerializationException("Invalid deserialized data", e);
            }
        }

        DateTimeOffset(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
                throw new ArgumentNullException(nameof(info));

            _dateTime = info.GetDateTime("DateTime");
            _offsetMinutes = info.GetInt16("OffsetMinutes");
        }

        private DateTime ClockDateTime
        {
            get
            {
                DateTime time = _dateTime + Offset;
                return new DateTime(time.Ticks, DateTimeKind.Unspecified);
            }
        }

        public DateTime Date
        {
            get
            {
                return ClockDateTime.Date;
            }
        }

        public DateTime DateTime
        {
            get
            {
                return ClockDateTime;
            }
        }

        public int Day
        {
            get
            {
                return ClockDateTime.Day;
            }
        }

        public DayOfWeek DayOfWeek
        {
            get
            {
                return ClockDateTime.DayOfWeek;
            }
        }

        public int DayOfYear
        {
            get
            {
                return ClockDateTime.DayOfYear;
            }
        }

        public int Hour
        {
            get
            {
                return ClockDateTime.Hour;
            }
        }

        public DateTime LocalDateTime
        {
            get
            {
                return UtcDateTime.ToLocalTime();
            }
        }

        public int Millisecond
        {
            get
            {
                return ClockDateTime.Millisecond;
            }
        }

        public int Minute
        {
            get
            {
                return ClockDateTime.Minute;
            }
        }

        public int Month
        {
            get
            {
                return ClockDateTime.Month;
            }
        }

        public static DateTimeOffset Now
        {
            get
            {
                return new DateTimeOffset(DateTime.Now);
            }
        }

        public TimeSpan Offset
        {
            get
            {
                return new TimeSpan(0, _offsetMinutes, 0);
            }
        }

        public int Second
        {
            get
            {
                return ClockDateTime.Second;
            }
        }

        public long Ticks
        {
            get
            {
                return ClockDateTime.Ticks;
            }
        }

        public TimeSpan TimeOfDay
        {
            get
            {
                return ClockDateTime.TimeOfDay;
            }
        }

        public DateTime UtcDateTime
        {
            get
            {
                return DateTime.SpecifyKind(_dateTime, DateTimeKind.Utc);
            }
        }

        public static DateTimeOffset UtcNow
        {
            get
            {
                return new DateTimeOffset(DateTime.UtcNow);
            }
        }

        public long UtcTicks
        {
            get
            {
                return UtcDateTime.Ticks;
            }
        }

        public int Year
        {
            get
            {
                return ClockDateTime.Year;
            }
        }

        public static bool TryParse(string input, out DateTimeOffset result)
        {
            result = MinValue;
            DateTime dtParsed;
            if (!DateTime2.TryParse(input, DateTimeFormatInfo.CurrentInfo, DateTimeStyles.None, out dtParsed))
                return false;

            result = new DateTimeOffset(dtParsed);
            return true;
        }

        public static bool TryParse(string input, IFormatProvider formatProvider, DateTimeStyles styles, out DateTimeOffset result)
        {
            result = MinValue;
            DateTimeStyles valStyle;
            int error;
            if (!InternalTryValidateStyles(styles, out valStyle, out error))
                return false;

            styles = valStyle;
            DateTime dtParsed;
            if (!DateTime2.TryParse(input, DateTimeFormatInfo.GetInstance(formatProvider), styles, out dtParsed))
                return false;

            result = new DateTimeOffset(dtParsed);
            return true;
        }

        public static bool TryParseExact(string input, string format, IFormatProvider formatProvider, DateTimeStyles styles, out DateTimeOffset result)
        {
            result = MinValue;
            DateTimeStyles valStyle;
            int error;
            if (!InternalTryValidateStyles(styles, out valStyle, out error))
                return false;

            styles = valStyle;
            DateTime dtParsed;
            if (!DateTime2.TryParseExact(input, format, DateTimeFormatInfo.GetInstance(formatProvider), styles, out dtParsed))
                return false;

            result = new DateTimeOffset(dtParsed);
            return true;
        }

        public static bool TryParseExact(string input, string[] formats, IFormatProvider formatProvider, DateTimeStyles styles, out DateTimeOffset result)
        {
            result = MinValue;
            DateTimeStyles valStyle;
            int error;
            if (!InternalTryValidateStyles(styles, out valStyle, out error))
                return false;

            styles = valStyle;
            DateTime dtParsed;
            if (!DateTime2.TryParseExact(input, formats, DateTimeFormatInfo.GetInstance(formatProvider), styles, out dtParsed))
                return false;

            result = new DateTimeOffset(dtParsed);
            return true;
        }
    }
}
