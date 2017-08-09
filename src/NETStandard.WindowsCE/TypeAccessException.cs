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
    // TypeAccessException derives from TypeLoadException rather than MemberAccessException because in
    // pre-v4 releases of the runtime TypeLoadException was used in lieu of a TypeAccessException.
    [Serializable]
    public class TypeAccessException : TypeLoadException, ISerializable
    {
        private const int COR_E_TYPEACCESS = unchecked((int)0x80131543);
        private const string Arg_TypeAccessException = "Attempt to access the type failed.";

        public TypeAccessException()
            : base(Arg_TypeAccessException)
        {
            this.SetErrorCode(COR_E_TYPEACCESS);
        }

        public TypeAccessException(string message)
            : base(message)
        {
            this.SetErrorCode(COR_E_TYPEACCESS);
        }

        public TypeAccessException(string message, Exception inner)
            : base(message, inner)
        {
            this.SetErrorCode(COR_E_TYPEACCESS);
        }

        protected TypeAccessException(SerializationInfo info, StreamingContext context)
        {
            ExceptionSerializer.SetObjectData(this, info, context);
            this.SetErrorCode(COR_E_TYPEACCESS);
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            ExceptionSerializer.GetObjectData(this, info, context);
        }
    }
}