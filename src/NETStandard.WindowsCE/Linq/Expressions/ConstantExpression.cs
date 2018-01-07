using System;

#if NET35_CF
namespace System.Linq.Expressions
#else
namespace Mock.System.Linq.Expressions
#endif
{
    /// <summary>
    /// Represents a constant value as an expression-tree node
    /// </summary>
    public class ConstantExpression : Expression
    {
        internal ConstantExpression(object value, Type type) : base(ExpressionType.Constant)
        {
            Value = value;
            Type = type;
        }

        /// <summary>
        /// The type of value represented
        /// </summary>
        public Type Type { get; }

        /// <summary>
        /// The value represented
        /// </summary>
        public object Value { get; }
    }
}
