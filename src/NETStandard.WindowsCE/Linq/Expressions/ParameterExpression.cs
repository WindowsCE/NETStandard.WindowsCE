using System;

#if NET35_CF
namespace System.Linq.Expressions
#else
namespace Mock.System.Linq.Expressions
#endif
{
    /// <summary>
    /// Represents a parameter used in a lambda as an expression-tree node
    /// </summary>
    public class ParameterExpression : Expression
    {
        internal ParameterExpression(Type type, string name) : base(ExpressionType.Parameter)
        {
            Type = type;
            Name = name;
        }

        /// <summary>
        /// The type of value represented by this parameter
        /// </summary>
        public Type Type { get; }

        /// <summary>
        /// The name of the parameter
        /// </summary>
        public string Name { get; }
    }
}
