using System;

namespace System.Runtime.CompilerServices
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
