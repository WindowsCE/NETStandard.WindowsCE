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
    //[System.Runtime.CompilerServices.TypeForwardedFrom("System.Core, Version=3.5.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089")]
    public class LockRecursionException : Exception, ISerializable
    {
        public LockRecursionException()
        {
        }

        public LockRecursionException(string message)
            : base(message)
        {
        }

        public LockRecursionException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        protected LockRecursionException(SerializationInfo info, StreamingContext context)
            //: base(info, context)
        {
            ExceptionSerializer.SetObjectData(this, info, context);
        }

        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
                throw new ArgumentNullException(nameof(info));

            ExceptionSerializer.GetObjectData(this, info, context);
        }
    }
}