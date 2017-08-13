// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using DateTimeOffset = Mock.System.DateTimeOffset;

// Regex to replace theories with member data
// \[Theory\]\r\n        \[MemberData\(nameof\(([A-z0-9_]+)\)\)\]\r\n        public static void ([A-z0-9_]+)\(
// \r\n        [TestMethod]\r\n        public void $2_MemberDataTests()\r\n        {\r\n            foreach (var item in $1())\r\n                $2(0, 0d, 0f, 0d, 0, 0);\r\n        }\r\n\r\n        public void $2(

namespace Tests
{
    [TestClass]
    public partial class DateTimeOffsetTests
    {
        [TestMethod]
        public void DateTimeOffset_MaxValue()
        {
            VerifyDateTimeOffset(DateTimeOffset.MaxValue, 9999, 12, 31, 23, 59, 59, 999, TimeSpan.Zero);
        }

        [TestMethod]
        public void DateTimeOffset_MinValue()
        {
            VerifyDateTimeOffset(DateTimeOffset.MinValue, 1, 1, 1, 0, 0, 0, 0, TimeSpan.Zero);
        }

        [TestMethod]
        public void DateTimeOffset_Ctor_Empty()
        {
            VerifyDateTimeOffset(new DateTimeOffset(), 1, 1, 1, 0, 0, 0, 0, TimeSpan.Zero);
            VerifyDateTimeOffset(default(DateTimeOffset), 1, 1, 1, 0, 0, 0, 0, TimeSpan.Zero);
        }

        [TestMethod]
        public void DateTimeOffset_Ctor_DateTime()
        {
            var dateTimeOffset = new DateTimeOffset(new DateTime(2012, 6, 11, 0, 0, 0, 0, DateTimeKind.Utc));
            VerifyDateTimeOffset(dateTimeOffset, 2012, 6, 11, 0, 0, 0, 0, TimeSpan.Zero);

            dateTimeOffset = new DateTimeOffset(new DateTime(1986, 8, 15, 10, 20, 5, 4, DateTimeKind.Local));
            VerifyDateTimeOffset(dateTimeOffset, 1986, 8, 15, 10, 20, 5, 4, null);

            DateTimeOffset today = new DateTimeOffset(DateTime.Today);
            DateTimeOffset now = DateTimeOffset.Now.Date;
            VerifyDateTimeOffset(today, now.Year, now.Month, now.Day, 0, 0, 0, 0, now.Offset);

            today = new DateTimeOffset(new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, now.Second, DateTimeKind.Utc));
            Assert.AreEqual(TimeSpan.Zero, today.Offset);
            Assert.IsFalse(today.UtcDateTime.IsDaylightSavingTime());
        }

        [TestMethod]
        public void DateTimeOffset_Ctor_DateTime_Invalid()
        {
            // DateTime < DateTimeOffset.MinValue
            DateTimeOffset min = DateTimeOffset.MinValue;
            AssertExtensions.Throws<ArgumentOutOfRangeException>(null, () => new DateTimeOffset(new DateTime(min.Year - 1, min.Day, min.Hour, min.Minute, min.Second, min.Millisecond, DateTimeKind.Utc)));
            AssertExtensions.Throws<ArgumentOutOfRangeException>(null, () => new DateTimeOffset(new DateTime(min.Year, min.Month - 1, min.Day, min.Hour, min.Minute, min.Second, min.Millisecond, DateTimeKind.Utc)));
            AssertExtensions.Throws<ArgumentOutOfRangeException>(null, () => new DateTimeOffset(new DateTime(min.Year, min.Month, min.Day - 1, min.Hour, min.Minute, min.Second, min.Millisecond, DateTimeKind.Utc)));
            AssertExtensions.Throws<ArgumentOutOfRangeException>(null, () => new DateTimeOffset(new DateTime(min.Year, min.Month, min.Day, min.Hour - 1, min.Minute, min.Second, min.Millisecond, DateTimeKind.Utc)));
            AssertExtensions.Throws<ArgumentOutOfRangeException>(null, () => new DateTimeOffset(new DateTime(min.Year, min.Month, min.Day, min.Hour, min.Minute - 1, min.Second, min.Millisecond, DateTimeKind.Utc)));
            AssertExtensions.Throws<ArgumentOutOfRangeException>(null, () => new DateTimeOffset(new DateTime(min.Year, min.Month, min.Day, min.Hour, min.Minute, min.Second - 1, min.Millisecond, DateTimeKind.Utc)));
            AssertExtensions.Throws<ArgumentOutOfRangeException>("millisecond", () => new DateTimeOffset(new DateTime(min.Year, min.Month, min.Day, min.Hour, min.Minute, min.Second, min.Millisecond - 1, DateTimeKind.Utc)));

            // DateTime > DateTimeOffset.MaxValue
            DateTimeOffset max = DateTimeOffset.MaxValue;
            AssertExtensions.Throws<ArgumentOutOfRangeException>(null, () => new DateTimeOffset(new DateTime(max.Year + 1, max.Month, max.Day, max.Hour, max.Minute, max.Second, max.Millisecond, DateTimeKind.Utc)));
            AssertExtensions.Throws<ArgumentOutOfRangeException>(null, () => new DateTimeOffset(new DateTime(max.Year, max.Month + 1, max.Day, max.Hour, max.Minute, max.Second, max.Millisecond, DateTimeKind.Utc)));
            AssertExtensions.Throws<ArgumentOutOfRangeException>(null, () => new DateTimeOffset(new DateTime(max.Year, max.Month, max.Day + 1, max.Hour, max.Minute, max.Second, max.Millisecond, DateTimeKind.Utc)));
            AssertExtensions.Throws<ArgumentOutOfRangeException>(null, () => new DateTimeOffset(new DateTime(max.Year, max.Month, max.Day, max.Hour + 1, max.Minute, max.Second, max.Millisecond, DateTimeKind.Utc)));
            AssertExtensions.Throws<ArgumentOutOfRangeException>(null, () => new DateTimeOffset(new DateTime(max.Year, max.Month, max.Day, max.Hour, max.Minute + 1, max.Second, max.Millisecond, DateTimeKind.Utc)));
            AssertExtensions.Throws<ArgumentOutOfRangeException>(null, () => new DateTimeOffset(new DateTime(max.Year, max.Month, max.Day, max.Hour, max.Minute, max.Second + 1, max.Millisecond, DateTimeKind.Utc)));
            AssertExtensions.Throws<ArgumentOutOfRangeException>("millisecond", () => new DateTimeOffset(new DateTime(max.Year, max.Month, max.Day, max.Hour, max.Minute, max.Second, max.Millisecond + 1, DateTimeKind.Utc)));
        }

        [TestMethod]
        public void DateTimeOffset_Ctor_DateTime_TimeSpan()
        {
            var dateTimeOffset = new DateTimeOffset(DateTime.MinValue, TimeSpan.FromHours(-14));
            VerifyDateTimeOffset(dateTimeOffset, 1, 1, 1, 0, 0, 0, 0, TimeSpan.FromHours(-14));

            dateTimeOffset = new DateTimeOffset(DateTime.MaxValue, TimeSpan.FromHours(14));
            VerifyDateTimeOffset(dateTimeOffset, 9999, 12, 31, 23, 59, 59, 999, TimeSpan.FromHours(14));

            dateTimeOffset = new DateTimeOffset(new DateTime(2012, 12, 31, 13, 50, 10), TimeSpan.Zero);
            VerifyDateTimeOffset(dateTimeOffset, 2012, 12, 31, 13, 50, 10, 0, TimeSpan.Zero);
        }

