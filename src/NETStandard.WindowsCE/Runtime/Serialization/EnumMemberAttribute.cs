using System;

#if NET35_CF
namespace System.Runtime.Serialization
#else
namespace Mock.System.Runtime.Serialization
#endif
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
    public sealed class EnumMemberAttribute : Attribute
    {
        private string _value;
        private bool _isValueSetExplicitly;

        public string Value
        {
            get { return _value; }
            set
            {
                _value = value;
                _isValueSetExplicitly = true;
            }
        }

        public bool IsValueSetExplicitly
        {
            get { return _isValueSetExplicitly; }
        }
    }
}
