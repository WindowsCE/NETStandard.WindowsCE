using System.Reflection;

#if NET35_CF
namespace System.Linq.Expressions
#else
namespace Mock.System.Linq.Expressions
#endif
{
    /// <summary>
    /// Represents member access as an expression-tree node
    /// </summary>
    public class MemberExpression : Expression
    {
        /// <summary>
        /// The member to be accessed
        /// </summary>
        public MemberInfo Member { get; }

        /// <summary>
        /// The target instance holding the value (null if static)
        /// </summary>
        public Expression Expression { get; }

        internal MemberExpression(MemberInfo member, Expression expression)
            : base(ExpressionType.MemberAccess)
        {
            Member = member;
            Expression = expression;
        }
    }
}