        [TestMethod]
        public void DateTimeOffset_Ctor_DateTime_TimeSpan_Invalid()
        {
            AssertExtensions.Throws<ArgumentException>("offset", () => new DateTimeOffset(DateTime.Now, TimeSpan.FromHours(15))); // Local time and non timezone timespan
            AssertExtensions.Throws<ArgumentException>("offset", () => new DateTimeOffset(DateTime.Now, TimeSpan.FromHours(-15))); // Local time and non timezone timespan

            AssertExtensions.Throws<ArgumentException>("offset", () => new DateTimeOffset(DateTime.UtcNow, TimeSpan.FromHours(1))); // Local time and non zero timespan

            AssertExtensions.Throws<ArgumentException>("offset", () => new DateTimeOffset(DateTime.UtcNow, new TimeSpan(0, 0, 3))); // TimeSpan is not whole minutes
            AssertExtensions.Throws<ArgumentException>("offset", () => new DateTimeOffset(DateTime.UtcNow, new TimeSpan(0, 0, -3))); // TimeSpan is not whole minutes
            AssertExtensions.Throws<ArgumentException>("offset", () => new DateTimeOffset(DateTime.UtcNow, new TimeSpan(0, 0, 0, 0, 3))); // TimeSpan is not whole minutes
            AssertExtensions.Throws<ArgumentException>("offset", () => new DateTimeOffset(DateTime.UtcNow, new TimeSpan(0, 0, 0, 0, -3))); // TimeSpan is not whole minutes

            // DateTime < DateTimeOffset.MinValue
            DateTimeOffset min = DateTimeOffset.MinValue;
            AssertExtensions.Throws<ArgumentOutOfRangeException>(null, () => new DateTimeOffset(new DateTime(min.Year - 1, min.Day, min.Hour, min.Minute, min.Second, min.Millisecond, DateTimeKind.Utc), TimeSpan.Zero));
            AssertExtensions.Throws<ArgumentOutOfRangeException>(null, () => new DateTimeOffset(new DateTime(min.Year, min.Month - 1, min.Day, min.Hour, min.Minute, min.Second, min.Millisecond, DateTimeKind.Utc), TimeSpan.Zero));
            AssertExtensions.Throws<ArgumentOutOfRangeException>(null, () => new DateTimeOffset(new DateTime(min.Year, min.Month, min.Day - 1, min.Hour, min.Minute, min.Second, min.Millisecond, DateTimeKind.Utc), TimeSpan.Zero));
            AssertExtensions.Throws<ArgumentOutOfRangeException>(null, () => new DateTimeOffset(new DateTime(min.Year, min.Month, min.Day, min.Hour - 1, min.Minute, min.Second, min.Millisecond, DateTimeKind.Utc), TimeSpan.Zero));
            AssertExtensions.Throws<ArgumentOutOfRangeException>(null, () => new DateTimeOffset(new DateTime(min.Year, min.Month, min.Day, min.Hour, min.Minute - 1, min.Second, min.Millisecond, DateTimeKind.Utc), TimeSpan.Zero));
            AssertExtensions.Throws<ArgumentOutOfRangeException>(null, () => new DateTimeOffset(new DateTime(min.Year, min.Month, min.Day, min.Hour, min.Minute, min.Second - 1, min.Millisecond, DateTimeKind.Utc), TimeSpan.Zero));
            AssertExtensions.Throws<ArgumentOutOfRangeException>("millisecond", () => new DateTimeOffset(new DateTime(min.Year, min.Month, min.Day, min.Hour, min.Minute, min.Second, min.Millisecond - 1, DateTimeKind.Utc), TimeSpan.Zero));

            // DateTime > DateTimeOffset.MaxValue
            DateTimeOffset max = DateTimeOffset.MaxValue;
            AssertExtensions.Throws<ArgumentOutOfRangeException>(null, () => new DateTimeOffset(new DateTime(max.Year + 1, max.Month, max.Day, max.Hour, max.Minute, max.Second, max.Millisecond, DateTimeKind.Utc), TimeSpan.Zero));
            AssertExtensions.Throws<ArgumentOutOfRangeException>(null, () => new DateTimeOffset(new DateTime(max.Year, max.Month + 1, max.Day, max.Hour, max.Minute, max.Second, max.Millisecond, DateTimeKind.Utc), TimeSpan.Zero));
            AssertExtensions.Throws<ArgumentOutOfRangeException>(null, () => new DateTimeOffset(new DateTime(max.Year, max.Month, max.Day + 1, max.Hour, max.Minute, max.Second, max.Millisecond, DateTimeKind.Utc), TimeSpan.Zero));
            AssertExtensions.Throws<ArgumentOutOfRangeException>(null, () => new DateTimeOffset(new DateTime(max.Year, max.Month, max.Day, max.Hour + 1, max.Minute, max.Second, max.Millisecond, DateTimeKind.Utc), TimeSpan.Zero));
            AssertExtensions.Throws<ArgumentOutOfRangeException>(null, () => new DateTimeOffset(new DateTime(max.Year, max.Month, max.Day, max.Hour, max.Minute + 1, max.Second, max.Millisecond, DateTimeKind.Utc), TimeSpan.Zero));
            AssertExtensions.Throws<ArgumentOutOfRangeException>(null, () => new DateTimeOffset(new DateTime(max.Year, max.Month, max.Day, max.Hour, max.Minute, max.Second + 1, max.Millisecond, DateTimeKind.Utc), TimeSpan.Zero));
            AssertExtensions.Throws<ArgumentOutOfRangeException>("millisecond", () => new DateTimeOffset(new DateTime(max.Year, max.Month, max.Day, max.Hour, max.Minute, max.Second, max.Millisecond + 1, DateTimeKind.Utc), TimeSpan.Zero));

            // Invalid offset
            AssertExtensions.Throws<ArgumentException>("offset", () => new DateTimeOffset(DateTime.Now, TimeSpan.FromTicks(1)));
            AssertExtensions.Throws<ArgumentException>("offset", () => new DateTimeOffset(DateTime.UtcNow, TimeSpan.FromTicks(1)));
        }

        [TestMethod]
        public void DateTimeOffset_Ctor_Long_TimeSpan()
        {
            var expected = new DateTime(1, 2, 3, 4, 5, 6, 7);
            var dateTimeOffset = new DateTimeOffset(expected.Ticks, TimeSpan.Zero);
            VerifyDateTimeOffset(dateTimeOffset, dateTimeOffset.Year, dateTimeOffset.Month, dateTimeOffset.Day, dateTimeOffset.Hour, dateTimeOffset.Minute, dateTimeOffset.Second, dateTimeOffset.Millisecond, TimeSpan.Zero);
        }

        [TestMethod]
        public void DateTimeOffset_Ctor_Long_TimeSpan_Invalid()
        {
            AssertExtensions.Throws<ArgumentException>("offset", () => new DateTimeOffset(0, new TimeSpan(0, 0, 3))); // TimeSpan is not whole minutes
            AssertExtensions.Throws<ArgumentException>("offset", () => new DateTimeOffset(0, new TimeSpan(0, 0, -3))); // TimeSpan is not whole minutes
            AssertExtensions.Throws<ArgumentException>("offset", () => new DateTimeOffset(0, new TimeSpan(0, 0, 0, 0, 3))); // TimeSpan is not whole minutes
            AssertExtensions.Throws<ArgumentException>("offset", () => new DateTimeOffset(0, new TimeSpan(0, 0, 0, 0, -3))); // TimeSpan is not whole minutes

            AssertExtensions.Throws<ArgumentOutOfRangeException>("offset", () => new DateTimeOffset(0, TimeSpan.FromHours(-15))); // TimeZone.Offset > 14
            AssertExtensions.Throws<ArgumentOutOfRangeException>("offset", () => new DateTimeOffset(0, TimeSpan.FromHours(15))); // TimeZone.Offset < -14

            AssertExtensions.Throws<ArgumentOutOfRangeException>("ticks", () => new DateTimeOffset(DateTimeOffset.MinValue.Ticks - 1, TimeSpan.Zero)); // Ticks < DateTimeOffset.MinValue.Ticks
            AssertExtensions.Throws<ArgumentOutOfRangeException>("ticks", () => new DateTimeOffset(DateTimeOffset.MaxValue.Ticks + 1, TimeSpan.Zero)); // Ticks > DateTimeOffset.MaxValue.Ticks
        }

        [TestMethod]
        public void DateTimeOffset_Ctor_Int_Int_Int_Int_Int_Int_Int_TimeSpan()
        {
            var dateTimeOffset = new DateTimeOffset(1973, 10, 6, 14, 30, 0, 500, TimeSpan.Zero);
            VerifyDateTimeOffset(dateTimeOffset, 1973, 10, 6, 14, 30, 0, 500, TimeSpan.Zero);
        }

        [TestMethod]
        public void DateTimeOffset_Ctor_Int_Int_Int_Int_Int_Int_Int_TimeSpan_Invalid()
        {
            AssertExtensions.Throws<ArgumentException>("offset", () => new DateTimeOffset(1, 1, 1, 1, 1, 1, 1, new TimeSpan(0, 0, 3))); // TimeSpan is not whole minutes
            AssertExtensions.Throws<ArgumentException>("offset", () => new DateTimeOffset(1, 1, 1, 1, 1, 1, 1, new TimeSpan(0, 0, -3))); // TimeSpan is not whole minutes
            AssertExtensions.Throws<ArgumentException>("offset", () => new DateTimeOffset(1, 1, 1, 1, 1, 1, 1, new TimeSpan(0, 0, 0, 0, 3))); // TimeSpan is not whole minutes
            AssertExtensions.Throws<ArgumentException>("offset", () => new DateTimeOffset(1, 1, 1, 1, 1, 1, 1, new TimeSpan(0, 0, 0, 0, -3))); // TimeSpan is not whole minutes

            AssertExtensions.Throws<ArgumentOutOfRangeException>("offset", () => new DateTimeOffset(1, 1, 1, 1, 1, 1, 1, TimeSpan.FromHours(-15))); // TimeZone.Offset > 14
            AssertExtensions.Throws<ArgumentOutOfRangeException>("offset", () => new DateTimeOffset(1, 1, 1, 1, 1, 1, 1, TimeSpan.FromHours(15))); // TimeZone.Offset < -14

            // Invalid DateTime
            AssertExtensions.Throws<ArgumentOutOfRangeException>(null, () => new DateTimeOffset(0, 1, 1, 1, 1, 1, 1, TimeSpan.Zero)); // Year < 1
            AssertExtensions.Throws<ArgumentOutOfRangeException>(null, () => new DateTimeOffset(10000, 1, 1, 1, 1, 1, 1, TimeSpan.Zero)); // Year > 9999

            AssertExtensions.Throws<ArgumentOutOfRangeException>(null, () => new DateTimeOffset(1, 0, 1, 1, 1, 1, 1, TimeSpan.Zero)); // Month < 1
            AssertExtensions.Throws<ArgumentOutOfRangeException>(null, () => new DateTimeOffset(1, 13, 1, 1, 1, 1, 1, TimeSpan.Zero)); // Motnh > 23

            AssertExtensions.Throws<ArgumentOutOfRangeException>(null, () => new DateTimeOffset(1, 1, 0, 1, 1, 1, 1, TimeSpan.Zero)); // Day < 1
            AssertExtensions.Throws<ArgumentOutOfRangeException>(null, () => new DateTimeOffset(1, 1, 32, 1, 1, 1, 1, TimeSpan.Zero)); // Day > days in month

            AssertExtensions.Throws<ArgumentOutOfRangeException>(null, () => new DateTimeOffset(1, 1, 1, -1, 1, 1, 1, TimeSpan.Zero)); // Hour < 0
            AssertExtensions.Throws<ArgumentOutOfRangeException>(null, () => new DateTimeOffset(1, 1, 1, 24, 1, 1, 1, TimeSpan.Zero)); // Hour > 23

            AssertExtensions.Throws<ArgumentOutOfRangeException>(null, () => new DateTimeOffset(1, 1, 1, 1, -1, 1, 1, TimeSpan.Zero)); // Minute < 0
            AssertExtensions.Throws<ArgumentOutOfRangeException>(null, () => new DateTimeOffset(1, 1, 1, 1, 60, 1, 1, TimeSpan.Zero)); // Minute > 59

            AssertExtensions.Throws<ArgumentOutOfRangeException>(null, () => new DateTimeOffset(1, 1, 1, 1, 1, -1, 1, TimeSpan.Zero)); // Second < 0
            AssertExtensions.Throws<ArgumentOutOfRangeException>(null, () => new DateTimeOffset(1, 1, 1, 1, 1, 60, 1, TimeSpan.Zero)); // Second > 59

            AssertExtensions.Throws<ArgumentOutOfRangeException>("millisecond", () => new DateTimeOffset(1, 1, 1, 1, 1, 1, -1, TimeSpan.Zero)); // Millisecond < 0
            AssertExtensions.Throws<ArgumentOutOfRangeException>("millisecond", () => new DateTimeOffset(1, 1, 1, 1, 1, 1, 1000, TimeSpan.Zero)); // Millisecond > 999

            // DateTime < DateTimeOffset.MinValue
            DateTimeOffset min = DateTimeOffset.MinValue;
            AssertExtensions.Throws<ArgumentOutOfRangeException>(null, () => new DateTimeOffset(min.Year - 1, min.Month, min.Day, min.Hour, min.Minute, min.Second, min.Millisecond, TimeSpan.Zero));
            AssertExtensions.Throws<ArgumentOutOfRangeException>(null, () => new DateTimeOffset(min.Year, min.Month - 1, min.Day, min.Hour, min.Minute, min.Second, min.Millisecond, TimeSpan.Zero));
            AssertExtensions.Throws<ArgumentOutOfRangeException>(null, () => new DateTimeOffset(min.Year, min.Month, min.Day - 1, min.Hour, min.Minute, min.Second, min.Millisecond, TimeSpan.Zero));
            AssertExtensions.Throws<ArgumentOutOfRangeException>(null, () => new DateTimeOffset(min.Year, min.Month, min.Day, min.Hour - 1, min.Minute, min.Second, min.Millisecond, TimeSpan.Zero));
            AssertExtensions.Throws<ArgumentOutOfRangeException>(null, () => new DateTimeOffset(min.Year, min.Month, min.Day, min.Hour, min.Minute - 1, min.Second, min.Millisecond, TimeSpan.Zero));
            AssertExtensions.Throws<ArgumentOutOfRangeException>(null, () => new DateTimeOffset(min.Year, min.Month, min.Day, min.Hour, min.Minute, min.Second - 1, min.Millisecond, TimeSpan.Zero));
            AssertExtensions.Throws<ArgumentOutOfRangeException>("millisecond", () => new DateTimeOffset(min.Year, min.Month, min.Day, min.Hour, min.Minute, min.Second, min.Millisecond - 1, TimeSpan.Zero));

            // DateTime > DateTimeOffset.MaxValue
            DateTimeOffset max = DateTimeOffset.MaxValue;
            AssertExtensions.Throws<ArgumentOutOfRangeException>(null, () => new DateTimeOffset(max.Year + 1, max.Month, max.Day, max.Hour, max.Minute, max.Second, max.Millisecond, TimeSpan.Zero));
            AssertExtensions.Throws<ArgumentOutOfRangeException>(null, () => new DateTimeOffset(max.Year, max.Month + 1, max.Day + 1, max.Hour, max.Minute, max.Second, max.Millisecond, TimeSpan.Zero));
            AssertExtensions.Throws<ArgumentOutOfRangeException>(null, () => new DateTimeOffset(max.Year, max.Month, max.Day + 1, max.Hour, max.Minute, max.Second, max.Millisecond, TimeSpan.Zero));
            AssertExtensions.Throws<ArgumentOutOfRangeException>(null, () => new DateTimeOffset(max.Year, max.Month, max.Day, max.Hour + 1, max.Minute, max.Second, max.Millisecond, TimeSpan.Zero));
            AssertExtensions.Throws<ArgumentOutOfRangeException>(null, () => new DateTimeOffset(max.Year, max.Month, max.Day, max.Hour, max.Minute + 1, max.Second, max.Millisecond, TimeSpan.Zero));
            AssertExtensions.Throws<ArgumentOutOfRangeException>(null, () => new DateTimeOffset(max.Year, max.Month, max.Day, max.Hour, max.Minute, max.Second + 1, max.Millisecond, TimeSpan.Zero));
            AssertExtensions.Throws<ArgumentOutOfRangeException>("millisecond", () => new DateTimeOffset(max.Year, max.Month, max.Day, max.Hour, max.Minute, max.Second, max.Millisecond + 1, TimeSpan.Zero));
        }

