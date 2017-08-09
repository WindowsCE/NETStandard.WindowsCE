using System;

#if NET35_CF
namespace System.Runtime.CompilerServices
#else
namespace Mock.System.Runtime.CompilerServices
#endif
{
    [AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Module | AttributeTargets.Class | AttributeTargets.Method)]
    public class CompilationRelaxationsAttribute : Attribute
    {
        private readonly int _relaxations;

        public CompilationRelaxationsAttribute(int relaxations)
        {
            _relaxations = relaxations;
        }

        public int CompilationRelaxations
            => _relaxations;
    }
}
