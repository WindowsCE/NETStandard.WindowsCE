using System;

namespace System.Runtime.CompilerServices
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
