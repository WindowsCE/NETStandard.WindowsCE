// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Runtime.Serialization;

#if NET35_CF
using System.Runtime.ExceptionServices;
#else
using Mock.System.Runtime.ExceptionServices;
#endif

#if NET35_CF
namespace System
#else
namespace Mock.System
#endif
{
    [Serializable]
    public class InvalidTimeZoneException : Exception, ISerializable
    {
        public InvalidTimeZoneException(string message)
            : base(message) { }

        public InvalidTimeZoneException(string message, Exception innerException)
            : base(message, innerException) { }

        protected InvalidTimeZoneException(SerializationInfo info, StreamingContext context)
        {
            ExceptionSerializer.SetObjectData(this, info, context);
        }

        public InvalidTimeZoneException()
            : base() { }

        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            ExceptionSerializer.GetObjectData(this, info, context);
        }
    }
}