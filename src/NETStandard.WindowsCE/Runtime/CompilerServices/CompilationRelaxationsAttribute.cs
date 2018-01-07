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
        public CompilationRelaxationsAttribute(int relaxations)
        {
            CompilationRelaxations = relaxations;
        }

        public CompilationRelaxationsAttribute(CompilationRelaxations relaxations)
        {
            CompilationRelaxations = (int)relaxations;
        }

        public int CompilationRelaxations { get; }
    }
}
