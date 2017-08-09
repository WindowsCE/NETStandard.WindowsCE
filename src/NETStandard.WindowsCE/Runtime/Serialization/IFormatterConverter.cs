// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

/*============================================================
**
**
**
** Purpose: The interface provides the connection between an
** instance of SerializationInfo and the formatter-provided
** class which knows how to parse the data inside the 
** SerializationInfo.
**
**
============================================================*/

using System;
using System.Runtime.InteropServices;

#if NET35_CF
namespace System.Runtime.Serialization
#else
namespace Mock.System.Runtime.Serialization
#endif
{
    /// <summary>
    /// Provides the connection between an instance of <see cref="SerializationInfo"/>
    /// and the formatter-provided class best suited to parse the data inside the <see cref="SerializationInfo"/>.
    /// </summary>
    [CLSCompliant(false), ComVisible(true)]
    public interface IFormatterConverter
    {
        /// <summary>
        /// Converts a value to the given <see cref="Type" />.
        /// </summary>
        /// <param name="value">The object to be converted.</param>
        /// <param name="type">The <see cref="Type" /> into which <paramref name="value" /> is to be converted.</param>
        /// <returns>The converted <paramref name="value" />.</returns>
        object Convert(object value, Type type);

        /// <summary>
        /// Converts a value to the given <see cref="TypeCode" />.
        /// </summary>
        /// <param name="value">The object to be converted.</param>
        /// <param name="typeCode">The <see cref="TypeCode" /> into which <paramref name="value" /> is to be converted.</param>
        /// <returns>The converted <paramref name="value" />.</returns>
        object Convert(object value, TypeCode typeCode);

        /// <summary>Converts a value to a <see cref="T:System.Boolean" />.</summary>
        /// <param name="value">The object to be converted.</param>
        /// <returns>The converted <paramref name="value" />.</returns>
        bool ToBoolean(object value);

        /// <summary>Converts a value to a Unicode character.</summary>
        /// <param name="value">The object to be converted.</param>
        /// <returns>The converted <paramref name="value" />.</returns>
        char ToChar(object value);

        /// <summary>Converts a value to a <see cref="T:System.SByte" />.</summary>
        /// <param name="value">The object to be converted.</param>
        /// <returns>The converted <paramref name="value" />.</returns>
        sbyte ToSByte(object value);

        /// <summary>Converts a value to an 8-bit unsigned integer.</summary>
        /// <param name="value">The object to be converted.</param>
        /// <returns>The converted <paramref name="value" />.</returns>
        byte ToByte(object value);

        /// <summary>Converts a value to a 16-bit signed integer.</summary>
        /// <param name="value">The object to be converted.</param>
        /// <returns>The converted <paramref name="value" />.</returns>
        short ToInt16(object value);

        /// <summary>Converts a value to a 16-bit unsigned integer.</summary>
        /// <param name="value">The object to be converted.</param>
        /// <returns>The converted <paramref name="value" />.</returns>
        ushort ToUInt16(object value);

        /// <summary>Converts a value to a 32-bit signed integer.</summary>
        /// <param name="value">The object to be converted.</param>
        /// <returns>The converted <paramref name="value" />.</returns>
        int ToInt32(object value);

        /// <summary>Converts a value to a 32-bit unsigned integer.</summary>
        /// <param name="value">The object to be converted.</param>
        /// <returns>The converted <paramref name="value" />.</returns>
        uint ToUInt32(object value);

        /// <summary>Converts a value to a 64-bit signed integer.</summary>
        /// <param name="value">The object to be converted.</param>
        /// <returns>The converted <paramref name="value" />.</returns>
        long ToInt64(object value);

        /// <summary>Converts a value to a 64-bit unsigned integer.</summary>
        /// <param name="value">The object to be converted.</param>
        /// <returns>The converted <paramref name="value" />.</returns>
        ulong ToUInt64(object value);

        /// <summary>Converts a value to a single-precision floating-point number.</summary>
        /// <param name="value">The object to be converted.</param>
        /// <returns>The converted <paramref name="value" />.</returns>
        float ToSingle(object value);

        /// <summary>Converts a value to a double-precision floating-point number.</summary>
        /// <param name="value">The object to be converted.</param>
        /// <returns>The converted <paramref name="value" />.</returns>
        double ToDouble(object value);

        /// <summary>Converts a value to a <see cref="T:System.Decimal" />.</summary>
        /// <param name="value">The object to be converted.</param>
        /// <returns>The converted <paramref name="value" />.</returns>
        decimal ToDecimal(object value);

        /// <summary>Converts a value to a <see cref="T:System.DateTime" />.</summary>
        /// <param name="value">The object to be converted.</param>
        /// <returns>The converted <paramref name="value" />.</returns>
        DateTime ToDateTime(object value);

        /// <summary>Converts a value to a <see cref="T:System.String" />.</summary>
        /// <param name="value">The object to be converted.</param>
        /// <returns>The converted <paramref name="value" />.</returns>
        string ToString(object value);
    }
}
