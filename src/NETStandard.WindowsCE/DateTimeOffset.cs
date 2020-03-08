// Ref: https://github.com/conceptdev/Facebook/blob/master/iOS/Newtonsoft.Json/DateTimeOffset.cs
// Ref: https://github.com/dotnet/coreclr/blob/master/src/mscorlib/src/System/DateTimeOffset.cs
using System.Globalization;
using System.Runtime.Serialization;

using System;
using System.Text.RegularExpressions;

namespace System
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

        private const long UnixEpochTicks = TimeSpan.TicksPerDay * DateTime2.DaysTo1970; // 621,355,968,000,000,000
        private const long UnixEpochSeconds = UnixEpochTicks / TimeSpan.TicksPerSecond; // 62,135,596,800
        private const long UnixEpochMilliseconds = UnixEpochTicks / TimeSpan.TicksPerMillisecond; // 62,135,596,800,000

        internal const long UnixMinSeconds = DateTime2.MinTicks / TimeSpan.TicksPerSecond - UnixEpochSeconds;
        internal const long UnixMaxSeconds = DateTime2.MaxTicks / TimeSpan.TicksPerSecond - UnixEpochSeconds;

        // TODO: Should accept time zone anywhere
        private const string OffsetRegexPattern = "(" +
            @"(?<UTC>Z)|" +
            @"(?<HOUR>[+-]\d{2}):(?<MINUTE>\d{2})|" +
            @"(?<HOUR>[+-]\d{1,2}))$";
        private static readonly Regex OffsetRegex = new Regex(
            OffsetRegexPattern,
            RegexOptions.Singleline | RegexOptions.Compiled | RegexOptions.ExplicitCapture);
        private const string UtcFormatString = "yyyy-MM-dd HH:mm:ss";
        private const string RUtcFormatString = "ddd, dd MMM yyyy HH:mm:ss";

        static DateTimeOffset()
        {
            MinValue = new DateTimeOffset(DateTime2.MinTicks, TimeSpan.Zero);
            MaxValue = new DateTimeOffset(DateTime2.MaxTicks, TimeSpan.Zero);
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
            : this(dateTime, offset, true)
        { }

        private DateTimeOffset(DateTime dateTime, TimeSpan offset, bool validateKind)
        {
            if (validateKind)
            {
                if (dateTime.Kind == DateTimeKind.Local)
                {
                    if (offset != TimeZone.CurrentTimeZone.GetUtcOffset(dateTime))
                        throw new ArgumentException("The UTC Offset of the local dateTime parameter does not match the offset argument.", nameof(offset));
                }
                else if ((dateTime.Kind == DateTimeKind.Utc) && (offset != TimeSpan.Zero))
                {
                    throw new ArgumentException("The UTC Offset for Utc DateTime instances must be 0.", nameof(offset));
                }
            }

            _offsetMinutes = ValidateOffset(offset);

            if (!validateKind)
            {
                if (dateTime.Kind == DateTimeKind.Local)
                {
                    var dtOffset = TimeZone.CurrentTimeZone.GetUtcOffset(dateTime);
                    dateTime = new DateTime(dateTime.Ticks - dtOffset.Ticks, DateTimeKind.Unspecified);
                }
                offset = default(TimeSpan);
            }

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
            => Parse(input, DateTimeFormatInfo.CurrentInfo, DateTimeStyles.None);

        public static DateTimeOffset Parse(string input, IFormatProvider formatProvider)
            => Parse(input, formatProvider, DateTimeStyles.None);

        public static DateTimeOffset Parse(string input, IFormatProvider formatProvider, DateTimeStyles styles)
        {
            styles = ValidateStyles(styles, nameof(styles));

            var inputOffset = ParseOffset(input);
            if (inputOffset.HasValue || (styles & DateTimeStyles.AssumeUniversal) > 0)
                styles |= DateTimeStyles.AdjustToUniversal;

            var dt = DateTime.Parse(input, DateTimeFormatInfo.GetInstance(formatProvider), styles);

            if (!inputOffset.HasValue)
                return new DateTimeOffset(dt);

            return new DateTimeOffset(dt, inputOffset.Value, false);
        }

        public static DateTimeOffset ParseExact(string input, string format, IFormatProvider formatProvider)
            => ParseExact(input, format, formatProvider, DateTimeStyles.None);

        public static DateTimeOffset ParseExact(string input, string format, IFormatProvider formatProvider, DateTimeStyles styles)
        {
            if (format == null)
                throw new ArgumentNullException(nameof(format));

            if (format.Equals("u", StringComparison.Ordinal))
                format = UtcFormatString + "Z";
            else if (format.Equals("r", StringComparison.OrdinalIgnoreCase))
            {
                format = RUtcFormatString + "z";
                int gmtIndex = input.Length - 4;
                if (input.IndexOf(" GMT") == gmtIndex)
                    input = string.Concat(input.Remove(gmtIndex), "+0");
            }

            styles = ValidateStyles(styles, nameof(styles));

            var inputOffset = ParseOffset(input);
            if (inputOffset.HasValue || (styles & DateTimeStyles.AssumeUniversal) > 0)
                styles |= DateTimeStyles.AdjustToUniversal;

            var dt = DateTime.ParseExact(input, format, DateTimeFormatInfo.GetInstance(formatProvider), styles);

            if (!inputOffset.HasValue)
                return new DateTimeOffset(dt);

            return new DateTimeOffset(dt, inputOffset.Value, false);
        }

        public static DateTimeOffset ParseExact(string input, string[] formats, IFormatProvider formatProvider, DateTimeStyles styles)
        {
            if (formats == null)
                throw new ArgumentNullException(nameof(formats));
            if (formats.Length == 0)
                throw new FormatException("The list of formats cannot be empty");

            for (int i = 0; i < formats.Length; i++)
            {
                if (formats[i].Equals("u", StringComparison.Ordinal))
                    formats[i] = UtcFormatString + "Z";
                else if (formats[i].Equals("r", StringComparison.OrdinalIgnoreCase))
                {
                    formats[i] = RUtcFormatString + "z";
                    int gmtIndex = input.Length - 4;
                    if (input.IndexOf(" GMT") == gmtIndex)
                        input = string.Concat(input.Remove(gmtIndex), "+0");
                }
            }

            styles = ValidateStyles(styles, nameof(styles));

            var inputOffset = ParseOffset(input);
            if (inputOffset.HasValue || (styles & DateTimeStyles.AssumeUniversal) > 0)
                styles |= DateTimeStyles.AdjustToUniversal;

            var dt = DateTime.ParseExact(input, formats, DateTimeFormatInfo.GetInstance(formatProvider), styles);

            if (!inputOffset.HasValue)
                return new DateTimeOffset(dt);

            return new DateTimeOffset(dt, inputOffset.Value, false);
        }

        private static TimeSpan? ParseOffset(string input)
        {
            var match = OffsetRegex.Match(input);
            if (!match.Success)
                return null;

            if (match.Groups["UTC"].Success)
                return default(TimeSpan);

            int hour = 0, minute = 0;
            var grpHour = match.Groups["HOUR"];
            var grpMinute = match.Groups["MINUTE"];
            if (grpHour.Success)
                hour = int.Parse(grpHour.Value);
            if (grpMinute.Success)
                minute = int.Parse(grpMinute.Value);

            return new TimeSpan(hour, minute, 0);
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
            => ToString(null, DateTimeFormatInfo.CurrentInfo);

        public string ToString(string format)
            => ToString(format, DateTimeFormatInfo.CurrentInfo);

        public string ToString(IFormatProvider formatProvider)
            => ToString(null, DateTimeFormatInfo.CurrentInfo);

        public string ToString(string format, IFormatProvider formatProvider)
        {
            if (!string.IsNullOrEmpty(format))
            {
                if (format.Equals("u", StringComparison.Ordinal))
                {
                    return string.Concat(
                        _dateTime.ToString(UtcFormatString, formatProvider),
                        "Z");
                }
                else if (format.Equals("r", StringComparison.OrdinalIgnoreCase))
                {
                    return string.Concat(
                        _dateTime.ToString(RUtcFormatString, formatProvider),
                        " GMT");
                }

                if (format.Equals("K", StringComparison.Ordinal))
                    throw new FormatException("The 'K' format specifier cannot appear as the single character in format");

                format = OffsetToString(format);
            }
            else
            {
                return string.Concat(
                    ClockDateTime.ToString(formatProvider),
                    " ",
                    OffsetToString("K"));
            }

            string result = ClockDateTime.ToString(format, formatProvider);
            if (format.Equals("o", StringComparison.OrdinalIgnoreCase) && _dateTime.Kind == DateTimeKind.Unspecified)
                result += OffsetToString("zzz");

            return result;
        }

        private string OffsetToString(string format)
        {
            string dHours = Offset.Hours.ToString("+00;-00", CultureInfo.InvariantCulture);
            string dMinutes = Offset.Minutes.ToString("00;00", CultureInfo.InvariantCulture);
            string sHours = Offset.Hours.ToString("+0;-0", CultureInfo.InvariantCulture);

            format = format.Replace("K", "zzz");
            format = format.Replace("zzz", dHours + ":" + dMinutes);
            format = format.Replace("zz", dHours);
            format = format.Replace("z", sHours);
            return format;
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
                throw new ArgumentOutOfRangeException(nameof(offset), "The UTC time represented when the offset is applied must be between year 0 and 10,000.");
            }
            return new DateTime(ticks, DateTimeKind.Unspecified);
        }

        private static short ValidateOffset(TimeSpan offset)
        {
            long ticks = offset.Ticks;
            if ((ticks % 0x23c34600L) != 0L)
            {
                throw new ArgumentException("Offset must be specified in whole minutes.", nameof(offset));
            }
            if ((ticks < -504000000000L) || (ticks > 0x7558bdb000L))
            {
                throw new ArgumentOutOfRangeException(nameof(offset), "Offset must be within plus or minus 14 hours.");
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
            => TryParse(input, DateTimeFormatInfo.CurrentInfo, DateTimeStyles.None, out result);

        public static bool TryParse(string input, IFormatProvider formatProvider, DateTimeStyles styles, out DateTimeOffset result)
        {
            result = MinValue;
            styles = ValidateStyles(styles, nameof(styles));

            var inputOffset = ParseOffset(input);
            if (inputOffset.HasValue || (styles & DateTimeStyles.AssumeUniversal) > 0)
                styles |= DateTimeStyles.AdjustToUniversal;

            DateTime dtParsed;
            if (!DateTime2.TryParse(input, DateTimeFormatInfo.GetInstance(formatProvider), styles, out dtParsed))
                return false;

            if (inputOffset.HasValue)
                result = new DateTimeOffset(dtParsed, inputOffset.Value, false);
            else
                result = new DateTimeOffset(dtParsed);

            return true;
        }

        public static bool TryParseExact(string input, string format, IFormatProvider formatProvider, DateTimeStyles styles, out DateTimeOffset result)
        {
            if (format == null)
                throw new ArgumentNullException(nameof(format));

            if (format.Equals("u", StringComparison.Ordinal))
                format = UtcFormatString + "Z";
            else if (format.Equals("r", StringComparison.OrdinalIgnoreCase))
            {
                format = RUtcFormatString + "z";
                int gmtIndex = input.Length - 4;
                if (input.IndexOf(" GMT") == gmtIndex)
                    input = string.Concat(input.Remove(gmtIndex), "+0");
            }

            result = MinValue;
            styles = ValidateStyles(styles, nameof(styles));

            var inputOffset = ParseOffset(input);
            if (inputOffset.HasValue || (styles & DateTimeStyles.AssumeUniversal) > 0)
                styles |= DateTimeStyles.AdjustToUniversal;

            DateTime dtParsed;
            if (!DateTime2.TryParseExact(input, format, DateTimeFormatInfo.GetInstance(formatProvider), styles, out dtParsed))
                return false;

            if (inputOffset.HasValue)
                result = new DateTimeOffset(dtParsed, inputOffset.Value, false);
            else
                result = new DateTimeOffset(dtParsed);

            return true;
        }

        public static bool TryParseExact(string input, string[] formats, IFormatProvider formatProvider, DateTimeStyles styles, out DateTimeOffset result)
        {
            if (formats == null)
                throw new ArgumentNullException(nameof(formats));
            if (formats.Length == 0)
                throw new FormatException("The list of formats cannot be empty");

            for (int i = 0; i < formats.Length; i++)
            {
                if (formats[i].Equals("u", StringComparison.Ordinal))
                    formats[i] = UtcFormatString + "Z";
                else if (formats[i].Equals("r", StringComparison.OrdinalIgnoreCase))
                {
                    formats[i] = RUtcFormatString + "z";
                    int gmtIndex = input.Length - 4;
                    if (input.IndexOf(" GMT") == gmtIndex)
                        input = string.Concat(input.Remove(gmtIndex), "+0");
                }
            }

            result = MinValue;
            styles = ValidateStyles(styles, nameof(styles));

            var inputOffset = ParseOffset(input);
            if (inputOffset.HasValue || (styles & DateTimeStyles.AssumeUniversal) > 0)
                styles |= DateTimeStyles.AdjustToUniversal;

            DateTime dtParsed;
            if (!DateTime2.TryParseExact(input, formats, DateTimeFormatInfo.GetInstance(formatProvider), styles, out dtParsed))
                return false;

            if (inputOffset.HasValue)
                result = new DateTimeOffset(dtParsed, inputOffset.Value, false);
            else
                result = new DateTimeOffset(dtParsed);

            return true;
        }

        public static DateTimeOffset FromUnixTimeSeconds(long seconds)
        {
            if (seconds < UnixMinSeconds || seconds > UnixMaxSeconds)
            {
                throw new ArgumentOutOfRangeException(nameof(seconds),
                    $"The valid values should be between {UnixMinSeconds} and {UnixMaxSeconds} inclusive");
            }

            long ticks = seconds * TimeSpan.TicksPerSecond + UnixEpochTicks;
            return new DateTimeOffset(ticks, TimeSpan.Zero);
        }

        public static DateTimeOffset FromUnixTimeMilliseconds(long milliseconds)
        {
            const long MinMilliseconds = DateTime2.MinTicks / TimeSpan.TicksPerMillisecond - UnixEpochMilliseconds;
            const long MaxMilliseconds = DateTime2.MaxTicks / TimeSpan.TicksPerMillisecond - UnixEpochMilliseconds;

            if (milliseconds < MinMilliseconds || milliseconds > MaxMilliseconds)
            {
                throw new ArgumentOutOfRangeException(nameof(milliseconds),
                    $"The valid values should be between {MinMilliseconds} and {MaxMilliseconds} inclusive");
            }

            long ticks = milliseconds * TimeSpan.TicksPerMillisecond + UnixEpochTicks;
            return new DateTimeOffset(ticks, TimeSpan.Zero);
        }

        public long ToUnixTimeSeconds()
        {
            // Truncate sub-second precision before offsetting by the Unix Epoch to avoid
            // the last digit being off by one for dates that result in negative Unix times.
            //
            // For example, consider the DateTimeOffset 12/31/1969 12:59:59.001 +0
            //   ticks            = 621355967990010000
            //   ticksFromEpoch   = ticks - UnixEpochTicks                   = -9990000
            //   secondsFromEpoch = ticksFromEpoch / TimeSpan.TicksPerSecond = 0
            //
            // Notice that secondsFromEpoch is rounded *up* by the truncation induced by integer division,
            // whereas we actually always want to round *down* when converting to Unix time. This happens
            // automatically for positive Unix time values. Now the example becomes:
            //   seconds          = ticks / TimeSpan.TicksPerSecond = 62135596799
            //   secondsFromEpoch = seconds - UnixEpochSeconds      = -1
            //
            // In other words, we want to consistently round toward the time 1/1/0001 00:00:00,
            // rather than toward the Unix Epoch (1/1/1970 00:00:00).
            long seconds = UtcDateTime.Ticks / TimeSpan.TicksPerSecond;
            return seconds - UnixEpochSeconds;
        }

        public long ToUnixTimeMilliseconds()
        {
            // Truncate sub-millisecond precision before offsetting by the Unix Epoch to avoid
            // the last digit being off by one for dates that result in negative Unix times
            long milliseconds = UtcDateTime.Ticks / TimeSpan.TicksPerMillisecond;
            return milliseconds - UnixEpochMilliseconds;
        }
    }
}
