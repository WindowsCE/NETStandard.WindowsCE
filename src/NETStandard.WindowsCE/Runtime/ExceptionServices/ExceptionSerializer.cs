using System;
using System.Collections;
using System.Globalization;
using System.Reflection;
using System.Runtime.Serialization;

#if NET35_CF
namespace System.Runtime.ExceptionServices
#else
namespace Mock.System.Runtime.ExceptionServices
#endif
{
    /// <summary>
    /// Provides methods to help serialization of <see cref="Exception"/> instance.
    /// </summary>
    public static class ExceptionSerializer
    {
        private static readonly FieldInfo HResultFieldInfo;
        private static readonly FieldInfo InnerExceptionFieldInfo;
        private static readonly FieldInfo MessageFieldInfo;
        private static readonly FieldInfo MethodDescsFieldInfo;
        private static readonly string DefaultClassName;
        private static readonly int DefaultHResult;

        private static string DefaultMessage;
        private static CultureInfo DefaultMessageCulture;

        static ExceptionSerializer()
        {
            const BindingFlags flags = BindingFlags.GetField |
                BindingFlags.Instance |
                BindingFlags.NonPublic;
            Type t_Exception = typeof(Exception);
            DefaultClassName = t_Exception.FullName;
            HResultFieldInfo = t_Exception.GetField("_HResult", flags);
            InnerExceptionFieldInfo = t_Exception.GetField("_innerException", flags);
            MessageFieldInfo = t_Exception.GetField("_message", flags);
            MethodDescsFieldInfo = t_Exception.GetField("_methodDescs", flags);

            Exception ex = new Exception();
            DefaultHResult = GetHResult(ex);
            DefaultMessage = ex.Message;
            DefaultMessageCulture = CultureInfo.CurrentUICulture;
        }

        /// <summary>
        /// Populates a <see cref="SerializationInfo"/> with the data
        /// needed to serialize the target object.
        /// </summary>
        /// <param name="ex">The target object.</param>
        /// <param name="info">The <see cref="SerializationInfo"/> to populate with data.</param>
        /// <param name="context">The destination for this serialization.</param>
        public static void GetObjectData(Exception ex, SerializationInfo info, StreamingContext context)
        {
            if (ex == null)
                throw new ArgumentNullException(nameof(ex));
            if (info == null)
                throw new ArgumentNullException(nameof(info));

            info.AddValue("Message", ex.Message, typeof(string));
            info.AddValue("InnerException", AsSerializable(ex.InnerException), typeof(Exception));
            info.AddValue("StackTraceString", ex.StackTrace, typeof(string));
            info.AddValue("HResult", GetHResult(ex));
            info.AddValue("MethodDescs", GetMethodDescs(ex), typeof(IntPtr[]));

            string source = null;
            Exception2 ex2 = ex as Exception2;
            if (ex2 != null)
                source = ex2.Source;
            info.AddValue("Source", source, typeof(string));

            if (context.State == StreamingContextStates.Remoting)
            {
                // Compatibility with full framework
                info.AddValue("ClassName", DefaultClassName, typeof(string));
                info.AddValue("Data", null, typeof(IDictionary));
                info.AddValue("HelpURL", null, typeof(string));
                info.AddValue("RemoteStackTraceString", null, typeof(string));
                info.AddValue("RemoteStackIndex", 0, typeof(int));
                info.AddValue("ExceptionMethod", null, typeof(string));
            }
        }

        /// <summary>
        /// Reads a <see cref="SerializationInfo"/> data which contains
        /// the data needed to deserialize to a new instance.
        /// </summary>
        /// <param name="ex">The new instance.</param>
        /// <param name="info">The <see cref="SerializationInfo"/> to read data.</param>
        /// <param name="context">The destination for this serialization.</param>
        public static void SetObjectData(Exception ex, SerializationInfo info, StreamingContext context)
        {
            if (ex == null)
                throw new ArgumentNullException(nameof(ex));
            if (info == null)
                throw new ArgumentNullException(nameof(info));

            string message = info.GetString("Message");
            Exception inner = (Exception)info.GetValue("InnerException", typeof(Exception));
            string stackTrace = info.GetString("StackTraceString");
            int hResult = info.GetInt32("HResult");
            IntPtr[] methodDescs = (IntPtr[])info.GetValue("MethodDescs", typeof(IntPtr[]));
            string source = info.GetString("Source");

            ex.SetMessage(message);
            ex.SetInnerException(inner);
            HResultFieldInfo.SetValue(ex, hResult);
            MethodDescsFieldInfo.SetValue(ex, methodDescs);

            Exception2 ex2 = ex as Exception2;
            if (ex2 != null)
            {
                ex2.SetStackTraceString(stackTrace);
                ex2.Source = source;
            }
        }

        /// <summary>
        /// Wraps a <see cref="Exception"/> instance into a new serializable type.
        /// </summary>
        /// <param name="ex">The target instance.</param>
        /// <returns>A new <see cref="Exception2"/> instance.</returns>
        public static Exception AsSerializable(Exception ex)
        {
            if (ex is ISerializable)
                return ex;

            Exception2 ex2 = new Exception2(ex.Message, ex.InnerException);
            HResultFieldInfo.SetValue(ex2, GetHResult(ex));
            MethodDescsFieldInfo.SetValue(ex2, GetMethodDescs(ex));
            return ex2;
        }

        private static IntPtr[] GetMethodDescs(Exception ex)
        {
            if (ex == null)
                throw new ArgumentNullException(nameof(ex));

            return (IntPtr[])MethodDescsFieldInfo.GetValue(ex);
        }

        /// <summary>
        /// Gets the default message set for <see cref="Exception"/> when no
        /// message is defined.
        /// </summary>
        /// <returns>The default message for current culture.</returns>
        public static string GetDefaultMessage()
        {
            CultureInfo currentCulture = CultureInfo.CurrentUICulture;
            if (currentCulture != DefaultMessageCulture)
            {
                DefaultMessage = new Exception().Message;
                DefaultMessageCulture = currentCulture;
            }

            return DefaultMessage;
        }

        private static int GetHResult(Exception ex)
        {
            if (ex == null)
                throw new ArgumentNullException(nameof(ex));

            if (HResultFieldInfo == null)
                return DefaultHResult;

            return (int)HResultFieldInfo.GetValue(ex);
        }

        /// <summary>
        /// Sets the error code for a <see cref="Exception"/>.
        /// </summary>
        /// <param name="ex">The target object.</param>
        /// <param name="hResult">The error code.</param>
        public static void SetErrorCode(this Exception ex, int hResult)
        {
            if (ex == null)
                throw new ArgumentNullException(nameof(ex));

            HResultFieldInfo.SetValue(ex, hResult);
        }

        /// <summary>
        /// Sets the message for a <see cref="Exception"/>.
        /// </summary>
        /// <param name="ex">The target object.</param>
        /// <param name="message">The message text to set.</param>
        public static void SetMessage(this Exception ex, string message)
        {
            if (ex == null)
                throw new ArgumentNullException(nameof(ex));

            MessageFieldInfo.SetValue(ex, message);
        }

        /// <summary>
        /// Sets the inner exception for a <see cref="Exception"/>.
        /// </summary>
        /// <param name="ex">The target object.</param>
        /// <param name="innerException">The inner exception to set.</param>
        public static void SetInnerException(this Exception ex, Exception innerException)
        {
            if (ex == null)
                throw new ArgumentNullException(nameof(ex));

            InnerExceptionFieldInfo.SetValue(ex, innerException);
        }
    }
}
