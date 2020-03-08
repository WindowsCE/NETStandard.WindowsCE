using System;

namespace System.Runtime.Serialization
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = true, Inherited = true)]
    public sealed class KnownTypeAttribute : Attribute
    {
        private readonly string _methodName;
        private readonly Type _type;

        public string MethodName
        {
            get { return _methodName; }
        }

        public Type Type
        {
            get { return _type; }
        }

        private KnownTypeAttribute()
        { }

        public KnownTypeAttribute(Type type)
        {
            _type = type;
        }

        public KnownTypeAttribute(string methodName)
        {
            _methodName = methodName;
        }
    }
}