        [TestMethod]
        public void DateTimeOffset_Ctor_Int_Int_Int_Int_Int_Int_TimeSpan()
        {
            var dateTimeOffset = new DateTimeOffset(1973, 10, 6, 14, 30, 0, TimeSpan.Zero);
            VerifyDateTimeOffset(dateTimeOffset, 1973, 10, 6, 14, 30, 0, 0, TimeSpan.Zero);
        }

        [TestMethod]
        public void DateTimeOffset_Ctor_Int_Int_Int_Int_Int_Int_TimeSpan_Invalid()
        {
            AssertExtensions.Throws<ArgumentException>("offset", () => new DateTimeOffset(1, 1, 1, 1, 1, 1, new TimeSpan(0, 0, 3))); // TimeSpan is not whole minutes
            AssertExtensions.Throws<ArgumentException>("offset", () => new DateTimeOffset(1, 1, 1, 1, 1, 1, new TimeSpan(0, 0, -3))); // TimeSpan is not whole minutes
            AssertExtensions.Throws<ArgumentException>("offset", () => new DateTimeOffset(1, 1, 1, 1, 1, 1, new TimeSpan(0, 0, 0, 0, 3))); // TimeSpan is not whole minutes
            AssertExtensions.Throws<ArgumentException>("offset", () => new DateTimeOffset(1, 1, 1, 1, 1, 1, new TimeSpan(0, 0, 0, 0, -3))); // TimeSpan is not whole minutes

            AssertExtensions.Throws<ArgumentOutOfRangeException>("offset", () => new DateTimeOffset(1, 1, 1, 1, 1, 1, TimeSpan.FromHours(-15))); // TimeZone.Offset > 14
            AssertExtensions.Throws<ArgumentOutOfRangeException>("offset", () => new DateTimeOffset(1, 1, 1, 1, 1, 1, TimeSpan.FromHours(15))); // TimeZone.Offset < -14

            // Invalid DateTime
            AssertExtensions.Throws<ArgumentOutOfRangeException>(null, () => new DateTimeOffset(0, 1, 1, 1, 1, 1, TimeSpan.Zero)); // Year < 1
            AssertExtensions.Throws<ArgumentOutOfRangeException>(null, () => new DateTimeOffset(10000, 1, 1, 1, 1, 1, TimeSpan.Zero)); // Year > 9999

            AssertExtensions.Throws<ArgumentOutOfRangeException>(null, () => new DateTimeOffset(1, 0, 1, 1, 1, 1, TimeSpan.Zero)); // Month < 1
            AssertExtensions.Throws<ArgumentOutOfRangeException>(null, () => new DateTimeOffset(1, 13, 1, 1, 1, 1, TimeSpan.Zero)); // Month > 23

            AssertExtensions.Throws<ArgumentOutOfRangeException>(null, () => new DateTimeOffset(1, 1, 0, 1, 1, 1, TimeSpan.Zero)); // Day < 1
            AssertExtensions.Throws<ArgumentOutOfRangeException>(null, () => new DateTimeOffset(1, 1, 32, 1, 1, 1, TimeSpan.Zero)); // Day > days in month

            AssertExtensions.Throws<ArgumentOutOfRangeException>(null, () => new DateTimeOffset(1, 1, 1, -1, 1, 1, TimeSpan.Zero)); // Hour < 0
            AssertExtensions.Throws<ArgumentOutOfRangeException>(null, () => new DateTimeOffset(1, 1, 1, 24, 1, 1, TimeSpan.Zero)); // Hour > 23

            AssertExtensions.Throws<ArgumentOutOfRangeException>(null, () => new DateTimeOffset(1, 1, 1, 1, -1, 1, TimeSpan.Zero)); // Minute < 0
            AssertExtensions.Throws<ArgumentOutOfRangeException>(null, () => new DateTimeOffset(1, 1, 1, 1, 60, 1, TimeSpan.Zero)); // Minute > 59

            AssertExtensions.Throws<ArgumentOutOfRangeException>(null, () => new DateTimeOffset(1, 1, 1, 1, 1, -1, TimeSpan.Zero)); // Second < 0
            AssertExtensions.Throws<ArgumentOutOfRangeException>(null, () => new DateTimeOffset(1, 1, 1, 1, 1, 60, TimeSpan.Zero)); // Second > 59

            // DateTime < DateTimeOffset.MinValue
            DateTimeOffset min = DateTimeOffset.MinValue;
            AssertExtensions.Throws<ArgumentOutOfRangeException>(null, () => new DateTimeOffset(min.Year - 1, min.Month, min.Day, min.Hour, min.Minute, min.Second, TimeSpan.Zero));
            AssertExtensions.Throws<ArgumentOutOfRangeException>(null, () => new DateTimeOffset(min.Year, min.Month - 1, min.Day, min.Hour, min.Minute, min.Second, TimeSpan.Zero));
            AssertExtensions.Throws<ArgumentOutOfRangeException>(null, () => new DateTimeOffset(min.Year, min.Month, min.Day - 1, min.Hour, min.Minute, min.Second, TimeSpan.Zero));
            AssertExtensions.Throws<ArgumentOutOfRangeException>(null, () => new DateTimeOffset(min.Year, min.Month, min.Day, min.Hour - 1, min.Minute, min.Second, TimeSpan.Zero));
            AssertExtensions.Throws<ArgumentOutOfRangeException>(null, () => new DateTimeOffset(min.Year, min.Month, min.Day, min.Hour, min.Minute - 1, min.Second, TimeSpan.Zero));
            AssertExtensions.Throws<ArgumentOutOfRangeException>(null, () => new DateTimeOffset(min.Year, min.Month, min.Day, min.Hour, min.Minute, min.Second - 1, TimeSpan.Zero));

            // DateTime > DateTimeOffset.MaxValue
            DateTimeOffset max = DateTimeOffset.MaxValue;
            AssertExtensions.Throws<ArgumentOutOfRangeException>(null, () => new DateTimeOffset(max.Year + 1, max.Month, max.Day, max.Hour, max.Minute, max.Second, TimeSpan.Zero));
            AssertExtensions.Throws<ArgumentOutOfRangeException>(null, () => new DateTimeOffset(max.Year, max.Month + 1, max.Day + 1, max.Hour, max.Minute, max.Second, TimeSpan.Zero));
            AssertExtensions.Throws<ArgumentOutOfRangeException>(null, () => new DateTimeOffset(max.Year, max.Month, max.Day + 1, max.Hour, max.Minute, max.Second, TimeSpan.Zero));
            AssertExtensions.Throws<ArgumentOutOfRangeException>(null, () => new DateTimeOffset(max.Year, max.Month, max.Day, max.Hour + 1, max.Minute, max.Second, TimeSpan.Zero));
            AssertExtensions.Throws<ArgumentOutOfRangeException>(null, () => new DateTimeOffset(max.Year, max.Month, max.Day, max.Hour, max.Minute + 1, max.Second, TimeSpan.Zero));
            AssertExtensions.Throws<ArgumentOutOfRangeException>(null, () => new DateTimeOffset(max.Year, max.Month, max.Day, max.Hour, max.Minute, max.Second + 1, TimeSpan.Zero));
        }

