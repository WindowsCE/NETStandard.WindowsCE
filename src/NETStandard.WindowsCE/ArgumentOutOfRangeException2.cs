// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

/*=============================================================================
**
**
**
** Purpose: Exception class for method arguments outside of the legal range.
**
**
=============================================================================*/

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
    public class ArgumentOutOfRangeException2 : ArgumentOutOfRangeException, ISerializable
    {
        private object _actualValue;

        // Creates a new ArgumentOutOfRangeException with its message 
        // string set to a default message explaining an argument was out of range.
        public ArgumentOutOfRangeException2()
            : base()
        {
        }

        public ArgumentOutOfRangeException2(string paramName)
            : base(paramName)
        {
        }

        public ArgumentOutOfRangeException2(string paramName, string message)
            : base(paramName, message)
        {
        }

        public ArgumentOutOfRangeException2(string message, Exception innerException)
            : base(null, message)
        {
            this.SetInnerException(innerException);
        }

        // We will not use this in the classlibs, but we'll provide it for
        // anyone that's really interested so they don't have to stick a bunch
        // of printf's in their code.
        public ArgumentOutOfRangeException2(string paramName, object actualValue, string message)
            : base(paramName, message)
        {
            _actualValue = actualValue;
        }

        protected ArgumentOutOfRangeException2(SerializationInfo info, StreamingContext context)
        {
            ExceptionSerializer.SetObjectData(this, info, context);
            _actualValue = info.GetValue("ActualValue", typeof(object));
        }

        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
                throw new ArgumentNullException(nameof(info));

            ExceptionSerializer.GetObjectData(this, info, context);
            info.AddValue("ActualValue", _actualValue, typeof(object));
        }

        // Gets the value of the argument that caused the exception.
        // Note - we don't set this anywhere in the class libraries in 
        // version 1, but it might come in handy for other developers who
        // want to avoid sticking printf's in their code.
        public virtual object ActualValue
        {
            get { return _actualValue; }
        }
    }
}
