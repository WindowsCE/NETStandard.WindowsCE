using System;

namespace System.Runtime.Serialization
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
    public sealed class DataMemberAttribute : Attribute
    {
        private int _order = -1;
        private bool _emitDefaultValue = true;
        private string _name;
        private bool _isNameSetExplicitly;
        private bool _isRequired;

        public string Name
        {
            get { return _name; }
            set
            {
                _name = value;
                _isNameSetExplicitly = true;
            }
        }

        public bool IsNameSetExplicitly
        {
            get { return _isNameSetExplicitly; }
        }

        public int Order
        {
            get { return _order; }
            set
            {
                if (value < 0)
                    throw new InvalidDataContractException("OrderCannotBeNegative");
                _order = value;
            }
        }

        public bool IsRequired
        {
            get { return _isRequired; }
            set { _isRequired = value; }
        }

        public bool EmitDefaultValue
        {
            get { return _emitDefaultValue; }
            set { _emitDefaultValue = value; }
        }
    }
}
