// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

/*============================================================
**
**
**
** Purpose: A formatter-friendly mechanism for walking all of
** the data in a SerializationInfo.  Follows the IEnumerator 
** mechanism from Collections.
**
**
============================================================*/

using System;

#if NET35_CF
namespace System.Runtime.Serialization
#else
namespace Mock.System.Runtime.Serialization
#endif
{
    /// <summary>
    /// Holds the value, <see cref="Type" />, and name of a serialized object.
    /// </summary>
    public struct SerializationEntry
    {
        private Type m_type;
        private object m_value;
        private string m_name;

        /// <summary>
        /// Gets the value contained in the object.
        /// </summary>
        /// <returns>The value contained in the object.</returns>
        public object Value
        {
            get { return m_value; }
        }

        /// <summary>
        /// Gets the name of the object.
        /// </summary>
        /// <returns>The name of the object.</returns>
        public string Name
        {
            get { return m_name; }
        }

        /// <summary>
        /// Gets the <see cref="Type" /> of the object.
        /// </summary>
        /// <returns>The <see cref="Type" /> of the object.</returns>
        public Type ObjectType
        {
            get { return m_type; }
        }

        /// <summary>
        /// Initializes a new instance of <see cref="SerializationEntry"/> class.
        /// </summary>
        /// <param name="entryName">The name of object.</param>
        /// <param name="entryValue">The value contained in the object.</param>
        /// <param name="entryType">The <see cref="Type"/> of the object.</param>
        public SerializationEntry(string entryName, object entryValue, Type entryType)
        {
            m_value = entryValue;
            m_name = entryName;
            m_type = entryType;
        }
    }
}
