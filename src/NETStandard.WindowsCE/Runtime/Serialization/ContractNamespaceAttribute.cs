using System;

#if NET35_CF
namespace System.Runtime.Serialization
#else
namespace Mock.System.Runtime.Serialization
#endif
{
    [AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Module, AllowMultiple = true, Inherited = false)]
    public sealed class ContractNamespaceAttribute : Attribute
    {
        private readonly string _contractNamespace;

        public string ClrNamespace { get; set; }

        public string ContractNamespace
        {
            get { return _contractNamespace; }
        }

        public ContractNamespaceAttribute(string contractNamespace)
        {
            _contractNamespace = contractNamespace;
        }
    }
}
