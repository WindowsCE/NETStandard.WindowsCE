// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Runtime.Serialization;
using System;

using System.Runtime.ExceptionServices;

namespace System.Threading
{
    [Serializable]
    //[System.Runtime.CompilerServices.TypeForwardedFrom("mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089")]
    public class WaitHandleCannotBeOpenedException : ApplicationException, ISerializable
    {
        public WaitHandleCannotBeOpenedException()
            : base(SR.Threading_WaitHandleCannotBeOpenedException)
        {
            HResult = HResults.COR_E_WAITHANDLECANNOTBEOPENED;
        }

        public WaitHandleCannotBeOpenedException(String message)
            : base(message)
        {
            HResult = HResults.COR_E_WAITHANDLECANNOTBEOPENED;
        }

        public WaitHandleCannotBeOpenedException(String message, Exception innerException)
            : base(message, innerException)
        {
            HResult = HResults.COR_E_WAITHANDLECANNOTBEOPENED;
        }

        protected WaitHandleCannotBeOpenedException(SerializationInfo info, StreamingContext context)
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