// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

/*============================================================
**
**
**
** Purpose: A base implementation of the IFormatterConverter
**          interface that uses the Convert class and the 
**          IConvertible interface.
**
**
============================================================*/

using System;
using System.Globalization;
using SConvert = System.Convert;

namespace System.Runtime.Serialization
{
    /// <summary>
    /// Represents a base implementation of the <see cref="IFormatterConverter"/>
    /// interface that uses the <see cref="SConvert"/> class.
    /// </summary>
    public class FormatterConverter : IFormatterConverter
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FormatterConverter"/> class.
        /// </summary>
        public FormatterConverter()
        { }

        /// <summary>
        /// Converts a value to the given <see cref="Type"/>.
        /// </summary>
        /// <param name="value">The object to convert.</param>
        /// <param name="type">The <see cref="Type"/> into which <paramref name="value"/> is converted.</param>
        /// <returns>The converted <paramref name="value"/> or null if the <paramref name="type"/> parameter is null.</returns>
        public object Convert(object value, Type type)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            return SConvert.ChangeType(value, type, CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Converts a value to the given <see cref="TypeCode"/>.
        /// </summary>
        /// <param name="value">The object to convert.</param>
        /// <param name="typeCode">The <see cref="TypeCode"/> into which <paramref name="value"/> is converted.</param>
        /// <returns>The converted <paramref name="value"/> or null if the <paramref name="typeCode"/> parameter is <see cref="TypeCode.Empty"/>.</returns>
        public object Convert(object value, TypeCode typeCode)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            return SConvert.ChangeType(value, typeCode, CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Converts a value to a <see cref="bool"/>.
        /// </summary>
        /// <param name="value">The object to convert.</param>
        /// <returns>The converted <paramref name="value"/>.</returns>
        public bool ToBoolean(object value)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            return SConvert.ToBoolean(value, CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Converts a value to a <see cref="char"/>.
        /// </summary>
        /// <param name="value">The object to convert.</param>
        /// <returns>The converted <paramref name="value"/>.</returns>
        public char ToChar(object value)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            return SConvert.ToChar(value, CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Converts a value to a <see cref="sbyte"/>.
        /// </summary>
        /// <param name="value">The object to convert.</param>
        /// <returns>The converted <paramref name="value"/>.</returns>
        [CLSCompliant(false)]
        public sbyte ToSByte(object value)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            return SConvert.ToSByte(value, CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Converts a value to a <see cref="byte"/>.
        /// </summary>
        /// <param name="value">The object to convert.</param>
        /// <returns>The converted <paramref name="value"/>.</returns>
        public byte ToByte(object value)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            return SConvert.ToByte(value, CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Converts a value to a <see cref="short"/>.
        /// </summary>
        /// <param name="value">The object to convert.</param>
        /// <returns>The converted <paramref name="value"/>.</returns>
        public short ToInt16(object value)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            return SConvert.ToInt16(value, CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Converts a value to a <see cref="ushort"/>.
        /// </summary>
        /// <param name="value">The object to convert.</param>
        /// <returns>The converted <paramref name="value"/>.</returns>
        [CLSCompliant(false)]
        public ushort ToUInt16(object value)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            return SConvert.ToUInt16(value, CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Converts a value to a <see cref="int"/>.
        /// </summary>
        /// <param name="value">The object to convert.</param>
        /// <returns>The converted <paramref name="value"/>.</returns>
        public int ToInt32(object value)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            return SConvert.ToInt32(value, CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Converts a value to a <see cref="uint"/>.
        /// </summary>
        /// <param name="value">The object to convert.</param>
        /// <returns>The converted <paramref name="value"/>.</returns>
        [CLSCompliant(false)]
        public uint ToUInt32(object value)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            return SConvert.ToUInt32(value, CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Converts a value to a <see cref="long"/>.
        /// </summary>
        /// <param name="value">The object to convert.</param>
        /// <returns>The converted <paramref name="value"/>.</returns>
        public long ToInt64(object value)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            return SConvert.ToInt64(value, CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Converts a value to a <see cref="ulong"/>.
        /// </summary>
        /// <param name="value">The object to convert.</param>
        /// <returns>The converted <paramref name="value"/>.</returns>
        [CLSCompliant(false)]
        public ulong ToUInt64(object value)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            return SConvert.ToUInt64(value, CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Converts a value to a <see cref="float"/>.
        /// </summary>
        /// <param name="value">The object to convert.</param>
        /// <returns>The converted <paramref name="value"/>.</returns>
        public float ToSingle(object value)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            return SConvert.ToSingle(value, CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Converts a value to a <see cref="double"/>.
        /// </summary>
        /// <param name="value">The object to convert.</param>
        /// <returns>The converted <paramref name="value"/>.</returns>
        public double ToDouble(object value)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            return SConvert.ToDouble(value, CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Converts a value to a <see cref="decimal"/>.
        /// </summary>
        /// <param name="value">The object to convert.</param>
        /// <returns>The converted <paramref name="value"/>.</returns>
        public decimal ToDecimal(object value)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            return SConvert.ToDecimal(value, CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Converts a value to a <see cref="DateTime"/>.
        /// </summary>
        /// <param name="value">The object to convert.</param>
        /// <returns>The converted <paramref name="value"/>.</returns>
        public DateTime ToDateTime(object value)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            return SConvert.ToDateTime(value, CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Converts a value to a <see cref="string"/>.
        /// </summary>
        /// <param name="value">The object to convert.</param>
        /// <returns>The converted <paramref name="value"/>.</returns>
        public string ToString(object value)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            return SConvert.ToString(value, CultureInfo.InvariantCulture);
        }
    }
}