        [TestMethod]
        public void DateTimeOffset_ImplicitCast_DateTime()
        {
            DateTime dateTime = new DateTime(2012, 6, 11, 0, 0, 0, 0, DateTimeKind.Utc);
            DateTimeOffset dateTimeOffset = dateTime;
            VerifyDateTimeOffset(dateTimeOffset, 2012, 6, 11, 0, 0, 0, 0, TimeSpan.Zero);
        }

        [TestMethod]
        public void DateTimeOffset_AddSubtract_TimeSpan()
        {
            var dateTimeOffset = new DateTimeOffset(new DateTime(2012, 6, 18, 10, 5, 1, 0, DateTimeKind.Utc));
            TimeSpan timeSpan = dateTimeOffset.TimeOfDay;

            DateTimeOffset newDate = dateTimeOffset.Subtract(timeSpan);
            Assert.AreEqual(new DateTimeOffset(new DateTime(2012, 6, 18, 0, 0, 0, 0, DateTimeKind.Utc)).Ticks, newDate.Ticks);
            Assert.AreEqual(dateTimeOffset.Ticks, newDate.Add(timeSpan).Ticks);
        }

        private static IEnumerable<object[]> Subtract_TimeSpan_TestData()
        {
            var dateTimeOffset = new DateTimeOffset(new DateTime(2012, 6, 18, 10, 5, 1, 0, DateTimeKind.Utc));

            yield return new object[] { dateTimeOffset, new TimeSpan(10, 5, 1), new DateTimeOffset(new DateTime(2012, 6, 18, 0, 0, 0, 0, DateTimeKind.Utc)) };
            yield return new object[] { dateTimeOffset, new TimeSpan(-10, -5, -1), new DateTimeOffset(new DateTime(2012, 6, 18, 20, 10, 2, 0, DateTimeKind.Utc)) };
        }

        [TestMethod]
        public void DateTimeOffset_Subtract_TimeSpan_MemberDataTests()
        {
            foreach (var item in Subtract_TimeSpan_TestData())
                Subtract_TimeSpan((DateTimeOffset)item[0], (TimeSpan)item[1], (DateTimeOffset)item[2]);
        }

        private static void Subtract_TimeSpan(DateTimeOffset dt, TimeSpan ts, DateTimeOffset expected)
        {
            Assert.AreEqual(expected, dt - ts);
            Assert.AreEqual(expected, dt.Subtract(ts));
        }

        private static IEnumerable<object[]> Subtract_DateTimeOffset_TestData()
        {
            var dateTimeOffset1 = new DateTimeOffset(new DateTime(1996, 6, 3, 22, 15, 0, DateTimeKind.Utc));
            var dateTimeOffset2 = new DateTimeOffset(new DateTime(1996, 12, 6, 13, 2, 0, DateTimeKind.Utc));
            var dateTimeOffset3 = new DateTimeOffset(new DateTime(1996, 10, 12, 8, 42, 0, DateTimeKind.Utc));

            yield return new object[] { dateTimeOffset2, dateTimeOffset1, new TimeSpan(185, 14, 47, 0) };
            yield return new object[] { dateTimeOffset1, dateTimeOffset2, new TimeSpan(-185, -14, -47, 0) };
            yield return new object[] { dateTimeOffset1, dateTimeOffset2, new TimeSpan(-185, -14, -47, 0) };
        }

        [TestMethod]
        public void DateTimeOffset_Subtract_DateTimeOffset_MemberDataTests()
        {
            foreach (var item in Subtract_DateTimeOffset_TestData())
                Subtract_DateTimeOffset((DateTimeOffset)item[0], (DateTimeOffset)item[1], (TimeSpan)item[2]);
        }

        private static void Subtract_DateTimeOffset(DateTimeOffset dt1, DateTimeOffset dt2, TimeSpan expected)
        {
            Assert.AreEqual(expected, dt1 - dt2);
            Assert.AreEqual(expected, dt1.Subtract(dt2));
        }

        private static IEnumerable<object[]> Add_TimeSpan_TestData()
        {
            yield return new object[] { new DateTimeOffset(new DateTime(1000, DateTimeKind.Utc)), new TimeSpan(10), new DateTimeOffset(new DateTime(1010, DateTimeKind.Utc)) };
            yield return new object[] { new DateTimeOffset(new DateTime(1000, DateTimeKind.Utc)), TimeSpan.Zero, new DateTimeOffset(new DateTime(1000, DateTimeKind.Utc)) };
            yield return new object[] { new DateTimeOffset(new DateTime(1000, DateTimeKind.Utc)), new TimeSpan(-10), new DateTimeOffset(new DateTime(990, DateTimeKind.Utc)) };
        }

        [TestMethod]
        public void DateTimeOffset_Add_TimeSpan_MemberDataTests()
        {
            foreach (var item in Add_TimeSpan_TestData())
                Add_TimeSpan((DateTimeOffset)item[0], (TimeSpan)item[1], (DateTimeOffset)item[2]);
        }

        private static void Add_TimeSpan(DateTimeOffset dateTimeOffset, TimeSpan timeSpan, DateTimeOffset expected)
        {
            Assert.AreEqual(expected, dateTimeOffset.Add(timeSpan));
            Assert.AreEqual(expected, dateTimeOffset + timeSpan);
        }

        [TestMethod]
        public void DateTimeOffset_Add_TimeSpan_NewDateOutOfRange_ThrowsArgumentOutOfRangeException()
        {
            AssertExtensions.Throws<ArgumentOutOfRangeException>("value", () => DateTimeOffset.MinValue.Add(TimeSpan.FromTicks(-1)));
            AssertExtensions.Throws<ArgumentOutOfRangeException>("value", () => DateTimeOffset.MaxValue.Add(TimeSpan.FromTicks(11)));
        }

        private static IEnumerable<object[]> AddYears_TestData()
        {
            yield return new object[] { new DateTimeOffset(new DateTime(1986, 8, 15, 10, 20, 5, 70, DateTimeKind.Utc)), 10, new DateTimeOffset(new DateTime(1996, 8, 15, 10, 20, 5, 70, DateTimeKind.Utc)) };
            yield return new object[] { new DateTimeOffset(new DateTime(1986, 8, 15, 10, 20, 5, 70, DateTimeKind.Utc)), 0, new DateTimeOffset(new DateTime(1986, 8, 15, 10, 20, 5, 70, DateTimeKind.Utc)) };
            yield return new object[] { new DateTimeOffset(new DateTime(1986, 8, 15, 10, 20, 5, 70, DateTimeKind.Utc)), -10, new DateTimeOffset(new DateTime(1976, 8, 15, 10, 20, 5, 70, DateTimeKind.Utc)) };
        }

        [TestMethod]
        public void DateTimeOffset_AddYears_MemberDataTests()
        {
            foreach (var item in AddYears_TestData())
                AddYears((DateTimeOffset)item[0], (int)item[1], (DateTimeOffset)item[2]);
        }

        private static void AddYears(DateTimeOffset dateTimeOffset, int years, DateTimeOffset expected)
        {
            Assert.AreEqual(expected, dateTimeOffset.AddYears(years));
        }

        [TestMethod]
        public void DateTimeOffset_AddYears_NewDateOutOfRange_ThrowsArgumentOutOfRangeException()
        {
            AssertExtensions.Throws<ArgumentOutOfRangeException>("years", () => DateTimeOffset.Now.AddYears(10001));
            AssertExtensions.Throws<ArgumentOutOfRangeException>("years", () => DateTimeOffset.Now.AddYears(-10001));

            AssertExtensions.Throws<ArgumentOutOfRangeException>("months", () => DateTimeOffset.MaxValue.AddYears(1));
            AssertExtensions.Throws<ArgumentOutOfRangeException>("months", () => DateTimeOffset.MinValue.AddYears(-1));
        }

        private static IEnumerable<object[]> AddMonths_TestData()
        {
            yield return new object[] { new DateTimeOffset(new DateTime(1986, 8, 15, 10, 20, 5, 70, DateTimeKind.Utc)), 2, new DateTimeOffset(new DateTime(1986, 10, 15, 10, 20, 5, 70, DateTimeKind.Utc)) };
            yield return new object[] { new DateTimeOffset(new DateTime(1986, 8, 15, 10, 20, 5, 70, DateTimeKind.Utc)), 0, new DateTimeOffset(new DateTime(1986, 8, 15, 10, 20, 5, 70, DateTimeKind.Utc)) };
            yield return new object[] { new DateTimeOffset(new DateTime(1986, 8, 15, 10, 20, 5, 70, DateTimeKind.Utc)), -2, new DateTimeOffset(new DateTime(1986, 6, 15, 10, 20, 5, 70, DateTimeKind.Utc)) };
        }


        [TestMethod]
        public void DateTimeOffset_AddMonths_MemberDataTests()
        {
            foreach (var item in AddMonths_TestData())
                AddMonths((DateTimeOffset)item[0], (int)item[1], (DateTimeOffset)item[2]);
        }

        private static void AddMonths(DateTimeOffset dateTimeOffset, int months, DateTimeOffset expected)
        {
            Assert.AreEqual(expected, dateTimeOffset.AddMonths(months));
        }

        [TestMethod]
        public void DateTimeOffset_AddMonths_NewDateOutOfRange_ThrowsArgumentOutOfRangeException()
        {
            AssertExtensions.Throws<ArgumentOutOfRangeException>("months", () => DateTimeOffset.Now.AddMonths(120001));
            AssertExtensions.Throws<ArgumentOutOfRangeException>("months", () => DateTimeOffset.Now.AddMonths(-120001));

            AssertExtensions.Throws<ArgumentOutOfRangeException>("months", () => DateTimeOffset.MaxValue.AddMonths(1));
            AssertExtensions.Throws<ArgumentOutOfRangeException>("months", () => DateTimeOffset.MinValue.AddMonths(-1));
        }

        private static IEnumerable<object[]> AddDays_TestData()
        {
            yield return new object[] { new DateTimeOffset(new DateTime(1986, 8, 15, 10, 20, 5, 70, DateTimeKind.Utc)), 2, new DateTimeOffset(new DateTime(1986, 8, 17, 10, 20, 5, 70, DateTimeKind.Utc)) };
            yield return new object[] { new DateTimeOffset(new DateTime(1986, 8, 15, 10, 20, 5, 70, DateTimeKind.Utc)), 0, new DateTimeOffset(new DateTime(1986, 8, 15, 10, 20, 5, 70, DateTimeKind.Utc)) };
            yield return new object[] { new DateTimeOffset(new DateTime(1986, 8, 15, 10, 20, 5, 70, DateTimeKind.Utc)), -2, new DateTimeOffset(new DateTime(1986, 8, 13, 10, 20, 5, 70, DateTimeKind.Utc)) };
        }


