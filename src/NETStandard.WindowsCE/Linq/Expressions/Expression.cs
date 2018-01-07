// Ref: http://protobuf-net.googlecode.com/

using System;
using System.Collections.Generic;
using System.Reflection;

#if NET35_CF
namespace System.Linq.Expressions
#else
namespace Mock.System.Linq.Expressions
#endif
{
    /// <summary>
    /// Represents a node in an expression tree
    /// </summary>
    public abstract class Expression
    {
        internal static List<T> BuildList<T>(T[] values) where T : Expression
        {
            return values == null || values.Length == 0
                ? new List<T>(0)
                : new List<T>(values);
        }

        internal Expression(ExpressionType nodeType)
        {
            NodeType = nodeType;
        }

        /// <summary>
        /// Indicates the type of the concrete expression
        /// </summary>
        public ExpressionType NodeType { get; private set; }

        /// <summary>
        /// Creates a new expression node represnting a constant value
        /// </summary>
        public static ConstantExpression Constant(object value)
        {
            return Constant(value, value?.GetType() ?? typeof(object));
        }

        /// <summary>
        /// Creates a new expression node represnting a constant value
        /// </summary>
        public static ConstantExpression Constant(object value, Type type)
        {
            return new ConstantExpression(value, type);
        }

        /// <summary>
        /// Creates a new parameter for use in an expression tree
        /// </summary>
        public static ParameterExpression Parameter(Type type, string name)
        {
            return new ParameterExpression(type, name);
        }

        /// <summary>
        /// Creates a new expression node representing method invokation 
        /// </summary>
        public static MethodCallExpression Call(Expression instance, MethodInfo method, params Expression[] args)
        {
            return new MethodCallExpression(instance, method, args);
        }

        /// <summary>
        /// Creates a completed expression tree
        /// </summary>
        public static LambdaExpression Lambda(Expression body, params ParameterExpression[] args)
        {
            return new LambdaExpression(body, args);
        }

        /// <summary>
        /// Creates a completed expression tree
        /// </summary>
        public static Expression<T> Lambda<T>(Expression body, params ParameterExpression[] args) where T : class
        {
            return new Expression<T>(body, args);
        }

        /// <summary>
        /// Creates a new expression node reading a value from a field
        /// </summary>
        public static MemberExpression Field(Expression instance, FieldInfo field)
        {
            return new MemberExpression(field, instance);
        }

        /// <summary>
        /// Creates a new expression node reading a value from a property
        /// </summary>
        public static MemberExpression Property(Expression instance, PropertyInfo field)
        {
            return new MemberExpression(field, instance);
        }

        /// <summary>
        /// Creates a new expression node reading a value from a property
        /// </summary>
        public static MemberExpression Property(Expression instance, MethodInfo accessor)
        {
            PropertyInfo property = null;
            if (accessor == null)
                return new MemberExpression(property, instance);

            Type declaringType = accessor.DeclaringType;
            foreach (PropertyInfo prop in declaringType.GetProperties(
                BindingFlags.Public | BindingFlags.NonPublic
                                    | (accessor.IsStatic ? BindingFlags.Static : BindingFlags.Instance)))
            {
                MethodInfo method;
                if (prop.CanRead &&
                    ((method = prop.GetGetMethod(true)) == accessor
                     || (declaringType.IsInterface && method.Name == accessor.Name)))
                {
                    property = prop;
                    break;
                }
            }
            return new MemberExpression(property, instance);
        }
    }
}