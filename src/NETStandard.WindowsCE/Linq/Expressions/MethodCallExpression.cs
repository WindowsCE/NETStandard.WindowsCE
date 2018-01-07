using System.Collections.Generic;
using System.Reflection;

#if NET35_CF
namespace System.Linq.Expressions
#else
namespace Mock.System.Linq.Expressions
#endif
{
    /// <summary>
    /// Represents method invokation as an expression-tree node
    /// </summary>
    public class MethodCallExpression : Expression
    {
        internal MethodCallExpression(object instance, MethodInfo method, Expression[] args)
            : base(ExpressionType.Call)
        {
            Object = instance;
            Method = method;
            Arguments = BuildList(args);
        }

        /// <summary>
        /// The method to be invoked
        /// </summary>
        public MethodInfo Method { get; }

        /// <summary>
        /// The target object for the method (null if static)
        /// </summary>
        public object Object { get; }

        /// <summary>
        /// The arguments to be passed to the method
        /// </summary>
        public List<Expression> Arguments { get; }
    }
}