        [TestMethod]
        public void DateTimeOffset_AddDays_MemberDataTests()
        {
            foreach (var item in AddDays_TestData())
                AddDays((DateTimeOffset)item[0], (int)item[1], (DateTimeOffset)item[2]);
        }

        private static void AddDays(DateTimeOffset dateTimeOffset, double days, DateTimeOffset expected)
        {
            Assert.AreEqual(expected, dateTimeOffset.AddDays(days));
        }

        [TestMethod]
        public void DateTimeOffset_AddDays_NewDateOutOfRange_ThrowsArgumentOutOfRangeException()
        {
            AssertExtensions.Throws<ArgumentOutOfRangeException>("value", () => DateTimeOffset.MaxValue.AddDays(1));
            AssertExtensions.Throws<ArgumentOutOfRangeException>("value", () => DateTimeOffset.MinValue.AddDays(-1));
        }

        private static IEnumerable<object[]> AddHours_TestData()
        {
            yield return new object[] { new DateTimeOffset(new DateTime(1986, 8, 15, 10, 20, 5, 70, DateTimeKind.Utc)), 3, new DateTimeOffset(new DateTime(1986, 8, 15, 13, 20, 5, 70, DateTimeKind.Utc)) };
            yield return new object[] { new DateTimeOffset(new DateTime(1986, 8, 15, 10, 20, 5, 70, DateTimeKind.Utc)), 0, new DateTimeOffset(new DateTime(1986, 8, 15, 10, 20, 5, 70, DateTimeKind.Utc)) };
            yield return new object[] { new DateTimeOffset(new DateTime(1986, 8, 15, 10, 20, 5, 70, DateTimeKind.Utc)), -3, new DateTimeOffset(new DateTime(1986, 8, 15, 7, 20, 5, 70, DateTimeKind.Utc)) };
        }


        [TestMethod]
        public void DateTimeOffset_AddHours_MemberDataTests()
        {
            foreach (var item in AddHours_TestData())
                AddHours((DateTimeOffset)item[0], (int)item[1], (DateTimeOffset)item[2]);
        }

        private static void AddHours(DateTimeOffset dateTimeOffset, double hours, DateTimeOffset expected)
        {
            Assert.AreEqual(expected, dateTimeOffset.AddHours(hours));
        }

        [TestMethod]
        public void DateTimeOffset_AddHours_NewDateOutOfRange_ThrowsArgumentOutOfRangeException()
        {
            AssertExtensions.Throws<ArgumentOutOfRangeException>("value", () => DateTimeOffset.MaxValue.AddHours(1));
            AssertExtensions.Throws<ArgumentOutOfRangeException>("value", () => DateTimeOffset.MinValue.AddHours(-1));
        }

        private static IEnumerable<object[]> AddMinutes_TestData()
        {
            yield return new object[] { new DateTimeOffset(new DateTime(1986, 8, 15, 10, 20, 5, 70, DateTimeKind.Utc)), 5, new DateTimeOffset(new DateTime(1986, 8, 15, 10, 25, 5, 70, DateTimeKind.Utc)) };
            yield return new object[] { new DateTimeOffset(new DateTime(1986, 8, 15, 10, 20, 5, 70, DateTimeKind.Utc)), 0, new DateTimeOffset(new DateTime(1986, 8, 15, 10, 20, 5, 70, DateTimeKind.Utc)) };
            yield return new object[] { new DateTimeOffset(new DateTime(1986, 8, 15, 10, 20, 5, 70, DateTimeKind.Utc)), -5, new DateTimeOffset(new DateTime(1986, 8, 15, 10, 15, 5, 70, DateTimeKind.Utc)) };
        }


        [TestMethod]
        public void DateTimeOffset_AddMinutes_MemberDataTests()
        {
            foreach (var item in AddMinutes_TestData())
                AddMinutes((DateTimeOffset)item[0], (int)item[1], (DateTimeOffset)item[2]);
        }

        private static void AddMinutes(DateTimeOffset dateTimeOffset, double minutes, DateTimeOffset expected)
        {
            Assert.AreEqual(expected, dateTimeOffset.AddMinutes(minutes));
        }

        [TestMethod]
        public void DateTimeOffset_AddMinutes_NewDateOutOfRange_ThrowsArgumentOutOfRangeException()
        {
            AssertExtensions.Throws<ArgumentOutOfRangeException>("value", () => DateTimeOffset.MaxValue.AddMinutes(1));
            AssertExtensions.Throws<ArgumentOutOfRangeException>("value", () => DateTimeOffset.MinValue.AddMinutes(-1));
        }

        private static IEnumerable<object[]> AddSeconds_TestData()
        {
            yield return new object[] { new DateTimeOffset(new DateTime(1986, 8, 15, 10, 20, 5, 70, DateTimeKind.Utc)), 30, new DateTimeOffset(new DateTime(1986, 8, 15, 10, 20, 35, 70, DateTimeKind.Utc)) };
            yield return new object[] { new DateTimeOffset(new DateTime(1986, 8, 15, 10, 20, 5, 70, DateTimeKind.Utc)), 0, new DateTimeOffset(new DateTime(1986, 8, 15, 10, 20, 5, 70, DateTimeKind.Utc)) };
            yield return new object[] { new DateTimeOffset(new DateTime(1986, 8, 15, 10, 20, 5, 70, DateTimeKind.Utc)), -3, new DateTimeOffset(new DateTime(1986, 8, 15, 10, 20, 2, 70, DateTimeKind.Utc)) };
        }


        [TestMethod]
        public void DateTimeOffset_AddSeconds_MemberDataTests()
        {
            foreach (var item in AddSeconds_TestData())
                AddSeconds((DateTimeOffset)item[0], (int)item[1], (DateTimeOffset)item[2]);
        }

        private static void AddSeconds(DateTimeOffset dateTimeOffset, double seconds, DateTimeOffset expected)
        {
            Assert.AreEqual(expected, dateTimeOffset.AddSeconds(seconds));
        }

        [TestMethod]
        public void DateTimeOffset_AddSeconds_NewDateOutOfRange_ThrowsArgumentOutOfRangeException()
        {
            AssertExtensions.Throws<ArgumentOutOfRangeException>("value", () => DateTimeOffset.MaxValue.AddSeconds(1));
            AssertExtensions.Throws<ArgumentOutOfRangeException>("value", () => DateTimeOffset.MinValue.AddSeconds(-1));
        }

        private static IEnumerable<object[]> AddMilliseconds_TestData()
        {
            yield return new object[] { new DateTimeOffset(new DateTime(1986, 8, 15, 10, 20, 5, 70, DateTimeKind.Utc)), 10, new DateTimeOffset(new DateTime(1986, 8, 15, 10, 20, 5, 80, DateTimeKind.Utc)) };
            yield return new object[] { new DateTimeOffset(new DateTime(1986, 8, 15, 10, 20, 5, 70, DateTimeKind.Utc)), 0, new DateTimeOffset(new DateTime(1986, 8, 15, 10, 20, 5, 70, DateTimeKind.Utc)) };
            yield return new object[] { new DateTimeOffset(new DateTime(1986, 8, 15, 10, 20, 5, 70, DateTimeKind.Utc)), -10, new DateTimeOffset(new DateTime(1986, 8, 15, 10, 20, 5, 60, DateTimeKind.Utc)) };
        }


        [TestMethod]
        public void DateTimeOffset_AddMilliseconds_MemberDataTests()
        {
            foreach (var item in AddMilliseconds_TestData())
                AddMilliseconds((DateTimeOffset)item[0], (int)item[1], (DateTimeOffset)item[2]);
        }

        private static void AddMilliseconds(DateTimeOffset dateTimeOffset, double milliseconds, DateTimeOffset expected)
        {
            Assert.AreEqual(expected, dateTimeOffset.AddMilliseconds(milliseconds));
        }

        [TestMethod]
        public void DateTimeOffset_AddMilliseconds_NewDateOutOfRange_ThrowsArgumentOutOfRangeException()
        {
            AssertExtensions.Throws<ArgumentOutOfRangeException>("value", () => DateTimeOffset.MaxValue.AddMilliseconds(1));
            AssertExtensions.Throws<ArgumentOutOfRangeException>("value", () => DateTimeOffset.MinValue.AddMilliseconds(-1));
        }

        private static IEnumerable<object[]> AddTicks_TestData()
        {
            yield return new object[] { new DateTimeOffset(new DateTime(1000, DateTimeKind.Utc)), 10, new DateTimeOffset(new DateTime(1010, DateTimeKind.Utc)) };
            yield return new object[] { new DateTimeOffset(new DateTime(1000, DateTimeKind.Utc)), 0, new DateTimeOffset(new DateTime(1000, DateTimeKind.Utc)) };
            yield return new object[] { new DateTimeOffset(new DateTime(1000, DateTimeKind.Utc)), -10, new DateTimeOffset(new DateTime(990, DateTimeKind.Utc)) };
        }


        [TestMethod]
        public void DateTimeOffset_AddTicks_MemberDataTests()
        {
            foreach (var item in AddTicks_TestData())
                AddTicks((DateTimeOffset)item[0], (int)item[1], (DateTimeOffset)item[2]);
        }

        private static void AddTicks(DateTimeOffset dateTimeOffset, long ticks, DateTimeOffset expected)
        {
            Assert.AreEqual(expected, dateTimeOffset.AddTicks(ticks));
        }

        [TestMethod]
        public void DateTimeOffset_AddTicks_NewDateOutOfRange_ThrowsArgumentOutOfRangeException()
        {
            AssertExtensions.Throws<ArgumentOutOfRangeException>("value", () => DateTimeOffset.MaxValue.AddTicks(1));
            AssertExtensions.Throws<ArgumentOutOfRangeException>("value", () => DateTimeOffset.MinValue.AddTicks(-1));
        }

        [TestMethod]
        public void DateTimeOffset_ToFromFileTime()
        {
            var today = new DateTimeOffset(DateTime.Today);

            long dateTimeRaw = today.ToFileTime();
            Assert.AreEqual(today, DateTimeOffset.FromFileTime(dateTimeRaw));
        }

