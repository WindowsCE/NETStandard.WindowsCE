using System;
using System.Runtime.Serialization;

using System.Runtime.ExceptionServices;

namespace System
{
    [Serializable]
    public sealed class TypeInitializationException : Exception2
    {
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
            => string.Format(SR.TypeInitialization_Type, fullTypeName ?? string.Empty);
    }
}
