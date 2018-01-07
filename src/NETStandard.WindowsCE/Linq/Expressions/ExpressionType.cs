#if NET35_CF
namespace System.Linq.Expressions
#else
namespace Mock.System.Linq.Expressions
#endif
{
    /// <summary>
    /// The type of expression (used as a discriminator)
    /// </summary>
    public enum ExpressionType
    {
        /// <summary>
        /// Represents a constant value
        /// </summary>
        Constant,
        /// <summary>
        /// Represents a call to a method
        /// </summary>
        Call,
        /// <summary>
        /// Represents a lambda expression, complete with arguments
        /// </summary>
        Lambda,
        /// <summary>
        /// Represents a parameter used in an expression
        /// </summary>
        Parameter,
        /// <summary>
        /// Represents field/property access
        /// </summary>
        MemberAccess
    }
}