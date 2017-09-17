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
    /// The ArgumentException is thrown when an argument
    /// is null when it shouldn't be.
    /// </summary>
    [ComVisible(true)]
    [Serializable]
    public class ArgumentNullException2 : ArgumentNullException, ISerializable
    {
        private readonly string _paramName;

        /// <summary>
        /// Creates a new ArgumentNullException with its message
        /// string set to a default message explaining an argument was null.
        /// </summary>
        public ArgumentNullException2()
            : base()
        { }

        public ArgumentNullException2(string paramName)
            : base(paramName)
        {
            _paramName = paramName;
        }

        public ArgumentNullException2(string message, Exception innerException)
            : base(null, message)
        {
            this.SetInnerException(innerException);
        }

        public ArgumentNullException2(string paramName, string message)
            : base(paramName, message)
        {
            _paramName = paramName;
        }

        protected ArgumentNullException2(SerializationInfo info, StreamingContext context)
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
