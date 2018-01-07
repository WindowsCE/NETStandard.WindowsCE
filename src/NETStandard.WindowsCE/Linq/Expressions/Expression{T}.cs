#if NET35_CF
namespace System.Linq.Expressions
#else
namespace Mock.System.Linq.Expressions
#endif
{
    /// <summary>
    /// Represents an expression-tree
    /// </summary>
    public class Expression<T> : LambdaExpression where T : class
    {
        internal Expression(Expression body, ParameterExpression[] args)
            : base(body, args)
        {
        }
    }
}
