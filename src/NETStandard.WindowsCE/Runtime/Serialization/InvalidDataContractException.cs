using System;

#if NET35_CF
namespace System.Runtime.Serialization
#else
namespace Mock.System.Runtime.Serialization
#endif
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