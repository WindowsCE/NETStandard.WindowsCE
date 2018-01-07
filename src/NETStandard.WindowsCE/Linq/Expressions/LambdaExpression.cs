using System.Collections.Generic;

#if NET35_CF
namespace System.Linq.Expressions
#else
namespace Mock.System.Linq.Expressions
#endif
{
    /// <summary>
    /// Represents an expression-tree
    /// </summary>
    public class LambdaExpression : Expression
    {
        internal LambdaExpression(Expression body, ParameterExpression[] args)
            : base(ExpressionType.Lambda)
        {
            Parameters = BuildList(args);
            Body = body;
        }
        /// <summary>
        /// The root operation for the expression to perform
        /// </summary>
        public Expression Body { get; }

        /// <summary>
        /// The parameters used in the expression (at any level)
        /// </summary>
        public List<ParameterExpression> Parameters { get; }
    }
}
