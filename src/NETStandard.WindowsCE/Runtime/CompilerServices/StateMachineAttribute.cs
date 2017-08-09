using System;

#if NET35_CF
namespace System.Runtime.CompilerServices
#else
namespace Mock.System.Runtime.CompilerServices
#endif
{
    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    public class StateMachineAttribute : Attribute
    {
        private readonly Type _stateMachineType;

        public StateMachineAttribute(Type stateMachineType)
        {
            _stateMachineType = stateMachineType;
        }

        public Type StateMachineType
            => _stateMachineType;
    }
}
