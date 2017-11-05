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
namespace System.Threading
#else
namespace Mock.System.Threading
#endif
{
    [Serializable]
    public class SemaphoreFullException : SystemException, ISerializable
    {
        public SemaphoreFullException()
            : base(SR.Threading_SemaphoreFullException)
        {
        }

        public SemaphoreFullException(String message)
            : base(message)
        {
        }

        public SemaphoreFullException(String message, Exception innerException)
            : base(message, innerException)
        {
        }

        protected SemaphoreFullException(SerializationInfo info, StreamingContext context)
        {
            ExceptionSerializer.SetObjectData(this, info, context);
        }

        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            ExceptionSerializer.GetObjectData(this, info, context);
        }
    }
}