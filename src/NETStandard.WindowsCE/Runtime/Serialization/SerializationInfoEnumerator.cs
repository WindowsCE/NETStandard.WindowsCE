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
using System.Collections;
using System.Runtime.InteropServices;

#if NET35_CF
namespace System.Runtime.Serialization
#else
namespace Mock.System.Runtime.Serialization
#endif
{
    //
    // A simple enumerator over the values stored in the SerializationInfo.
    // This does not snapshot the values, it just keeps pointers to the 
    // member variables of the SerializationInfo that created it.
    //
    [ComVisible(true)]
    public sealed class SerializationInfoEnumerator : IEnumerator
    {
        string[] m_members;
        object[] m_data;
        Type[] m_types;
        int m_numItems;
        int m_currItem;
        bool m_current;

        internal SerializationInfoEnumerator(String[] members, Object[] info, Type[] types, int numItems)
        {
            m_members = members;
            m_data = info;
            m_types = types;
            //The MoveNext semantic is much easier if we enforce that [0..m_numItems] are valid entries
            //in the enumerator, hence we subtract 1.
            m_numItems = numItems - 1;
            m_currItem = -1;
            m_current = false;
        }

        public bool MoveNext()
        {
            if (m_currItem < m_numItems)
            {
                m_currItem++;
                m_current = true;
            }
            else
            {
                m_current = false;
            }
            return m_current;
        }

        /// <internalonly/>
        object IEnumerator.Current
        { //Actually returns a SerializationEntry
            get
            {
                if (m_current == false)
                {
                    throw new InvalidOperationException("InvalidOperation_EnumOpCantHappen");
                }
                return (object)(new SerializationEntry(m_members[m_currItem], m_data[m_currItem], m_types[m_currItem]));
            }
        }

        public SerializationEntry Current
        { //Actually returns a SerializationEntry
            get
            {
                if (m_current == false)
                {
                    throw new InvalidOperationException("InvalidOperation_EnumOpCantHappen");
                }
                return (new SerializationEntry(m_members[m_currItem], m_data[m_currItem], m_types[m_currItem]));
            }
        }

        public void Reset()
        {
            m_currItem = -1;
            m_current = false;
        }

        public string Name
        {
            get
            {
                if (m_current == false)
                {
                    throw new InvalidOperationException("InvalidOperation_EnumOpCantHappen");
                }
                return m_members[m_currItem];
            }
        }
        public object Value
        {
            get
            {
                if (m_current == false)
                {
                    throw new InvalidOperationException("InvalidOperation_EnumOpCantHappen");
                }
                return m_data[m_currItem];
            }
        }
        public Type ObjectType
        {
            get
            {
                if (m_current == false)
                {
                    throw new InvalidOperationException("InvalidOperation_EnumOpCantHappen");
                }
                return m_types[m_currItem];
            }
        }
    }
}