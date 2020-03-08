using System;

namespace System.Runtime.Serialization
{
    public class InvalidDataContractException : Exception
    {
        public InvalidDataContractException()
            : base()
        { }

        public InvalidDataContractException(string message)
            : base(message)
        { }

        public InvalidDataContractException(string message, Exception innerException)
            : base(message, innerException)
        { }
    }
}