        [TestMethod]
        public void DateTimeOffset_UtcDateTime()
        {
            DateTime now = DateTime.Now;
            var dateTimeOffset = new DateTimeOffset(now);
            Assert.AreEqual(DateTime.Today, dateTimeOffset.Date);
            Assert.AreEqual(now, dateTimeOffset.DateTime);
            Assert.AreEqual(now.ToUniversalTime(), dateTimeOffset.UtcDateTime);
        }

        [TestMethod]
        public void DateTimeOffset_UtcNow()
        {
            DateTimeOffset start = DateTimeOffset.UtcNow;
            Assert.IsTrue(
                SpinWait.SpinUntil(() => DateTimeOffset.UtcNow > start, TimeSpan.FromSeconds(30)),
                "Expected UtcNow to changes");
        }

        [TestMethod]
        public void DateTimeOffset_DayOfYear()
        {
            DateTimeOffset dateTimeOffset = new DateTimeOffset();
            Assert.AreEqual(dateTimeOffset.DateTime.DayOfYear, dateTimeOffset.DayOfYear);
        }

        [TestMethod]
        public void DateTimeOffset_DayOfWeekTest()
        {
            DateTimeOffset dateTimeOffset = new DateTimeOffset();
            Assert.AreEqual(dateTimeOffset.DateTime.DayOfWeek, dateTimeOffset.DayOfWeek);
        }

        [TestMethod]
        public void DateTimeOffset_TimeOfDay()
        {
            DateTimeOffset dateTimeOffset = new DateTimeOffset();
            Assert.AreEqual(dateTimeOffset.DateTime.TimeOfDay, dateTimeOffset.TimeOfDay);
        }

