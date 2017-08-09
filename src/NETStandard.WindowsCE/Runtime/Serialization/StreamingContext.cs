// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

/*============================================================
**
** ValueType: StreamingContext
**
**
** Purpose: A value type indicating the source or destination of our streaming.
**
**
===========================================================*/

using System;

#if NET35_CF
namespace System.Runtime.Serialization
#else
namespace Mock.System.Runtime.Serialization
#endif
{
    /// <summary>
    /// Describes the source and destination of a given serialized stream, and
    /// provides an additional caller-defined context.
    /// </summary>
    [Serializable]
    public struct StreamingContext
    {
        private object m_additionalContext;
        private StreamingContextStates m_state;

        /// <summary>
        /// Initializes a new instance of the <see cref="StreamingContext"/>
        /// class with a given context state.
        /// </summary>
        /// <param name="state">
        /// A bitwise combination of the <see cref="StreamingContextStates"/>
        /// values that specify the source or destination context for this
        /// <see cref="StreamingContext"/>.
        /// </param>
        public StreamingContext(StreamingContextStates state)
            : this(state, null)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="StreamingContext"/>
        /// class with a given context state.
        /// </summary>
        /// <param name="state">
        /// A bitwise combination of the <see cref="StreamingContextStates"/>
        /// values that specify the source or destination context for this
        /// <see cref="StreamingContext"/>.
        /// </param>
        /// <param name="additional">
        /// Any additional information to be associated with the <see cref="StreamingContext"/>.
        /// This information is available to any object that implements
        /// <see cref="T:System.Runtime.Serialization.ISerializable"/> or any
        /// serialization surrogate.
        /// Most users do not need to set this parameter.
        /// </param>
        public StreamingContext(StreamingContextStates state, object additional)
        {
            m_state = state;
            m_additionalContext = additional;
        }

        /// <summary>
        /// Gets context specified as part of the additional context.
        /// </summary>
        public object Context
        {
            get { return m_additionalContext; }
        }

        /// <summary>
        /// Determines whether two <see cref="StreamingContext"/> instances contain the same values.
        /// </summary>
        /// <param name="obj">An object to compare with the current instance.</param>
        /// <returns>
        /// true if the specified object is an instance of <see cref="StreamingContext"/>
        /// and equals the value of the current instance; otherwise, false.
        /// </returns>
        public override bool Equals(object obj)
        {
            if (!(obj is StreamingContext))
                return false;

            if (((StreamingContext)obj).m_additionalContext == m_additionalContext &&
                ((StreamingContext)obj).m_state == m_state)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Returns a hash code of this object.
        /// </summary>
        /// <returns>
        /// The <see cref="StreamingContextStates"/> value that contains the
        /// source or destination of the serialization for this
        /// <see cref="StreamingContext"/>.
        /// </returns>
        public override int GetHashCode()
        {
            return (int)m_state;
        }

        /// <summary>
        /// Gets the source or destination of the transmitted data.
        /// </summary>
        public StreamingContextStates State
        {
            get { return m_state; }
        }
    }
}
