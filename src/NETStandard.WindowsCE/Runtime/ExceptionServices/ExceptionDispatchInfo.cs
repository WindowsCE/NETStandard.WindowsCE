using System;
using System.Reflection;

#if NET35_CF
namespace System.Runtime.ExceptionServices
#else
namespace Mock.System.Runtime.ExceptionServices
#endif
{
    /// <summary>
    /// The ExceptionDispatchInfo object stores the stack trace information and
    /// Watson information that the exception contains at the point where it is
    /// captured. The exception can be thrown at another time and possibly on
    /// another thread by calling the ExceptionDispatchInfo.Throw method. The
    /// exception is thrown as if it had flowed from the point where it was
    /// captured to the point where the Throw method is called.
    /// </summary>
    public sealed class ExceptionDispatchInfo
    {
        private static FieldInfo _stackTraceField;

        private Exception _exception;
        private object _stackTraceOriginal;

        private ExceptionDispatchInfo(Exception exception)
        {
            _exception = exception;
            FieldInfo remoteStackTraceString = GetFieldInfo();
            _stackTraceOriginal = remoteStackTraceString.GetValue(_exception);
        }

        /// <summary>
        /// Creates an ExceptionDispatchInfo object that represents the specified exception at the current point in code.
        /// </summary>
        /// <param name="source">The exception whose state is captured, and which is represented by the returned object.</param>
        /// <returns>An object that represents the specified exception at the current point in code. </returns>
        public static ExceptionDispatchInfo Capture(Exception source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            return new ExceptionDispatchInfo(source);
        }

        /// <summary>
        /// Gets the exception that is represented by the current instance.
        /// </summary>
        public Exception SourceException
        {
            get { return _exception; }
        }

        private static FieldInfo GetFieldInfo()
        {
            if (_stackTraceField == null)
            {
                // ---
                // Code by Miguel de Icaza

                FieldInfo remoteStackTraceString =
                    remoteStackTraceString = typeof(Exception).GetField("_methodDescs",
                        BindingFlags.Instance | BindingFlags.NonPublic); // .NetCompact

                if (remoteStackTraceString == null)
                    remoteStackTraceString = typeof(Exception).GetField("_stackTrace",
                        BindingFlags.Instance | BindingFlags.NonPublic); // MS.Net

                // ---
                _stackTraceField = remoteStackTraceString;
            }
            return _stackTraceField;
        }

        private static void SetStackTrace(Exception exception, object value)
        {
            FieldInfo remoteStackTraceString = GetFieldInfo();
            remoteStackTraceString.SetValue(exception, value);
        }

        /// <summary>
        /// Throws the exception that is represented by the current ExceptionDispatchInfo object, after restoring the state that was saved when the exception was captured.
        /// </summary>
        public void Throw()
        {
            try
            {
                throw _exception;
            }
            catch (Exception exception)
            {
                GC.KeepAlive(exception);
                SetStackTrace(_exception, _stackTraceOriginal);
                throw;
            }
        }
    }
}
