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
    public sealed class TypeInitializationException : Exception2
    {
        private const string TypeInitialization_Default = "The type initializer for '{0}' threw an exception.";
        private readonly string _typeName;

        public string TypeName
            => _typeName ?? string.Empty;

        public TypeInitializationException(string fullTypeName, Exception innerException)
            : base(GetDefaultMessage(fullTypeName), innerException)
        {
            _typeName = fullTypeName;
        }

        private TypeInitializationException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            _typeName = info.GetString("TypeName");
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("TypeName", TypeName, typeof(string));
        }

        private static string GetDefaultMessage(string fullTypeName)
            => string.Format(TypeInitialization_Default, fullTypeName ?? string.Empty);
    }
}
