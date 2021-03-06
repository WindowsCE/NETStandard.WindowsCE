﻿using System;
using System.Runtime.Serialization;

using System.Runtime.ExceptionServices;

namespace System
{
    [Serializable]
    public class Exception2 : Exception, ISerializable
    {
        private string _stackTraceString;

        public Exception2()
            : base() { }

        public Exception2(string message)
            : base(message) { }

        public Exception2(string message, Exception innerException)
            : base(message, innerException) { }

        protected Exception2(SerializationInfo info, StreamingContext context)
        {
            ExceptionSerializer.SetObjectData(this, info, context);
        }

        public virtual string Source { get; set; }

        public override string StackTrace
        {
            get
            {
                if (_stackTraceString != null)
                    return _stackTraceString;

                return base.StackTrace;
            }
        }

        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            ExceptionSerializer.GetObjectData(this, info, context);
        }

        internal void SetStackTraceString(string stackTraceString)
        {
            _stackTraceString = stackTraceString;
        }
    }
}