        private static IEnumerable<object[]> UnixTime_TestData()
        {
            yield return new object[] { TestTime.FromMilliseconds(DateTimeOffset.MinValue, -62135596800000) };
            yield return new object[] { TestTime.FromMilliseconds(DateTimeOffset.MaxValue, 253402300799999) };
            yield return new object[] { TestTime.FromMilliseconds(new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero), 0) };
            yield return new object[] { TestTime.FromMilliseconds(new DateTimeOffset(2014, 6, 13, 17, 21, 50, TimeSpan.Zero), 1402680110000) };
            yield return new object[] { TestTime.FromMilliseconds(new DateTimeOffset(2830, 12, 15, 1, 23, 45, TimeSpan.Zero), 27169089825000) };
            yield return new object[] { TestTime.FromMilliseconds(new DateTimeOffset(2830, 12, 15, 1, 23, 45, 399, TimeSpan.Zero), 27169089825399) };
            yield return new object[] { TestTime.FromMilliseconds(new DateTimeOffset(9999, 12, 30, 23, 24, 25, TimeSpan.Zero), 253402212265000) };
            yield return new object[] { TestTime.FromMilliseconds(new DateTimeOffset(1907, 7, 7, 7, 7, 7, TimeSpan.Zero), -1971967973000) };
            yield return new object[] { TestTime.FromMilliseconds(new DateTimeOffset(1907, 7, 7, 7, 7, 7, 1, TimeSpan.Zero), -1971967972999) };
            yield return new object[] { TestTime.FromMilliseconds(new DateTimeOffset(1907, 7, 7, 7, 7, 7, 777, TimeSpan.Zero), -1971967972223) };
            yield return new object[] { TestTime.FromMilliseconds(new DateTimeOffset(601636288270011234, TimeSpan.Zero), -1971967972999) };
        }


        [TestMethod]
        public void DateTimeOffset_ToUnixTimeMilliseconds_MemberDataTests()
        {
            foreach (var item in UnixTime_TestData())
                ToUnixTimeMilliseconds((TestTime)item[0]);
        }

        private static void ToUnixTimeMilliseconds(TestTime test)
        {
            long expectedMilliseconds = test.UnixTimeMilliseconds;
            long actualMilliseconds = test.DateTimeOffset.ToUnixTimeMilliseconds();
            Assert.AreEqual(expectedMilliseconds, actualMilliseconds);
        }


        [TestMethod]
        public void DateTimeOffset_ToUnixTimeMilliseconds_RoundTrip_MemberDataTests()
        {
            foreach (var item in UnixTime_TestData())
                ToUnixTimeMilliseconds_RoundTrip((TestTime)item[0]);
        }

        private static void ToUnixTimeMilliseconds_RoundTrip(TestTime test)
        {
            long unixTimeMilliseconds = test.DateTimeOffset.ToUnixTimeMilliseconds();
            FromUnixTimeMilliseconds(TestTime.FromMilliseconds(test.DateTimeOffset, unixTimeMilliseconds));
        }


        [TestMethod]
        public void DateTimeOffset_ToUnixTimeSeconds_MemberDataTests()
        {
            foreach (var item in UnixTime_TestData())
                ToUnixTimeSeconds((TestTime)item[0]);
        }

        private static void ToUnixTimeSeconds(TestTime test)
        {
            long expectedSeconds = test.UnixTimeSeconds;
            long actualSeconds = test.DateTimeOffset.ToUnixTimeSeconds();
            Assert.AreEqual(expectedSeconds, actualSeconds);
        }


        [TestMethod]
        public void DateTimeOffset_ToUnixTimeSeconds_RoundTrip_MemberDataTests()
        {
            foreach (var item in UnixTime_TestData())
                ToUnixTimeSeconds_RoundTrip((TestTime)item[0]);
        }

        private static void ToUnixTimeSeconds_RoundTrip(TestTime test)
        {
            long unixTimeSeconds = test.DateTimeOffset.ToUnixTimeSeconds();
            FromUnixTimeSeconds(TestTime.FromSeconds(test.DateTimeOffset, unixTimeSeconds));
        }


        [TestMethod]
        public void DateTimeOffset_FromUnixTimeMilliseconds_MemberDataTests()
        {
            foreach (var item in UnixTime_TestData())
                FromUnixTimeMilliseconds((TestTime)item[0]);
        }

        private static void FromUnixTimeMilliseconds(TestTime test)
        {
            // Only assert that expected == actual up to millisecond precision for conversion from milliseconds
            long expectedTicks = (test.DateTimeOffset.UtcTicks / TimeSpan.TicksPerMillisecond) * TimeSpan.TicksPerMillisecond;
            long actualTicks = DateTimeOffset.FromUnixTimeMilliseconds(test.UnixTimeMilliseconds).UtcTicks;
            Assert.AreEqual(expectedTicks, actualTicks);
        }

        [TestMethod]
        public void DateTimeOffset_FromUnixTimeMilliseconds_Invalid()
        {
            AssertExtensions.Throws<ArgumentOutOfRangeException>("milliseconds", () => DateTimeOffset.FromUnixTimeMilliseconds(-62135596800001)); // Milliseconds < DateTimeOffset.MinValue
            AssertExtensions.Throws<ArgumentOutOfRangeException>("milliseconds", () => DateTimeOffset.FromUnixTimeMilliseconds(253402300800000)); // Milliseconds > DateTimeOffset.MaxValue

            AssertExtensions.Throws<ArgumentOutOfRangeException>("milliseconds", () => DateTimeOffset.FromUnixTimeMilliseconds(long.MinValue)); // Milliseconds < DateTimeOffset.MinValue
            AssertExtensions.Throws<ArgumentOutOfRangeException>("milliseconds", () => DateTimeOffset.FromUnixTimeMilliseconds(long.MaxValue)); // Milliseconds > DateTimeOffset.MaxValue
        }


        [TestMethod]
        public void DateTimeOffset_FromUnixTimeSeconds_MemberDataTests()
        {
            foreach (var item in UnixTime_TestData())
                FromUnixTimeSeconds((TestTime)item[0]);
        }

        private static void FromUnixTimeSeconds(TestTime test)
        {
            // Only assert that expected == actual up to second precision for conversion from seconds
            long expectedTicks = (test.DateTimeOffset.UtcTicks / TimeSpan.TicksPerSecond) * TimeSpan.TicksPerSecond;
            long actualTicks = DateTimeOffset.FromUnixTimeSeconds(test.UnixTimeSeconds).UtcTicks;
            Assert.AreEqual(expectedTicks, actualTicks);
        }

        [TestMethod]
        public void DateTimeOffset_FromUnixTimeSeconds_Invalid()
        {
            AssertExtensions.Throws<ArgumentOutOfRangeException>("seconds", () => DateTimeOffset.FromUnixTimeSeconds(-62135596801));// Seconds < DateTimeOffset.MinValue
            AssertExtensions.Throws<ArgumentOutOfRangeException>("seconds", () => DateTimeOffset.FromUnixTimeSeconds(253402300800)); // Seconds > DateTimeOffset.MaxValue

            AssertExtensions.Throws<ArgumentOutOfRangeException>("seconds", () => DateTimeOffset.FromUnixTimeSeconds(long.MinValue)); // Seconds < DateTimeOffset.MinValue
            AssertExtensions.Throws<ArgumentOutOfRangeException>("seconds", () => DateTimeOffset.FromUnixTimeSeconds(long.MaxValue)); // Seconds < DateTimeOffset.MinValue
        }


        [TestMethod]
        public void DateTimeOffset_FromUnixTimeMilliseconds_RoundTrip_MemberDataTests()
        {
            foreach (var item in UnixTime_TestData())
                FromUnixTimeMilliseconds_RoundTrip((TestTime)item[0]);
        }

        public void FromUnixTimeMilliseconds_RoundTrip(TestTime test)
        {
            DateTimeOffset dateTime = DateTimeOffset.FromUnixTimeMilliseconds(test.UnixTimeMilliseconds);
            ToUnixTimeMilliseconds(TestTime.FromMilliseconds(dateTime, test.UnixTimeMilliseconds));
        }


        [TestMethod]
        public void DateTimeOffset_FromUnixTimeSeconds_RoundTrip_MemberDataTests()
        {
            foreach (var item in UnixTime_TestData())
                FromUnixTimeSeconds_RoundTrip((TestTime)item[0]);
        }

        private static void FromUnixTimeSeconds_RoundTrip(TestTime test)
        {
            DateTimeOffset dateTime = DateTimeOffset.FromUnixTimeSeconds(test.UnixTimeSeconds);
            ToUnixTimeSeconds(TestTime.FromSeconds(dateTime, test.UnixTimeSeconds));
        }

        [TestMethod]
        public void DateTimeOffset_ToLocalTime()
        {
            DateTimeOffset dateTimeOffset = new DateTimeOffset(new DateTime(1000, DateTimeKind.Utc));
            Assert.AreEqual(new DateTimeOffset(dateTimeOffset.UtcDateTime.ToLocalTime()), dateTimeOffset.ToLocalTime());
        }

        private static IEnumerable<object[]> Equals_TestData()
        {
            yield return new object[] { DateTimeOffset.MinValue, DateTimeOffset.MinValue, true, true };
            yield return new object[] { DateTimeOffset.MinValue, DateTimeOffset.MaxValue, false, false };

            yield return new object[] { DateTimeOffset.Now, new object(), false, false };
            yield return new object[] { DateTimeOffset.Now, null, false, false };
        }


        [TestMethod]
        public void DateTimeOffset_Equals_MemberDataTests()
        {
            foreach (var item in Equals_TestData())
                Equals((DateTimeOffset)item[0], item[1], (bool)item[2], (bool)item[3]);
        }

        private static void Equals(DateTimeOffset dateTimeOffset1, object obj, bool expectedEquals, bool expectedEqualsExact)
        {
            Assert.AreEqual(expectedEquals, dateTimeOffset1.Equals(obj));
            if (obj is DateTimeOffset)
            {
                DateTimeOffset dateTimeOffset2 = (DateTimeOffset)obj;
                Assert.AreEqual(expectedEquals, dateTimeOffset1.Equals(dateTimeOffset2));
                Assert.AreEqual(expectedEquals, DateTimeOffset.Equals(dateTimeOffset1, dateTimeOffset2));

                Assert.AreEqual(expectedEquals, dateTimeOffset1.GetHashCode().Equals(dateTimeOffset2.GetHashCode()));
                Assert.AreEqual(expectedEqualsExact, dateTimeOffset1.EqualsExact(dateTimeOffset2));

                Assert.AreEqual(expectedEquals, dateTimeOffset1 == dateTimeOffset2);
                Assert.AreEqual(!expectedEquals, dateTimeOffset1 != dateTimeOffset2);
            }
        }

        private static IEnumerable<object[]> Compare_TestData()
        {
            yield return new object[] { new DateTimeOffset(new DateTime(1000, DateTimeKind.Utc)), new DateTimeOffset(new DateTime(1000, DateTimeKind.Utc)), 0 };
            yield return new object[] { new DateTimeOffset(new DateTime(1000, DateTimeKind.Utc)), new DateTimeOffset(new DateTime(1001, DateTimeKind.Utc)), -1 };
            yield return new object[] { new DateTimeOffset(new DateTime(1000, DateTimeKind.Utc)), new DateTimeOffset(new DateTime(999, DateTimeKind.Utc)), 1 };
        }


        [TestMethod]
        public void DateTimeOffset_Compare_MemberDataTests()
        {
            foreach (var item in Compare_TestData())
                Compare((DateTimeOffset)item[0], (DateTimeOffset)item[1], (int)item[2]);
        }

        private static void Compare(DateTimeOffset dateTimeOffset1, DateTimeOffset dateTimeOffset2, int expected)
        {
            Assert.AreEqual(expected, Math.Sign(dateTimeOffset1.CompareTo(dateTimeOffset2)));
            Assert.AreEqual(expected, Math.Sign(DateTimeOffset.Compare(dateTimeOffset1, dateTimeOffset2)));

            IComparable comparable = dateTimeOffset1;
            Assert.AreEqual(expected, Math.Sign(comparable.CompareTo(dateTimeOffset2)));

            if (expected > 0)
            {
                Assert.IsTrue(dateTimeOffset1 > dateTimeOffset2);
                Assert.AreEqual(expected >= 0, dateTimeOffset1 >= dateTimeOffset2);
                Assert.IsFalse(dateTimeOffset1 < dateTimeOffset2);
                Assert.AreEqual(expected == 0, dateTimeOffset1 <= dateTimeOffset2);
            }
            else if (expected < 0)
            {
                Assert.IsFalse(dateTimeOffset1 > dateTimeOffset2);
                Assert.AreEqual(expected == 0, dateTimeOffset1 >= dateTimeOffset2);
                Assert.IsTrue(dateTimeOffset1 < dateTimeOffset2);
                Assert.AreEqual(expected <= 0, dateTimeOffset1 <= dateTimeOffset2);
            }
            else if (expected == 0)
            {
                Assert.IsFalse(dateTimeOffset1 > dateTimeOffset2);
                Assert.IsTrue(dateTimeOffset1 >= dateTimeOffset2);
                Assert.IsFalse(dateTimeOffset1 < dateTimeOffset2);
                Assert.IsTrue(dateTimeOffset1 <= dateTimeOffset2);
            }
        }

        [TestMethod]
        public void DateTimeOffset_Parse_String()
        {
            DateTimeOffset expected = DateTimeOffset.MaxValue;
            string expectedString = expected.ToString();

            DateTimeOffset result = DateTimeOffset.Parse(expectedString);
            Assert.AreEqual(expectedString, result.ToString());
        }

        [TestMethod]
        public void DateTimeOffset_Parse_String_FormatProvider()
        {
            DateTimeOffset expected = DateTimeOffset.MaxValue;
            string expectedString = expected.ToString();

            DateTimeOffset result = DateTimeOffset.Parse(expectedString, null);
            Assert.AreEqual(expectedString, result.ToString((IFormatProvider)null));
        }

        [TestMethod]
        public void DateTimeOffset_Parse_String_FormatProvider_DateTimeStyles()
        {
            DateTimeOffset expected = DateTimeOffset.MaxValue;
            string expectedString = expected.ToString();

            DateTimeOffset result = DateTimeOffset.Parse(expectedString, null, DateTimeStyles.None);
            Assert.AreEqual(expectedString, result.ToString());
        }

        [TestMethod]
        public void DateTimeOffset_Parse_Japanese()
        {
            var expected = new DateTimeOffset(new DateTime(2012, 12, 21, 10, 8, 6));
            var cultureInfo = new CultureInfo("ja-JP");

            string expectedString = string.Format(cultureInfo, "{0}", expected);
            Assert.AreEqual(expected, DateTimeOffset.Parse(expectedString, cultureInfo));
        }

        [TestMethod]
        public void DateTimeOffset_TryParse_String()
        {
            DateTimeOffset expected = DateTimeOffset.MaxValue;
            string expectedString = expected.ToString("u");

            DateTimeOffset result;
            Assert.IsTrue(DateTimeOffset.TryParse(expectedString, out result));
            Assert.AreEqual(expectedString, result.ToString("u"));
        }

        [TestMethod]
        public void DateTimeOffset_TryParse_String_FormatProvider_DateTimeStyles_U()
        {
            DateTimeOffset expected = DateTimeOffset.MaxValue;
            string expectedString = expected.ToString("u");

            DateTimeOffset result;
            Assert.IsTrue(DateTimeOffset.TryParse(expectedString, null, DateTimeStyles.None, out result));
            Assert.AreEqual(expectedString, result.ToString("u"));
        }

        [TestMethod]
        public void DateTimeOffset_TryParse_String_FormatProvider_DateTimeStyles_G()
        {
            DateTimeOffset expected = DateTimeOffset.MaxValue;
            string expectedString = expected.ToString("g");

            DateTimeOffset result;
            Assert.IsTrue(DateTimeOffset.TryParse(expectedString, null, DateTimeStyles.AssumeUniversal, out result));
            Assert.AreEqual(expectedString, result.ToString("g"));
        }

        //[TestMethod]
        // The full .NET framework has a bug and incorrectly parses this date
        public void TryParse_TimeDesignators_NetCore()
        {
            DateTimeOffset result;
            Assert.IsTrue(DateTimeOffset.TryParse("4/21 5am", new CultureInfo("en-US"), DateTimeStyles.None, out result));
            Assert.AreEqual(4, result.Month);
            Assert.AreEqual(21, result.Day);
            Assert.AreEqual(5, result.Hour);
            Assert.AreEqual(0, result.Minute);
            Assert.AreEqual(0, result.Second);

            Assert.IsTrue(DateTimeOffset.TryParse("4/21 5pm", new CultureInfo("en-US"), DateTimeStyles.None, out result));
            Assert.AreEqual(4, result.Month);
            Assert.AreEqual(21, result.Day);
            Assert.AreEqual(17, result.Hour);
            Assert.AreEqual(0, result.Minute);
            Assert.AreEqual(0, result.Second);
        }

        [TestMethod]
        // The coreclr fixed a bug where the .NET framework incorrectly parses this date
        public void TryParse_TimeDesignators_Netfx()
        {
            DateTimeOffset result;
            Assert.IsTrue(DateTimeOffset.TryParse("4/21 5am", new CultureInfo("en-US"), DateTimeStyles.None, out result));
            Assert.AreEqual(DateTimeOffset.Now.Month, result.Month);
            Assert.AreEqual(DateTimeOffset.Now.Day, result.Day);
            Assert.AreEqual(4, result.Hour);
            Assert.AreEqual(0, result.Minute);
            Assert.AreEqual(0, result.Second);

            Assert.IsTrue(DateTimeOffset.TryParse("4/21 5pm", new CultureInfo("en-US"), DateTimeStyles.None, out result));
            Assert.AreEqual(DateTimeOffset.Now.Month, result.Month);
            Assert.AreEqual(DateTimeOffset.Now.Day, result.Day);
            Assert.AreEqual(16, result.Hour);
            Assert.AreEqual(0, result.Minute);
            Assert.AreEqual(0, result.Second);
        }

        [TestMethod]
        public void DateTimeOffset_ParseExact_String_String_FormatProvider()
        {
            DateTimeOffset expected = DateTimeOffset.MaxValue;
            string expectedString = expected.ToString("u");

            DateTimeOffset result = DateTimeOffset.ParseExact(expectedString, "u", null);
            Assert.AreEqual(expectedString, result.ToString("u"));
        }

        [TestMethod]
        public void DateTimeOffset_ParseExact_String_String_FormatProvider_DateTimeStyles_U()
        {
            DateTimeOffset expected = DateTimeOffset.MaxValue;
            string expectedString = expected.ToString("u");

            DateTimeOffset result = DateTimeOffset.ParseExact(expectedString, "u", null, DateTimeStyles.None);
            Assert.AreEqual(expectedString, result.ToString("u"));
        }

        [TestMethod]
        public void DateTimeOffset_ParseExact_String_String_FormatProvider_DateTimeStyles_G()
        {
            DateTimeOffset expected = DateTimeOffset.MaxValue;
            string expectedString = expected.ToString("g");

            DateTimeOffset result = DateTimeOffset.ParseExact(expectedString, "g", null, DateTimeStyles.AssumeUniversal);
            Assert.AreEqual(expectedString, result.ToString("g"));
        }

        [TestMethod]
        public void DateTimeOffset_ParseExact_String_String_FormatProvider_DateTimeStyles_O_DataMemberTests()
        {
            foreach (var item in Format_String_TestData_O())
                ParseExact_String_String_FormatProvider_DateTimeStyles_O((DateTimeOffset)item[0], (string)item[1]);
        }

        private static void ParseExact_String_String_FormatProvider_DateTimeStyles_O(DateTimeOffset dt, string expected)
        {
            string actual = dt.ToString("o");
            Assert.AreEqual(expected, actual);

            DateTimeOffset result = DateTimeOffset.ParseExact(actual, "o", null, DateTimeStyles.None);
            Assert.AreEqual(expected, result.ToString("o"));
        }

        private static IEnumerable<object[]> Format_String_TestData_O()
        {
            yield return new object[] { DateTimeOffset.MaxValue, "9999-12-31T23:59:59.9999999+00:00" };
            yield return new object[] { DateTimeOffset.MinValue, "0001-01-01T00:00:00.0000000+00:00" };
            yield return new object[] { new DateTimeOffset(1906, 8, 15, 7, 24, 5, 300, new TimeSpan(0, 0, 0)), "1906-08-15T07:24:05.3000000+00:00" };
            yield return new object[] { new DateTimeOffset(1906, 8, 15, 7, 24, 5, 300, new TimeSpan(7, 30, 0)), "1906-08-15T07:24:05.3000000+07:30" };
        }

        [TestMethod]
        public void DateTimeOffset_ParseExact_String_String_FormatProvider_DateTimeStyles_R_DataMemberTests()
        {
            foreach (var item in Format_String_TestData_R())
                ParseExact_String_String_FormatProvider_DateTimeStyles_R((DateTimeOffset)item[0], (string)item[1]);
        }

        private static void ParseExact_String_String_FormatProvider_DateTimeStyles_R(DateTimeOffset dt, string expected)
        {
            string actual = dt.ToString("r", DateTimeFormatInfo.InvariantInfo);
            Assert.AreEqual(expected, actual);

            DateTimeOffset result = DateTimeOffset.ParseExact(actual, "r", DateTimeFormatInfo.InvariantInfo, DateTimeStyles.None);
            Assert.AreEqual(expected, result.ToString("r", DateTimeFormatInfo.InvariantInfo));
        }

        private static IEnumerable<object[]> Format_String_TestData_R()
        {
            yield return new object[] { DateTimeOffset.MaxValue, "Fri, 31 Dec 9999 23:59:59 GMT" };
            yield return new object[] { DateTimeOffset.MinValue, "Mon, 01 Jan 0001 00:00:00 GMT" };
            yield return new object[] { new DateTimeOffset(1906, 8, 15, 7, 24, 5, 300, new TimeSpan(0, 0, 0)), "Wed, 15 Aug 1906 07:24:05 GMT" };
            yield return new object[] { new DateTimeOffset(1906, 8, 15, 7, 24, 5, 300, new TimeSpan(7, 30, 0)), "Tue, 14 Aug 1906 23:54:05 GMT" };
        }

        [TestMethod]
        public void DateTimeOffset_ParseExact_String_String_FormatProvider_DateTimeStyles_R()
        {
            DateTimeOffset expected = DateTimeOffset.MaxValue;
            string expectedString = expected.ToString("r");

            DateTimeOffset result = DateTimeOffset.ParseExact(expectedString, "r", null, DateTimeStyles.None);
            Assert.AreEqual(expectedString, result.ToString("r"));
        }

        [TestMethod]
        public void DateTimeOffset_ParseExact_String_String_FormatProvider_DateTimeStyles_CustomFormatProvider()
        {
            var formatter = new MyFormatter();
            string dateBefore = DateTime.Now.ToString();

            DateTimeOffset dateAfter = DateTimeOffset.ParseExact(dateBefore, "G", formatter, DateTimeStyles.AssumeUniversal);
            Assert.AreEqual(dateBefore, dateAfter.DateTime.ToString());
        }

        [TestMethod]
        public void DateTimeOffset_ParseExact_String_StringArray_FormatProvider_DateTimeStyles()
        {
            DateTimeOffset expected = DateTimeOffset.MaxValue;
            string expectedString = expected.ToString("g");

            var formats = new string[] { "g" };
            DateTimeOffset result = DateTimeOffset.ParseExact(expectedString, formats, null, DateTimeStyles.AssumeUniversal);
            Assert.AreEqual(expectedString, result.ToString("g"));
        }

        [TestMethod]
        public void DateTimeOffset_TryParseExact_String_String_FormatProvider_DateTimeStyles_NullFormatProvider()
        {
            DateTimeOffset expected = DateTimeOffset.MaxValue;
            string expectedString = expected.ToString("g");

            DateTimeOffset resulted;
            Assert.IsTrue(DateTimeOffset.TryParseExact(expectedString, "g", null, DateTimeStyles.AssumeUniversal, out resulted));
            Assert.AreEqual(expectedString, resulted.ToString("g"));
        }

        [TestMethod]
        public void DateTimeOffset_TryParseExact_String_StringArray_FormatProvider_DateTimeStyles()
        {
            DateTimeOffset expected = DateTimeOffset.MaxValue;
            string expectedString = expected.ToString("g");

            var formats = new string[] { "g" };
            DateTimeOffset result;
            Assert.IsTrue(DateTimeOffset.TryParseExact(expectedString, formats, null, DateTimeStyles.AssumeUniversal, out result));
            Assert.AreEqual(expectedString, result.ToString("g"));
        }

        [TestMethod]
        public void DateTimeOffset_Parse_InvalidDateTimeStyle_ThrowsArgumentException_Data1()
            => Parse_InvalidDateTimeStyle_ThrowsArgumentException(~(DateTimeStyles.AllowLeadingWhite | DateTimeStyles.AllowTrailingWhite | DateTimeStyles.AllowInnerWhite | DateTimeStyles.NoCurrentDateDefault | DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeLocal | DateTimeStyles.AssumeUniversal | DateTimeStyles.RoundtripKind));

        [TestMethod]
        public void DateTimeOffset_Parse_InvalidDateTimeStyle_ThrowsArgumentException_Data2()
            => Parse_InvalidDateTimeStyle_ThrowsArgumentException(DateTimeStyles.NoCurrentDateDefault);

        private static void Parse_InvalidDateTimeStyle_ThrowsArgumentException(DateTimeStyles style)
        {
            AssertExtensions.Throws<ArgumentException>("styles", () => DateTimeOffset.Parse("06/08/1990", null, style));
            AssertExtensions.Throws<ArgumentException>("styles", () => DateTimeOffset.ParseExact("06/08/1990", "Y", null, style));

            DateTimeOffset dateTimeOffset = default(DateTimeOffset);
            AssertExtensions.Throws<ArgumentException>("styles", () => DateTimeOffset.TryParse("06/08/1990", null, style, out dateTimeOffset));
            Assert.AreEqual(default(DateTimeOffset), dateTimeOffset);

            AssertExtensions.Throws<ArgumentException>("styles", () => DateTimeOffset.TryParseExact("06/08/1990", "Y", null, style, out dateTimeOffset));
            Assert.AreEqual(default(DateTimeOffset), dateTimeOffset);
        }

        private static void VerifyDateTimeOffset(DateTimeOffset dateTimeOffset, int year, int month, int day, int hour, int minute, int second, int millisecond, TimeSpan? offset)
        {
            Assert.AreEqual(year, dateTimeOffset.Year);
            Assert.AreEqual(month, dateTimeOffset.Month);
            Assert.AreEqual(day, dateTimeOffset.Day);
            Assert.AreEqual(hour, dateTimeOffset.Hour);
            Assert.AreEqual(minute, dateTimeOffset.Minute);
            Assert.AreEqual(second, dateTimeOffset.Second);
            Assert.AreEqual(millisecond, dateTimeOffset.Millisecond);

            if (offset.HasValue)
            {
                Assert.AreEqual(offset.Value, dateTimeOffset.Offset);
            }
        }

        private class MyFormatter : IFormatProvider
        {
            public object GetFormat(Type formatType)
            {
                return typeof(IFormatProvider) == formatType ? this : null;
            }
        }

        public class TestTime
        {
            private TestTime(DateTimeOffset dateTimeOffset, long unixTimeMilliseconds, long unixTimeSeconds)
            {
                DateTimeOffset = dateTimeOffset;
                UnixTimeMilliseconds = unixTimeMilliseconds;
                UnixTimeSeconds = unixTimeSeconds;
            }

            public static TestTime FromMilliseconds(DateTimeOffset dateTimeOffset, long unixTimeMilliseconds)
            {
                long unixTimeSeconds = unixTimeMilliseconds / 1000;

                // Always round UnixTimeSeconds down toward 1/1/0001 00:00:00
                // (this happens automatically for unixTimeMilliseconds > 0)
                bool hasSubSecondPrecision = unixTimeMilliseconds % 1000 != 0;
                if (unixTimeMilliseconds < 0 && hasSubSecondPrecision)
                {
                    --unixTimeSeconds;
                }

                return new TestTime(dateTimeOffset, unixTimeMilliseconds, unixTimeSeconds);
            }

            public static TestTime FromSeconds(DateTimeOffset dateTimeOffset, long unixTimeSeconds)
            {
                return new TestTime(dateTimeOffset, unixTimeSeconds * 1000, unixTimeSeconds);
            }

            public DateTimeOffset DateTimeOffset { get; private set; }
            public long UnixTimeMilliseconds { get; private set; }
            public long UnixTimeSeconds { get; private set; }
        }

        [TestMethod]
        public void DateTimeOffset_Ctor_Calendar_TimeSpan()
        {
            var dateTimeOffset = new DateTimeOffset(1, 1, 1, 0, 0, 0, 0, new GregorianCalendar(), TimeSpan.Zero);
            VerifyDateTimeOffset(dateTimeOffset, 1, 1, 1, 0, 0, 0, 0, TimeSpan.Zero);
        }

        [TestMethod]
        public void DateTimeOffset_ToString()
        {
            const string testFormat = "yyyy-MM-ddTHH:mm:ss.fffffffzzz";
            var dtOffset = new Mock.System.DateTimeOffset(
                new DateTime(2017, 2, 11, 19, 11, 20, 228),
                new TimeSpan(-2, 0, 0));

            Assert.AreEqual("2017-02-11T19:11:20.2280000-02:00", dtOffset.ToString(testFormat));
        }
    }
}
