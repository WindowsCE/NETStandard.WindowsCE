using System.Runtime.Serialization;

using System.Runtime.ExceptionServices;

namespace System.Threading
{
    /// <summary>
    /// The exception that is thrown when the post-phase action of a <see cref="Barrier"/> fails.
    /// </summary>
    [Serializable]
    //[System.Runtime.CompilerServices.TypeForwardedFrom("System, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089")]
    public class BarrierPostPhaseException : Exception, ISerializable
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BarrierPostPhaseException"/> class.
        /// </summary>
        public BarrierPostPhaseException()
            : this((string)null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BarrierPostPhaseException"/> class with the specified inner exception.
        /// </summary>
        /// <param name="innerException">The exception that is the cause of the current exception.</param>
        public BarrierPostPhaseException(Exception innerException)
            : this(null, innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BarrierPostPhaseException"/> class with a specified error message.
        /// </summary>
        /// <param name="message">A string that describes the exception.</param>
        public BarrierPostPhaseException(string message)
            : this(message, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BarrierPostPhaseException"/> class with a specified error message and inner exception.
        /// </summary>
        /// <param name="message">A string that describes the exception.</param>
        /// <param name="innerException">The exception that is the cause of the current exception.</param>
        public BarrierPostPhaseException(string message, Exception innerException)
            : base(message == null ? SR.BarrierPostPhaseException : message, innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the BarrierPostPhaseException class with serialized data.
        /// </summary>
        /// <param name="info">The object that holds the serialized object data.</param>
        /// <param name="context">The contextual information about the source or destination.</param>
        protected BarrierPostPhaseException(SerializationInfo info, StreamingContext context)
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
