// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

/*============================================================
**
**
**
** Purpose: The structure for holding all of the data needed
**          for object serialization and deserialization.
**
**
===========================================================*/

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;

#if NET35_CF
namespace System.Runtime.Serialization
#else
namespace Mock.System.Runtime.Serialization
#endif
{
    /// <summary>
    /// Stores all the data needed to serialize or deserialize an object.
    /// </summary>
    [ComVisible(true)]
    public sealed class SerializationInfo
    {
        private const int defaultSize = 4;

        // Even though we have a dictionary, we're still keeping all the arrays around for back-compat. 
        // Otherwise we may run into potentially breaking behaviors like GetEnumerator() not returning entries in the same order they were added.
        internal string[] m_members;
        internal object[] m_data;
        internal Type[] m_types;
        private Dictionary<string, int> m_nameToIndex;
        internal int m_currMember;
        internal IFormatterConverter m_converter;
        private string m_fullTypeName;
        private Type objectType;
        private bool isFullTypeNameSetExplicit;

        #region Constructors

        /// <summary>
        /// Creates a new instance of the <see cref="SerializationInfo" /> class.
        /// </summary>
        /// <param name="type">The <see cref="Type" /> of the object to serialize.</param>
        /// <param name="converter">The <see cref="IFormatterConverter" /> used during deserialization.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="type" /> or <paramref name="converter" /> is null.
        /// </exception>
        [CLSCompliant(false)]
        public SerializationInfo(Type type, IFormatterConverter converter)
            : this(type, converter, false)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="SerializationInfo" /> class.
        /// </summary>
        /// <param name="type">The <see cref="Type" /> of the object to serialize.</param>
        /// <param name="converter">The <see cref="IFormatterConverter" /> used during deserialization.</param>
        /// <param name="requireSameTokenInPartialTrust">Indicates whether the object requires same token in partial trust.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="type" /> or <paramref name="converter" /> is null.
        /// </exception>
        /// <exception cref="NotSupportedException"><paramref name="requireSameTokenInPartialTrust"/> is true.</exception>
        [CLSCompliant(false)]
        public SerializationInfo(Type type, IFormatterConverter converter, bool requireSameTokenInPartialTrust)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            if (converter == null)
                throw new ArgumentNullException(nameof(converter));

            if (requireSameTokenInPartialTrust)
                throw new NotSupportedException("Cannot require token trust");

            objectType = type;
            m_fullTypeName = type.FullName;

            m_members = new string[defaultSize];
            m_data = new object[defaultSize];
            m_types = new Type[defaultSize];

            m_nameToIndex = new Dictionary<string, int>();

            m_converter = converter;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the full name of the <see cref="Type" /> to serialize.
        /// </summary>
        /// <returns>The full name of the type to serialize.</returns>
        /// <exception cref="ArgumentNullException">The value this property is set to is null.</exception>
        public string FullTypeName
        {
            get { return m_fullTypeName; }
            set
            {
                if (null == value)
                    throw new ArgumentNullException(nameof(value));

                m_fullTypeName = value;
                isFullTypeNameSetExplicit = true;
            }
        }

        /// <summary>
        /// Gets the number of members that have been added to the <see cref="SerializationInfo" /> store.
        /// </summary>
        /// <returns>
        /// The number of members that have been added to the current <see cref="SerializationInfo" />.
        /// </returns>
        public int MemberCount
        {
            get { return m_currMember; }
        }

        /// <summary>
        /// Returns the type of the object to be serialized.
        /// </summary>
        /// <returns>The type of the object being serialized.</returns>
        public Type ObjectType
        {
            get { return objectType; }
        }

        /// <summary>
        /// Gets whether the full type name has been explicitly set.
        /// </summary>
        /// <returns>
        /// True if the full type name has been explicitly set; otherwise false.
        /// </returns>
        public bool IsFullTypeNameSetExplicit
        {
            get { return isFullTypeNameSetExplicit; }
        }

        internal string[] MemberNames
        {
            get { return m_members; }
        }

        internal object[] MemberValues
        {
            get { return m_data; }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Sets the <see cref="Type" /> of the object to serialize.
        /// </summary>
        /// <param name="type">The <see cref="Type" /> of the object to serialize.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="type" /> parameter is null.</exception>
        public void SetType(Type type)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            if (!object.ReferenceEquals(objectType, type))
            {
                objectType = type;
                m_fullTypeName = type.FullName;
                isFullTypeNameSetExplicit = false;
            }
        }

        private static bool Compare(byte[] a, byte[] b)
        {
            // if either or both assemblies do not have public key token,
            // we should demand, hence, returning false will force a demand
            if (a == null || b == null ||
                a.Length == 0 || b.Length == 0 || a.Length != b.Length)
            {
                return false;
            }
            else
            {
                for (int i = 0; i < a.Length; i++)
                {
                    if (a[i] != b[i]) return false;
                }

                return true;
            }
        }

        private void ExpandArrays()
        {
            int newSize = (m_currMember * 2);

            //
            // In the pathological case, we may wrap
            //
            if (newSize < m_currMember)
            {
                if (int.MaxValue > m_currMember)
                {
                    newSize = int.MaxValue;
                }
            }

            //
            // Allocate more space and copy the data
            //
            string[] newMembers = new string[newSize];
            object[] newData = new object[newSize];
            Type[] newTypes = new Type[newSize];

            Array.Copy(m_members, newMembers, m_currMember);
            Array.Copy(m_data, newData, m_currMember);
            Array.Copy(m_types, newTypes, m_currMember);

            //
            // Assign the new arrys back to the member vars.
            //
            m_members = newMembers;
            m_data = newData;
            m_types = newTypes;
        }

        #endregion

        #region Enumeration

        /// <summary>
        /// Returns an enumerator used to iterate through the name-value pairs
        /// in the <see cref="SerializationInfo" /> store.
        /// </summary>
        /// <returns>
        /// An enumerator for parsing the name-value pairs contained in the
        /// <see cref="SerializationInfo" /> store.
        /// </returns>
        public SerializationInfoEnumerator GetEnumerator()
        {
            return new SerializationInfoEnumerator(m_members, m_data, m_types, m_currMember);
        }

        #endregion

        #region Add value

        public void AddValue(String name, Object value, Type type)
        {
            if (null == name)
                throw new ArgumentNullException(nameof(name));

            if ((object)type == null)
                throw new ArgumentNullException(nameof(type));

            AddValueInternal(name, value, type);
        }

        public void AddValue(string name, object value)
        {
            if (null == value)
                AddValue(name, value, typeof(object));
            else
                AddValue(name, value, value.GetType());
        }

        public void AddValue(string name, bool value)
        {
            AddValue(name, (object)value, typeof(bool));
        }

        public void AddValue(string name, char value)
        {
            AddValue(name, (object)value, typeof(char));
        }

        [CLSCompliant(false)]
        public void AddValue(string name, sbyte value)
        {
            AddValue(name, (object)value, typeof(sbyte));
        }

        public void AddValue(string name, byte value)
        {
            AddValue(name, (object)value, typeof(byte));
        }

        public void AddValue(string name, short value)
        {
            AddValue(name, (object)value, typeof(short));
        }

        [CLSCompliant(false)]
        public void AddValue(string name, ushort value)
        {
            AddValue(name, (object)value, typeof(ushort));
        }

        public void AddValue(String name, int value)
        {
            AddValue(name, (object)value, typeof(int));
        }

        [CLSCompliant(false)]
        public void AddValue(string name, uint value)
        {
            AddValue(name, (object)value, typeof(uint));
        }

        public void AddValue(string name, long value)
        {
            AddValue(name, (object)value, typeof(long));
        }

        [CLSCompliant(false)]
        public void AddValue(string name, ulong value)
        {
            AddValue(name, (object)value, typeof(ulong));
        }

        public void AddValue(string name, float value)
        {
            AddValue(name, (object)value, typeof(float));
        }

        public void AddValue(string name, double value)
        {
            AddValue(name, (object)value, typeof(double));
        }

        public void AddValue(string name, decimal value)
        {
            AddValue(name, (object)value, typeof(decimal));
        }

        public void AddValue(string name, DateTime value)
        {
            AddValue(name, (object)value, typeof(DateTime));
        }

        internal void AddValueInternal(string name, object value, Type type)
        {
            if (m_nameToIndex.ContainsKey(name))
                throw new SerializationException("Cannot add the same member twice to a SerializationInfo object.");

            m_nameToIndex.Add(name, m_currMember);

            //
            // If we need to expand the arrays, do so.
            //
            if (m_currMember >= m_members.Length)
            {
                ExpandArrays();
            }

            //
            // Add the data and then advance the counter.
            //
            m_members[m_currMember] = name;
            m_data[m_currMember] = value;
            m_types[m_currMember] = type;
            m_currMember++;
        }

        /*=================================UpdateValue==================================
        **Action: Finds the value if it exists in the current data.  If it does, we replace
        **        the values, if not, we append it to the end.  This is useful to the 
        **        ObjectManager when it's performing fixups.
        **Returns: void
        **Arguments: name  -- the name of the data to be updated.
        **           value -- the new value.
        **           type  -- the type of the data being added.
        **Exceptions: None.  All error checking is done with asserts. Although public in coreclr,
        **            it's not exposed in a contract and is only meant to be used by corefx.
        ==============================================================================*/
        internal void UpdateValue(string name, object value, Type type)
        {
            int index = FindElement(name);
            if (index < 0)
            {
                AddValueInternal(name, value, type);
            }
            else
            {
                m_data[index] = value;
                m_types[index] = type;
            }
        }

        #endregion

        #region Get value

        private int FindElement(string name)
        {
            if (null == name)
                throw new ArgumentNullException(nameof(name));

            int index;
            if (m_nameToIndex.TryGetValue(name, out index))
                return index;

            return -1;
        }

        /*==================================GetElement==================================
        **Action: Use FindElement to get the location of a particular member and then return
        **        the value of the element at that location.  The type of the member is
        **        returned in the foundType field.
        **Returns: The value of the element at the position associated with name.
        **Arguments: name -- the name of the element to find.
        **           foundType -- the type of the element associated with the given name.
        **Exceptions: None.  FindElement does null checking and throws for elements not 
        **            found.
        ==============================================================================*/
        private object GetElement(string name, out Type foundType)
        {
            int index = FindElement(name);
            if (index == -1)
                throw new SerializationException(string.Format("Cannot add the same member twice to a SerializationInfo object.", name));

            foundType = m_types[index];
            return m_data[index];
        }

        private object GetElementNoThrow(string name, out Type foundType)
        {
            int index = FindElement(name);
            if (index == -1)
            {
                foundType = null;
                return null;
            }

            foundType = m_types[index];
            return m_data[index];
        }

        //
        // The user should call one of these getters to get the data back in the 
        // form requested.  
        //

        public object GetValue(string name, Type type)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            Type foundType;
            object value;

            value = GetElement(name, out foundType);
            if (object.ReferenceEquals(foundType, type) ||
                type.IsAssignableFrom(foundType) ||
                value == null
                )
            {
                return value;
            }

            return m_converter.Convert(value, type);
        }

        internal object GetValueNoThrow(string name, Type type)
        {
            Type foundType;
            object value;

            value = GetElementNoThrow(name, out foundType);
            if (value == null)
                return null;

            if (object.ReferenceEquals(foundType, type) ||
                type.IsAssignableFrom(foundType) ||
                value == null)
            {
                return value;
            }

            return m_converter.Convert(value, type);
        }

        public bool GetBoolean(string name)
        {
            Type foundType;
            object value;

            value = GetElement(name, out foundType);
            if (object.ReferenceEquals(foundType, typeof(bool)))
                return (bool)value;

            return m_converter.ToBoolean(value);
        }

        public char GetChar(string name)
        {
            Type foundType;
            object value;

            value = GetElement(name, out foundType);
            if (object.ReferenceEquals(foundType, typeof(char)))
                return (char)value;

            return m_converter.ToChar(value);
        }

        [CLSCompliant(false)]
        public sbyte GetSByte(string name)
        {
            Type foundType;
            object value;

            value = GetElement(name, out foundType);
            if (object.ReferenceEquals(foundType, typeof(sbyte)))
                return (sbyte)value;

            return m_converter.ToSByte(value);
        }

        public byte GetByte(string name)
        {
            Type foundType;
            object value;

            value = GetElement(name, out foundType);
            if (object.ReferenceEquals(foundType, typeof(byte)))
                return (byte)value;

            return m_converter.ToByte(value);
        }

        public short GetInt16(string name)
        {
            Type foundType;
            object value;

            value = GetElement(name, out foundType);
            if (object.ReferenceEquals(foundType, typeof(short)))
                return (short)value;

            return m_converter.ToInt16(value);
        }

        [CLSCompliant(false)]
        public ushort GetUInt16(string name)
        {
            Type foundType;
            object value;

            value = GetElement(name, out foundType);
            if (object.ReferenceEquals(foundType, typeof(ushort)))
                return (ushort)value;

            return m_converter.ToUInt16(value);
        }

        public int GetInt32(string name)
        {
            Type foundType;
            object value;

            value = GetElement(name, out foundType);
            if (object.ReferenceEquals(foundType, typeof(int)))
                return (int)value;

            return m_converter.ToInt32(value);
        }

        [CLSCompliant(false)]
        public uint GetUInt32(string name)
        {
            Type foundType;
            object value;

            value = GetElement(name, out foundType);
            if (object.ReferenceEquals(foundType, typeof(uint)))
                return (uint)value;

            return m_converter.ToUInt32(value);
        }

        public long GetInt64(string name)
        {
            Type foundType;
            object value;

            value = GetElement(name, out foundType);
            if (object.ReferenceEquals(foundType, typeof(long)))
                return (long)value;

            return m_converter.ToInt64(value);
        }

        [CLSCompliant(false)]
        public ulong GetUInt64(string name)
        {
            Type foundType;
            object value;

            value = GetElement(name, out foundType);
            if (object.ReferenceEquals(foundType, typeof(ulong)))
                return (ulong)value;

            return m_converter.ToUInt64(value);
        }

        public float GetSingle(string name)
        {
            Type foundType;
            object value;

            value = GetElement(name, out foundType);
            if (object.ReferenceEquals(foundType, typeof(float)))
                return (float)value;

            return m_converter.ToSingle(value);
        }


        public double GetDouble(string name)
        {
            Type foundType;
            object value;

            value = GetElement(name, out foundType);
            if (object.ReferenceEquals(foundType, typeof(double)))
                return (double)value;

            return m_converter.ToDouble(value);
        }

        public decimal GetDecimal(string name)
        {
            Type foundType;
            object value;

            value = GetElement(name, out foundType);
            if (object.ReferenceEquals(foundType, typeof(decimal)))
                return (decimal)value;

            return m_converter.ToDecimal(value);
        }

        public DateTime GetDateTime(string name)
        {
            Type foundType;
            object value;

            value = GetElement(name, out foundType);
            if (object.ReferenceEquals(foundType, typeof(DateTime)))
                return (DateTime)value;

            return m_converter.ToDateTime(value);
        }

        public string GetString(string name)
        {
            Type foundType;
            object value;

            value = GetElement(name, out foundType);
            if (object.ReferenceEquals(foundType, typeof(string)) || value == null)
                return (string)value;

            return m_converter.ToString(value);
        }

        #endregion
    }
}
