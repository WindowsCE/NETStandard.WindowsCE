using System;
using System.Runtime.InteropServices;
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
    /// <summary>
    /// The ArgumentException is thrown when an argument does not meet
    /// the contract of the method.  Ideally it should give a meaningful error
    /// message describing what was wrong and which parameter is incorrect.
    /// </summary>
    [ComVisible(true)]
    [Serializable]
    public class ArgumentException2 : ArgumentException, ISerializable
    {
        private readonly string _paramName;

        public virtual string ParamName
        {
            get { return _paramName; }
        }

        /// <summary>
        /// Creates a new ArgumentException with its message
        /// string set to the empty string.
        /// </summary>
        public ArgumentException2()
            : base()
        { }

        /// <summary>
        /// Creates a new ArgumentException with its message
        /// string set to message.
        /// </summary>
        public ArgumentException2(string message)
            : base(message)
        { }

        public ArgumentException2(string message, Exception innerException)
            : base(message, innerException)
        { }

        public ArgumentException2(string message, string paramName)
            : base(message, paramName)
        {
            _paramName = paramName;
        }

        public ArgumentException2(string message, string paramName, Exception innerException)
            : base(message, paramName)
        {
            _paramName = paramName;
            this.SetInnerException(innerException);
        }

        protected ArgumentException2(SerializationInfo info, StreamingContext context)
        {
            ExceptionSerializer.SetObjectData(this, info, context);
            _paramName = info.GetString("ParamName");
        }

        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
                throw new ArgumentNullException(nameof(info));

            ExceptionSerializer.GetObjectData(this, info, context);
            info.AddValue("ParamName", _paramName, typeof(string));
        }
    }
}
