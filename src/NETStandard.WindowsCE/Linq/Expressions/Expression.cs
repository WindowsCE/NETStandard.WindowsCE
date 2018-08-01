//
// Expression.cs
//
// Author:
//   Jb Evain (jbevain@novell.com)
//   Miguel de Icaza (miguel@novell.com)
//
// (C) 2008 Novell, Inc. (http://www.novell.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Reflection;

#if NET35_CF
namespace System.Linq.Expressions
#else
namespace Mock.System.Linq.Expressions
#endif
{
    public abstract class Expression
    {
        internal const BindingFlags PublicInstance = BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy;
        internal const BindingFlags NonPublicInstance = BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy;
        internal const BindingFlags PublicStatic = BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy;
        internal const BindingFlags AllInstance = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy;
        internal const BindingFlags AllStatic = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.FlattenHierarchy;
        internal const BindingFlags All = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance | BindingFlags.FlattenHierarchy;

        public ExpressionType NodeType { get; }

        public Type Type { get; }

        protected Expression(ExpressionType nodeType, Type type)
        {
            NodeType = nodeType;
            Type = type;
        }

        public override string ToString()
        {
            return ExpressionPrinter.ToString(this);
        }

        private static MethodInfo GetUnaryOperator(string operName, Type declaring, Type param)
        {
            return GetUnaryOperator(operName, declaring, param, null);
        }

        private static MethodInfo GetUnaryOperator(string operName, Type declaring, Type param, Type ret)
        {
            var methods = declaring.GetNotNullableType().GetMethods(PublicStatic);

            foreach (MethodInfo method in methods)
            {
                if (method.Name != operName)
                    continue;

                var parameters = method.GetParameters();
                if (parameters.Length != 1)
                    continue;

                if (method.IsGenericMethod)
                    continue;

                if (!IsAssignableToParameterType(param.GetNotNullableType(), parameters[0]))
                    continue;

                if (ret != null && method.ReturnType != ret.GetNotNullableType())
                    continue;

                return method;
            }

            return null;
        }

        internal static MethodInfo GetTrueOperator(Type self)
        {
            return GetBooleanOperator("op_True", self);
        }

        internal static MethodInfo GetFalseOperator(Type self)
        {
            return GetBooleanOperator("op_False", self);
        }

        private static MethodInfo GetBooleanOperator(string op, Type self)
        {
            return GetUnaryOperator(op, self, self, typeof(bool));
        }

        private static bool IsAssignableToParameterType(Type type, ParameterInfo param)
        {
            Type ptype = param.ParameterType;
            if (ptype.IsByRef)
                ptype = ptype.GetElementType();

            return type.GetNotNullableType().IsAssignableTo(ptype);
        }

        private static MethodInfo CheckUnaryMethod(MethodInfo method, Type param)
        {
            if (method.ReturnType == typeof(void))
                throw new ArgumentException("Specified method must return a value", nameof(method));

            if (!method.IsStatic)
                throw new ArgumentException("Method must be static", nameof(method));

            var parameters = method.GetParameters();

            if (parameters.Length != 1)
                throw new ArgumentException("Must have only one parameters", nameof(method));

            if (!IsAssignableToParameterType(param.GetNotNullableType(), parameters[0]))
                throw new InvalidOperationException("left-side argument type does not match expression type");

            return method;
        }

        private static MethodInfo UnaryCoreCheck(string operName, Expression expression, MethodInfo method, Func<Type, bool> validator)
        {
            if (expression == null)
                throw new ArgumentNullException(nameof(expression));

            if (method != null)
                return CheckUnaryMethod(method, expression.Type);

            Type type = expression.Type.GetNotNullableType();

            if (validator(type))
                return null;

            if (operName != null)
            {
                method = GetUnaryOperator(operName, type, expression.Type);
                if (method != null)
                    return method;
            }

            throw new InvalidOperationException(
                $"Operation {operName?.Substring(3) ?? "is"} not defined for {expression.Type}");
        }

        private static MethodInfo GetBinaryOperator(string operName, Type onType, Expression left, Expression right)
        {
            MethodInfo[] methods = onType.GetMethods(PublicStatic);

            foreach (MethodInfo method in methods)
            {
                if (method.Name != operName)
                    continue;

                var parameters = method.GetParameters();
                if (parameters.Length != 2)
                    continue;

                if (method.IsGenericMethod)
                    continue;

                if (!IsAssignableToParameterType(left.Type, parameters[0]))
                    continue;

                if (!IsAssignableToParameterType(right.Type, parameters[1]))
                    continue;

                // Method has papers in order.
                return method;
            }

            return null;
        }

        //
        // Performs basic checks on the incoming expressions for binary expressions
        // and any provided MethodInfo.
        //
        private static MethodInfo BinaryCoreCheck(string operName, Expression left, Expression right, MethodInfo method)
        {
            if (left == null)
                throw new ArgumentNullException(nameof(left));
            if (right == null)
                throw new ArgumentNullException(nameof(right));

            if (method != null)
            {
                if (method.ReturnType == typeof(void))
                    throw new ArgumentException("Specified method must return a value", nameof(method));

                if (!method.IsStatic)
                    throw new ArgumentException("Method must be static", nameof(method));

                var parameters = method.GetParameters();

                if (parameters.Length != 2)
                    throw new ArgumentException("Must have only two parameters", nameof(method));

                if (!IsAssignableToParameterType(left.Type, parameters[0]))
                    throw new InvalidOperationException("left-side argument type does not match left expression type");

                if (!IsAssignableToParameterType(right.Type, parameters[1]))
                    throw new InvalidOperationException("right-side argument type does not match right expression type");

                return method;
            }
            else
            {
                Type ltype = left.Type;
                Type rtype = right.Type;
                Type ultype = ltype.GetNotNullableType();
                Type urtype = rtype.GetNotNullableType();

                if (operName == "op_BitwiseOr" || operName == "op_BitwiseAnd")
                {
                    if (ultype == typeof(bool))
                    {
                        if (ultype == urtype && ltype == rtype)
                            return null;
                    }
                }

                // Use IsNumber to avoid expensive reflection.
                if (IsNumber(ultype))
                {
                    if (ultype == urtype && ltype == rtype)
                        return null;

                    if (operName != null)
                    {
                        method = GetBinaryOperator(operName, urtype, left, right);
                        if (method != null)
                            return method;
                    }
                }

                if (operName != null)
                {
                    method = GetBinaryOperator(operName, ultype, left, right);
                    if (method != null)
                        return method;
                }

                if (operName == "op_Equality" || operName == "op_Inequality")
                {
                    //
                    // == and != allow reference types without operators defined.
                    //
                    if (!ltype.IsValueType && !rtype.IsValueType)
                        return null;

                    if (ltype == rtype && ultype == typeof(bool))
                        return null;
                }

                if (operName == "op_LeftShift" || operName == "op_RightShift")
                {
                    if (IsInt(ultype) && urtype == typeof(int))
                        return null;
                }

                throw new InvalidOperationException(
                    $"Operation {operName?.Substring(3) ?? "is"} not defined for {ltype} and {rtype}");
            }
        }

        //
        // This is like BinaryCoreCheck, but if no method is used adds the restriction that
        // only ints and bools are allowed
        //
        private static MethodInfo BinaryBitwiseCoreCheck(string operName, Expression left, Expression right, MethodInfo method)
        {
            if (left == null)
                throw new ArgumentNullException(nameof(left));
            if (right == null)
                throw new ArgumentNullException(nameof(right));

            if (method == null)
            {
                // avoid reflection shortcut and catches Ints/bools before we check Numbers in general
                if (left.Type == right.Type && IsIntOrBool(left.Type))
                    return null;
            }

            method = BinaryCoreCheck(operName, left, right, method);
            if (method != null)
                return method;

            // The check in BinaryCoreCheck allows a bit more than we do
            // (floats and doubles).  Catch this here
            if (left.Type == typeof(double) || left.Type == typeof(float))
                throw new InvalidOperationException("Types not supported");

            return null;
        }

        private static BinaryExpression MakeSimpleBinary(ExpressionType et, Expression left, Expression right, MethodInfo method)
        {
            bool isLifted;
            Type type;

            if (method == null)
            {
                isLifted = left.Type.IsNullable();
                type = left.Type;
            }
            else
            {
                var parameters = method.GetParameters();

                ParameterInfo lp = parameters[0];
                ParameterInfo rp = parameters[1];

                if (IsAssignableToOperatorParameter(left, lp) && IsAssignableToOperatorParameter(right, rp))
                {
                    isLifted = false;
                    type = method.ReturnType;
                }
                else if (left.Type.IsNullable()
                  && right.Type.IsNullable()
                  && left.Type.GetNotNullableType() == lp.ParameterType
                  && right.Type.GetNotNullableType() == rp.ParameterType
                  && !method.ReturnType.IsNullable())
                {
                    isLifted = true;
                    type = method.ReturnType.MakeNullableType();
                }
                else
                    throw new InvalidOperationException();
            }

            return new BinaryExpression(et, type, left, right, isLifted, isLifted, method, null);
        }

        private static bool IsAssignableToOperatorParameter(Expression expression, ParameterInfo parameter)
        {
            if (expression.Type == parameter.ParameterType)
                return true;

            return !expression.Type.IsNullable()
                   && !parameter.ParameterType.IsNullable()
                   && IsAssignableToParameterType(expression.Type, parameter);
        }

        private static UnaryExpression MakeSimpleUnary(ExpressionType et, Expression expression, MethodInfo method)
        {
            bool isLifted;
            Type type;

            if (method == null)
            {
                type = expression.Type;
                isLifted = type.IsNullable();
            }
            else
            {
                ParameterInfo parameter = method.GetParameters()[0];

                if (IsAssignableToOperatorParameter(expression, parameter))
                {
                    isLifted = false;
                    type = method.ReturnType;
                }
                else if (expression.Type.IsNullable()
                  && expression.Type.GetNotNullableType() == parameter.ParameterType
                  && !method.ReturnType.IsNullable())
                {
                    isLifted = true;
                    type = method.ReturnType.MakeNullableType();
                }
                else
                {
                    throw new InvalidOperationException();
                }
            }

            return new UnaryExpression(et, expression, type, method, isLifted);
        }

        private static BinaryExpression MakeBoolBinary(ExpressionType et, Expression left, Expression right, bool liftToNull, MethodInfo method)
        {
            bool isLifted;
            Type type;

            if (method == null)
            {
                if (!left.Type.IsNullable() && !right.Type.IsNullable())
                {
                    isLifted = false;
                    liftToNull = false;
                    type = typeof(bool);
                }
                else if (left.Type.IsNullable() && right.Type.IsNullable())
                {
                    isLifted = true;
                    type = liftToNull ? typeof(bool?) : typeof(bool);
                }
                else
                {
                    throw new InvalidOperationException();
                }
            }
            else
            {
                var parameters = method.GetParameters();

                ParameterInfo lp = parameters[0];
                ParameterInfo rp = parameters[1];

                if (IsAssignableToOperatorParameter(left, lp) && IsAssignableToOperatorParameter(right, rp))
                {
                    isLifted = false;
                    liftToNull = false;
                    type = method.ReturnType;
                }
                else if (left.Type.IsNullable()
                  && right.Type.IsNullable()
                  && left.Type.GetNotNullableType() == lp.ParameterType
                  && right.Type.GetNotNullableType() == rp.ParameterType)
                {
                    isLifted = true;

                    if (method.ReturnType == typeof(bool))
                        type = liftToNull ? typeof(bool?) : typeof(bool);
                    else if (!method.ReturnType.IsNullable())
                    {
                        //
                        // This behavior is not documented: what
                        // happens if the result is not typeof(bool), but
                        // the parameters are nullable: the result
                        // becomes nullable<returntype>
                        //
                        // See:
                        // https://connect.microsoft.com/VisualStudio/feedback/ViewFeedback.aspx?FeedbackID=323139

                        type = method.ReturnType.MakeNullableType();
                    }
                    else
                    {
                        throw new InvalidOperationException();
                    }
                }
                else
                {
                    throw new InvalidOperationException();
                }
            }

            return new BinaryExpression(et, type, left, right, liftToNull, isLifted, method, null);
        }

        //
        // Arithmetic
        //
        public static BinaryExpression Add(Expression left, Expression right)
        {
            return Add(left, right, null);
        }

        public static BinaryExpression Add(Expression left, Expression right, MethodInfo method)
        {
            method = BinaryCoreCheck("op_Addition", left, right, method);
            return MakeSimpleBinary(ExpressionType.Add, left, right, method);
        }

        public static BinaryExpression AddChecked(Expression left, Expression right)
        {
            return AddChecked(left, right, null);
        }

        public static BinaryExpression AddChecked(Expression left, Expression right, MethodInfo method)
        {
            method = BinaryCoreCheck("op_Addition", left, right, method);

            if (method != null)
                return MakeSimpleBinary(ExpressionType.AddChecked, left, right, method);

            // The check in BinaryCoreCheck allows a bit more than we do
            // (byte, sbyte).  Catch that here
            if (left.Type == typeof(byte) || left.Type == typeof(sbyte))
                throw new InvalidOperationException($"AddChecked not defined for {left.Type} and {right.Type}");

            return MakeSimpleBinary(ExpressionType.AddChecked, left, right, null);
        }

        public static BinaryExpression Subtract(Expression left, Expression right)
        {
            return Subtract(left, right, null);
        }

        public static BinaryExpression Subtract(Expression left, Expression right, MethodInfo method)
        {
            method = BinaryCoreCheck("op_Subtraction", left, right, method);
            return MakeSimpleBinary(ExpressionType.Subtract, left, right, method);
        }

        public static BinaryExpression SubtractChecked(Expression left, Expression right)
        {
            return SubtractChecked(left, right, null);
        }

        public static BinaryExpression SubtractChecked(Expression left, Expression right, MethodInfo method)
        {
            method = BinaryCoreCheck("op_Subtraction", left, right, method);

            if (method != null)
                return MakeSimpleBinary(ExpressionType.SubtractChecked, left, right, method);

            // The check in BinaryCoreCheck allows a bit more than we do
            // (byte, sbyte).  Catch that here
            if (left.Type == typeof(byte) || left.Type == typeof(sbyte))
                throw new InvalidOperationException($"SubtractChecked not defined for {left.Type} and {right.Type}");

            return MakeSimpleBinary(ExpressionType.SubtractChecked, left, right, null);
        }

        public static BinaryExpression Modulo(Expression left, Expression right)
        {
            return Modulo(left, right, null);
        }

        public static BinaryExpression Modulo(Expression left, Expression right, MethodInfo method)
        {
            method = BinaryCoreCheck("op_Modulus", left, right, method);
            return MakeSimpleBinary(ExpressionType.Modulo, left, right, method);
        }

        public static BinaryExpression Multiply(Expression left, Expression right)
        {
            return Multiply(left, right, null);
        }

        public static BinaryExpression Multiply(Expression left, Expression right, MethodInfo method)
        {
            method = BinaryCoreCheck("op_Multiply", left, right, method);
            return MakeSimpleBinary(ExpressionType.Multiply, left, right, method);
        }

        public static BinaryExpression MultiplyChecked(Expression left, Expression right)
        {
            return MultiplyChecked(left, right, null);
        }

        public static BinaryExpression MultiplyChecked(Expression left, Expression right, MethodInfo method)
        {
            method = BinaryCoreCheck("op_Multiply", left, right, method);
            return MakeSimpleBinary(ExpressionType.MultiplyChecked, left, right, method);
        }

        public static BinaryExpression Divide(Expression left, Expression right)
        {
            return Divide(left, right, null);
        }

        public static BinaryExpression Divide(Expression left, Expression right, MethodInfo method)
        {
            method = BinaryCoreCheck("op_Division", left, right, method);
            return MakeSimpleBinary(ExpressionType.Divide, left, right, method);
        }

        public static BinaryExpression Power(Expression left, Expression right)
        {
            return Power(left, right, null);
        }

        public static BinaryExpression Power(Expression left, Expression right, MethodInfo method)
        {
            method = BinaryCoreCheck(null, left, right, method);

            if (left.Type.GetNotNullableType() != typeof(double))
                throw new InvalidOperationException("Power only supports double arguments");

            return MakeSimpleBinary(ExpressionType.Power, left, right, method);
        }

        //
        // Bitwise
        //
        public static BinaryExpression And(Expression left, Expression right)
        {
            return And(left, right, null);
        }

        public static BinaryExpression And(Expression left, Expression right, MethodInfo method)
        {
            method = BinaryBitwiseCoreCheck("op_BitwiseAnd", left, right, method);
            return MakeSimpleBinary(ExpressionType.And, left, right, method);
        }

        public static BinaryExpression Or(Expression left, Expression right)
        {
            return Or(left, right, null);
        }

        public static BinaryExpression Or(Expression left, Expression right, MethodInfo method)
        {
            method = BinaryBitwiseCoreCheck("op_BitwiseOr", left, right, method);
            return MakeSimpleBinary(ExpressionType.Or, left, right, method);
        }

        public static BinaryExpression ExclusiveOr(Expression left, Expression right)
        {
            return ExclusiveOr(left, right, null);
        }

        public static BinaryExpression ExclusiveOr(Expression left, Expression right, MethodInfo method)
        {
            method = BinaryBitwiseCoreCheck("op_ExclusiveOr", left, right, method);
            return MakeSimpleBinary(ExpressionType.ExclusiveOr, left, right, method);
        }

        public static BinaryExpression LeftShift(Expression left, Expression right)
        {
            return LeftShift(left, right, null);
        }

        public static BinaryExpression LeftShift(Expression left, Expression right, MethodInfo method)
        {
            method = BinaryBitwiseCoreCheck("op_LeftShift", left, right, method);
            return MakeSimpleBinary(ExpressionType.LeftShift, left, right, method);
        }

        public static BinaryExpression RightShift(Expression left, Expression right)
        {
            return RightShift(left, right, null);
        }

        public static BinaryExpression RightShift(Expression left, Expression right, MethodInfo method)
        {
            method = BinaryCoreCheck("op_RightShift", left, right, method);
            return MakeSimpleBinary(ExpressionType.RightShift, left, right, method);
        }

        //
        // Short-circuit
        //
        public static BinaryExpression AndAlso(Expression left, Expression right)
        {
            return AndAlso(left, right, null);
        }

        public static BinaryExpression AndAlso(Expression left, Expression right, MethodInfo method)
        {
            method = ConditionalBinaryCheck("op_BitwiseAnd", left, right, method);
            return MakeBoolBinary(ExpressionType.AndAlso, left, right, true, method);
        }

        private static MethodInfo ConditionalBinaryCheck(string oper, Expression left, Expression right, MethodInfo method)
        {
            method = BinaryCoreCheck(oper, left, right, method);

            if (method == null)
            {
                if (left.Type.GetNotNullableType() != typeof(bool))
                    throw new InvalidOperationException("Only booleans are allowed");
            }
            else
            {
                Type type = left.Type.GetNotNullableType();

                // The method should have identical parameter and return types.
                if (left.Type != right.Type || method.ReturnType != type)
                    throw new ArgumentException("left, right and return type must match");

                MethodInfo optrue = GetTrueOperator(type);
                MethodInfo opfalse = GetFalseOperator(type);

                if (optrue == null || opfalse == null)
                    throw new ArgumentException("Operators true and false are required but not defined");
            }

            return method;
        }

        public static BinaryExpression OrElse(Expression left, Expression right)
        {
            return OrElse(left, right, null);
        }

        public static BinaryExpression OrElse(Expression left, Expression right, MethodInfo method)
        {
            method = ConditionalBinaryCheck("op_BitwiseOr", left, right, method);
            return MakeBoolBinary(ExpressionType.OrElse, left, right, true, method);
        }

        //
        // Comparison
        //
        public static BinaryExpression Equal(Expression left, Expression right)
        {
            return Equal(left, right, false, null);
        }

        public static BinaryExpression Equal(Expression left, Expression right, bool liftToNull, MethodInfo method)
        {
            method = BinaryCoreCheck("op_Equality", left, right, method);
            return MakeBoolBinary(ExpressionType.Equal, left, right, liftToNull, method);
        }

        public static BinaryExpression NotEqual(Expression left, Expression right)
        {
            return NotEqual(left, right, false, null);
        }


        public static BinaryExpression NotEqual(Expression left, Expression right, bool liftToNull, MethodInfo method)
        {
            method = BinaryCoreCheck("op_Inequality", left, right, method);
            return MakeBoolBinary(ExpressionType.NotEqual, left, right, liftToNull, method);
        }

        public static BinaryExpression GreaterThan(Expression left, Expression right)
        {
            return GreaterThan(left, right, false, null);
        }

        public static BinaryExpression GreaterThan(Expression left, Expression right, bool liftToNull, MethodInfo method)
        {
            method = BinaryCoreCheck("op_GreaterThan", left, right, method);
            return MakeBoolBinary(ExpressionType.GreaterThan, left, right, liftToNull, method);
        }

        public static BinaryExpression GreaterThanOrEqual(Expression left, Expression right)
        {
            return GreaterThanOrEqual(left, right, false, null);
        }

        public static BinaryExpression GreaterThanOrEqual(Expression left, Expression right, bool liftToNull, MethodInfo method)
        {
            method = BinaryCoreCheck("op_GreaterThanOrEqual", left, right, method);
            return MakeBoolBinary(ExpressionType.GreaterThanOrEqual, left, right, liftToNull, method);
        }

        public static BinaryExpression LessThan(Expression left, Expression right)
        {
            return LessThan(left, right, false, null);
        }

        public static BinaryExpression LessThan(Expression left, Expression right, bool liftToNull, MethodInfo method)
        {
            method = BinaryCoreCheck("op_LessThan", left, right, method);
            return MakeBoolBinary(ExpressionType.LessThan, left, right, liftToNull, method);
        }

        public static BinaryExpression LessThanOrEqual(Expression left, Expression right)
        {
            return LessThanOrEqual(left, right, false, null);
        }

        public static BinaryExpression LessThanOrEqual(Expression left, Expression right, bool liftToNull, MethodInfo method)
        {
            method = BinaryCoreCheck("op_LessThanOrEqual", left, right, method);
            return MakeBoolBinary(ExpressionType.LessThanOrEqual, left, right, liftToNull, method);
        }

        //
        // Miscelaneous
        //

        private static void CheckArray(Expression array)
        {
            if (array == null)
                throw new ArgumentNullException(nameof(array));

            if (!array.Type.IsArray)
                throw new ArgumentException("The array argument must be of type array");
        }

        public static BinaryExpression ArrayIndex(Expression array, Expression index)
        {
            CheckArray(array);

            if (index == null)
                throw new ArgumentNullException(nameof(index));
            if (array.Type.GetArrayRank() != 1)
                throw new ArgumentException("The array argument must be a single dimensional array");
            if (index.Type != typeof(int))
                throw new ArgumentException("The index must be of type int");

            return new BinaryExpression(ExpressionType.ArrayIndex, array.Type.GetElementType(), array, index);
        }

        public static BinaryExpression Coalesce(Expression left, Expression right)
        {
            return Coalesce(left, right, null);
        }

        private static BinaryExpression MakeCoalesce(Expression left, Expression right)
        {
            Type result = null;

            if (left.Type.IsNullable())
            {
                Type lbase = left.Type.GetNotNullableType();

                if (!right.Type.IsNullable() && right.Type.IsAssignableTo(lbase))
                    result = lbase;
            }

            if (result == null && right.Type.IsAssignableTo(left.Type))
                result = left.Type;

            if (result == null)
            {
                if (left.Type.IsNullable() && left.Type.GetNotNullableType().IsAssignableTo(right.Type))
                    result = right.Type;
            }

            if (result == null)
                throw new ArgumentException("Incompatible argument types");

            return new BinaryExpression(ExpressionType.Coalesce, result, left, right, false, false, null, null);
        }

        private static BinaryExpression MakeConvertedCoalesce(Expression left, Expression right, LambdaExpression conversion)
        {
            MethodInfo invoke = conversion.Type.GetInvokeMethod();

            CheckNotVoid(invoke.ReturnType);

            if (invoke.ReturnType != right.Type)
                throw new InvalidOperationException("Conversion return type doesn't march right type");

            var parameters = invoke.GetParameters();

            if (parameters.Length != 1)
                throw new ArgumentException("Conversion has wrong number of parameters");

            if (!IsAssignableToParameterType(left.Type, parameters[0]))
                throw new InvalidOperationException("Conversion argument doesn't marcht left type");

            return new BinaryExpression(ExpressionType.Coalesce, right.Type, left, right, false, false, null, conversion);
        }

        public static BinaryExpression Coalesce(Expression left, Expression right, LambdaExpression conversion)
        {
            if (left == null)
                throw new ArgumentNullException(nameof(left));
            if (right == null)
                throw new ArgumentNullException(nameof(right));

            //
            // First arg must ne nullable (either Nullable<T> or a reference type
            //
            if (left.Type.IsValueType && !left.Type.IsNullable())
                throw new InvalidOperationException("Left expression can never be null");

            return conversion != null
                ? MakeConvertedCoalesce(left, right, conversion)
                : MakeCoalesce(left, right);
        }

        //
        // MakeBinary constructors
        //
        public static BinaryExpression MakeBinary(ExpressionType binaryType, Expression left, Expression right)
        {
            return MakeBinary(binaryType, left, right, false, null);
        }

        public static BinaryExpression MakeBinary(ExpressionType binaryType, Expression left, Expression right, bool liftToNull, MethodInfo method)
        {
            return MakeBinary(binaryType, left, right, liftToNull, method, null);
        }

        public static BinaryExpression MakeBinary(ExpressionType binaryType, Expression left, Expression right, bool liftToNull, MethodInfo method, LambdaExpression conversion)
        {
            switch (binaryType)
            {
                case ExpressionType.Add:
                    return Add(left, right, method);
                case ExpressionType.AddChecked:
                    return AddChecked(left, right, method);
                case ExpressionType.AndAlso:
                    return AndAlso(left, right);
                case ExpressionType.Coalesce:
                    return Coalesce(left, right, conversion);
                case ExpressionType.Divide:
                    return Divide(left, right, method);
                case ExpressionType.Equal:
                    return Equal(left, right, liftToNull, method);
                case ExpressionType.ExclusiveOr:
                    return ExclusiveOr(left, right, method);
                case ExpressionType.GreaterThan:
                    return GreaterThan(left, right, liftToNull, method);
                case ExpressionType.GreaterThanOrEqual:
                    return GreaterThanOrEqual(left, right, liftToNull, method);
                case ExpressionType.LeftShift:
                    return LeftShift(left, right, method);
                case ExpressionType.LessThan:
                    return LessThan(left, right, liftToNull, method);
                case ExpressionType.LessThanOrEqual:
                    return LessThanOrEqual(left, right, liftToNull, method);
                case ExpressionType.Modulo:
                    return Modulo(left, right, method);
                case ExpressionType.Multiply:
                    return Multiply(left, right, method);
                case ExpressionType.MultiplyChecked:
                    return MultiplyChecked(left, right, method);
                case ExpressionType.NotEqual:
                    return NotEqual(left, right, liftToNull, method);
                case ExpressionType.OrElse:
                    return OrElse(left, right);
                case ExpressionType.Power:
                    return Power(left, right, method);
                case ExpressionType.RightShift:
                    return RightShift(left, right, method);
                case ExpressionType.Subtract:
                    return Subtract(left, right, method);
                case ExpressionType.SubtractChecked:
                    return SubtractChecked(left, right, method);
                case ExpressionType.And:
                    return And(left, right, method);
                case ExpressionType.Or:
                    return Or(left, right, method);
            }

            throw new ArgumentException("MakeBinary expect a binary node type");
        }

        public static MethodCallExpression ArrayIndex(Expression array, params Expression[] indexes)
        {
            return ArrayIndex(array, indexes as IEnumerable<Expression>);
        }

        public static MethodCallExpression ArrayIndex(Expression array, IEnumerable<Expression> indexes)
        {
            CheckArray(array);

            if (indexes == null)
                throw new ArgumentNullException(nameof(indexes));

            var args = indexes.ToReadOnlyCollection();
            if (array.Type.GetArrayRank() != args.Count)
                throw new ArgumentException("The number of arguments doesn't match the rank of the array");

            foreach (Expression arg in args)
                if (arg.Type != typeof(int))
                    throw new ArgumentException("The index must be of type int");

            return Call(array, array.Type.GetMethod("Get", PublicInstance), args);
        }

        public static UnaryExpression ArrayLength(Expression array)
        {
            if (array == null)
                throw new ArgumentNullException(nameof(array));
            if (!array.Type.IsArray)
                throw new ArgumentException("The type of the expression must me Array");
            if (array.Type.GetArrayRank() != 1)
                throw new ArgumentException("The array must be a single dimensional array");

            return new UnaryExpression(ExpressionType.ArrayLength, array, typeof(int));
        }

        public static MemberAssignment Bind(MemberInfo member, Expression expression)
        {
            if (member == null)
                throw new ArgumentNullException(nameof(member));
            if (expression == null)
                throw new ArgumentNullException(nameof(expression));

            Type type = null;

            if (member is PropertyInfo prop && prop.GetSetMethod(true) != null)
                type = prop.PropertyType;

            if (member is FieldInfo field)
                type = field.FieldType;

            if (type == null)
                throw new ArgumentException("member");

            if (!expression.Type.IsAssignableTo(type))
                throw new ArgumentException("member");

            return new MemberAssignment(member, expression);
        }

        public static MemberAssignment Bind(MethodInfo propertyAccessor, Expression expression)
        {
            if (propertyAccessor == null)
                throw new ArgumentNullException(nameof(propertyAccessor));
            if (expression == null)
                throw new ArgumentNullException(nameof(expression));

            PropertyInfo prop = GetAssociatedProperty(propertyAccessor);
            if (prop == null)
                throw new ArgumentException("propertyAccessor");

            MethodInfo setter = prop.GetSetMethod(true);
            if (setter == null)
                throw new ArgumentException("setter");

            if (!expression.Type.IsAssignableTo(prop.PropertyType))
                throw new ArgumentException("member");

            return new MemberAssignment(prop, expression);
        }

        public static MethodCallExpression Call(Expression instance, MethodInfo method)
        {
            return Call(instance, method, null as IEnumerable<Expression>);
        }

        public static MethodCallExpression Call(MethodInfo method, params Expression[] arguments)
        {
            return Call(null, method, arguments as IEnumerable<Expression>);
        }

        public static MethodCallExpression Call(Expression instance, MethodInfo method, params Expression[] arguments)
        {
            return Call(instance, method, arguments as IEnumerable<Expression>);
        }

        public static MethodCallExpression Call(Expression instance, MethodInfo method, IEnumerable<Expression> arguments)
        {
            if (method == null)
                throw new ArgumentNullException(nameof(method));
            if (instance == null && !method.IsStatic)
                throw new ArgumentNullException(nameof(instance));
            if (method.IsStatic && instance != null)
                throw new ArgumentException("instance");
            if (!method.IsStatic && !instance.Type.IsAssignableTo(method.DeclaringType))
                throw new ArgumentException("Type is not assignable to the declaring type of the method");

            var args = CheckMethodArguments(method, arguments);

            return new MethodCallExpression(instance, method, args);
        }

        private static Type[] CollectTypes(IEnumerable<Expression> expressions)
        {
            return (from arg in expressions select arg.Type).ToArray();
        }

        private static MethodInfo TryMakeGeneric(MethodInfo method, Type[] args)
        {
            if (method == null)
                return null;

            if (!method.IsGenericMethod && args == null)
                return method;

            return args.Length == method.GetGenericArguments().Length
                ? method.MakeGenericMethod(args)
                : null;
        }

        public static MethodCallExpression Call(Expression instance, string methodName, Type[] typeArguments, params Expression[] arguments)
        {
            if (instance == null)
                throw new ArgumentNullException(nameof(instance));
            if (methodName == null)
                throw new ArgumentNullException(nameof(methodName));

            MethodInfo method = TryGetMethod(instance.Type, methodName, AllInstance,
                CollectTypes(arguments), typeArguments);

            var args = CheckMethodArguments(method, arguments);

            return new MethodCallExpression(instance, method, args);
        }

        private static bool MethodMatch(MethodInfo method, string name, Type[] parameterTypes)
        {
            if (method.Name != name)
                return false;

            var parameters = method.GetParameters();

            if (parameters.Length != parameterTypes.Length)
                return false;

            if (method.IsGenericMethod) // if it's a generic method, when can't compare its parameters
                return true;

            return !parameters
                .Where((t, i) => !IsAssignableToParameterType(parameterTypes[i], t))
                .Any();
        }

        private static MethodInfo TryGetMethod(Type type, string methodName, BindingFlags flags, Type[] parameterTypes, Type[] argumentTypes)
        {
            var methods = from meth in type.GetMethods(flags)
                          where MethodMatch(meth, methodName, parameterTypes)
                          select meth;

            if (methods.Count() > 1)
                throw new InvalidOperationException("Too much method candidates");

            MethodInfo method = TryMakeGeneric(methods.FirstOrDefault(), argumentTypes);
            if (method != null)
                return method;

            throw new InvalidOperationException("No such method");
        }

        public static MethodCallExpression Call(Type type, string methodName, Type[] typeArguments, params Expression[] arguments)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));
            if (methodName == null)
                throw new ArgumentNullException(nameof(methodName));

            MethodInfo method = TryGetMethod(type, methodName, AllStatic,
                CollectTypes(arguments), typeArguments);

            var args = CheckMethodArguments(method, arguments);

            return new MethodCallExpression(method, args);
        }

        public static ConditionalExpression Condition(Expression test, Expression ifTrue, Expression ifFalse)
        {
            if (test == null)
                throw new ArgumentNullException(nameof(test));
            if (ifTrue == null)
                throw new ArgumentNullException(nameof(ifTrue));
            if (ifFalse == null)
                throw new ArgumentNullException(nameof(ifFalse));
            if (test.Type != typeof(bool))
                throw new ArgumentException("Test expression should be of type bool");
            if (ifTrue.Type != ifFalse.Type)
                throw new ArgumentException("The ifTrue and ifFalse type do not match");

            return new ConditionalExpression(test, ifTrue, ifFalse);
        }

        public static ConstantExpression Constant(object value)
        {
            return value == null
                ? new ConstantExpression(null, typeof(object))
                : Constant(value, value.GetType());
        }

        public static ConstantExpression Constant(object value, Type type)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            //
            // value must be compatible with type, no conversions
            // are allowed
            //
            if (value == null)
            {
                if (type.IsValueType && !type.IsNullable())
                    throw new ArgumentException();
            }
            else
            {
                if (!(type.IsValueType && type.IsNullable()) && !value.GetType().IsAssignableTo(type))
                    throw new ArgumentException();
            }

            return new ConstantExpression(value, type);
        }

        private static bool IsConvertiblePrimitive(Type type)
        {
            Type t = type.GetNotNullableType();
            return t != typeof(bool)
                   && (t.IsEnum || t.IsPrimitive);
        }

        internal static bool IsPrimitiveConversion(Type type, Type target)
        {
            if (type == target)
                return true;

            if (type.IsNullable() && target == type.GetNotNullableType())
                return true;

            if (target.IsNullable() && type == target.GetNotNullableType())
                return true;

            if (IsConvertiblePrimitive(type) && IsConvertiblePrimitive(target))
                return true;

            return false;
        }

        internal static bool IsReferenceConversion(Type type, Type target)
        {
            if (type == target)
                return true;

            if (type == typeof(object) || target == typeof(object))
                return true;

            if (type.IsInterface || target.IsInterface)
                return true;

            if (type.IsValueType || target.IsValueType)
                return false;

            if (type.IsAssignableTo(target) || target.IsAssignableTo(type))
                return true;

            return false;
        }

        public static UnaryExpression Convert(Expression expression, Type type)
        {
            return Convert(expression, type, null);
        }

        private static MethodInfo GetUserConversionMethod(Type type, Type target)
        {
            MethodInfo method = GetUnaryOperator("op_Explicit", type, type, target);
            if (method == null)
                method = GetUnaryOperator("op_Implicit", type, type, target);
            if (method == null)
                method = GetUnaryOperator("op_Explicit", target, type, target);
            if (method == null)
                method = GetUnaryOperator("op_Implicit", target, type, target);
            if (method == null)
                throw new InvalidOperationException();

            return method;
        }

        public static UnaryExpression Convert(Expression expression, Type type, MethodInfo method)
        {
            if (expression == null)
                throw new ArgumentNullException(nameof(expression));
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            Type et = expression.Type;

            if (method != null)
                CheckUnaryMethod(method, et);
            else if (!IsPrimitiveConversion(et, type) && !IsReferenceConversion(et, type))
                method = GetUserConversionMethod(et, type);

            return new UnaryExpression(
                ExpressionType.Convert, expression, type, method, IsConvertNodeLifted(method, expression, type));
        }

        private static bool IsConvertNodeLifted(MethodInfo method, Expression operand, Type target)
        {
            if (method == null)
                return operand.Type.IsNullable() || target.IsNullable();

            if (operand.Type.IsNullable() && !ParameterMatch(method, operand.Type))
                return true;

            if (target.IsNullable() && !ReturnTypeMatch(method, target))
                return true;

            return false;
        }

        private static bool ParameterMatch(MethodInfo method, Type type)
        {
            return method.GetParameters()[0].ParameterType == type;
        }

        private static bool ReturnTypeMatch(MethodInfo method, Type type)
        {
            return method.ReturnType == type;
        }

        public static UnaryExpression ConvertChecked(Expression expression, Type type)
        {
            return ConvertChecked(expression, type, null);
        }

        public static UnaryExpression ConvertChecked(Expression expression, Type type, MethodInfo method)
        {
            if (expression == null)
                throw new ArgumentNullException(nameof(expression));
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            Type et = expression.Type;

            if (method != null)
                CheckUnaryMethod(method, et);
            else if (IsReferenceConversion(et, type))
                return Convert(expression, type, method);
            else if (!IsPrimitiveConversion(et, type))
                method = GetUserConversionMethod(et, type);

            return new UnaryExpression(
                ExpressionType.ConvertChecked, expression, type, method, IsConvertNodeLifted(method, expression, type));
        }

        public static ElementInit ElementInit(MethodInfo addMethod, params Expression[] arguments)
        {
            return ElementInit(addMethod, arguments as IEnumerable<Expression>);
        }

        public static ElementInit ElementInit(MethodInfo addMethod, IEnumerable<Expression> arguments)
        {
            if (addMethod == null)
                throw new ArgumentNullException(nameof(addMethod));
            if (arguments == null)
                throw new ArgumentNullException(nameof(arguments));
            if (addMethod.Name.ToLower(CultureInfo.InvariantCulture) != "add")
                throw new ArgumentException("addMethod");
            if (addMethod.IsStatic)
                throw new ArgumentException("addMethod must be an instance method", nameof(addMethod));

            var args = CheckMethodArguments(addMethod, arguments);

            return new ElementInit(addMethod, args);
        }

        public static MemberExpression Field(Expression expression, FieldInfo field)
        {
            if (field == null)
                throw new ArgumentNullException(nameof(field));

            if (!field.IsStatic)
            {
                if (expression == null)
                    throw new ArgumentNullException(nameof(expression));
                if (!expression.Type.IsAssignableTo(field.DeclaringType))
                    throw new ArgumentException("field");
            }
            else if (expression != null)
            {
                throw new ArgumentException("expression");
            }

            return new MemberExpression(expression, field, field.FieldType);
        }

        public static MemberExpression Field(Expression expression, string fieldName)
        {
            if (expression == null)
                throw new ArgumentNullException(nameof(expression));

            FieldInfo field = expression.Type.GetField(fieldName, AllInstance);
            if (field == null)
                throw new ArgumentException($"No field named {fieldName} on {expression.Type}");

            return new MemberExpression(expression, field, field.FieldType);
        }

        public static Type GetActionType(params Type[] typeArgs)
        {
            if (typeArgs == null)
                throw new ArgumentNullException(nameof(typeArgs));

            if (typeArgs.Length > 4)
                throw new ArgumentException("No Action type of this arity");

            if (typeArgs.Length == 0)
                return typeof(Action);

            Type action = null;
            switch (typeArgs.Length)
            {
                case 1:
                    action = typeof(Action<>);
                    break;
                case 2:
                    action = typeof(Action<,>);
                    break;
                case 3:
                    action = typeof(Action<,,>);
                    break;
                case 4:
                    action = typeof(Action<,,,>);
                    break;
            }

            return action.MakeGenericType(typeArgs);
        }

        public static Type GetFuncType(params Type[] typeArgs)
        {
            if (typeArgs == null)
                throw new ArgumentNullException(nameof(typeArgs));

            if (typeArgs.Length < 1 || typeArgs.Length > 5)
                throw new ArgumentException("No Func type of this arity");

            Type func = null;
            switch (typeArgs.Length)
            {
                case 1:
                    func = typeof(Func<>);
                    break;
                case 2:
                    func = typeof(Func<,>);
                    break;
                case 3:
                    func = typeof(Func<,,>);
                    break;
                case 4:
                    func = typeof(Func<,,,>);
                    break;
                case 5:
                    func = typeof(Func<,,,,>);
                    break;
            }

            return func.MakeGenericType(typeArgs);
        }

        public static InvocationExpression Invoke(Expression expression, params Expression[] arguments)
        {
            return Invoke(expression, arguments as IEnumerable<Expression>);
        }

        private static Type GetInvokableType(Type t)
        {
            return t.IsAssignableTo(typeof(Delegate))
                ? t
                : GetGenericType(t, typeof(Expression<>));
        }

        private static Type GetGenericType(Type t, Type def)
        {
            while (true)
            {
                if (t == null)
                    return null;

                if (t.IsGenericType && t.GetGenericTypeDefinition() == def)
                    return t;

                t = t.BaseType;
            }
        }

        public static InvocationExpression Invoke(Expression expression, IEnumerable<Expression> arguments)
        {
            if (expression == null)
                throw new ArgumentNullException(nameof(expression));

            Type type = GetInvokableType(expression.Type);
            if (type == null)
                throw new ArgumentException("The type of the expression is not invokable");

            var args = arguments.ToReadOnlyCollection();
            CheckForNull(args, "arguments");

            MethodInfo invoke = type.GetInvokeMethod();
            if (invoke == null)
                throw new ArgumentException("expression");

            if (invoke.GetParameters().Length != args.Count)
                throw new InvalidOperationException("Arguments count doesn't match parameters length");

            args = CheckMethodArguments(invoke, args);

            return new InvocationExpression(expression, invoke.ReturnType, args);
        }

        private static bool CanAssign(Type target, Type source)
        {
            // This catches object and value type mixage, type compatibility is handled later
            if (target.IsValueType ^ source.IsValueType)
                return false;

            return source.IsAssignableTo(target);
        }

        private static Expression CheckLambda(Type delegateType, Expression body, ReadOnlyCollection<ParameterExpression> parameters)
        {
            if (!delegateType.IsSubclassOf(typeof(Delegate)))
                throw new ArgumentException("delegateType");

            MethodInfo invoke = delegateType.GetInvokeMethod();
            if (invoke == null)
                throw new ArgumentException("delegate must contain an Invoke method", nameof(delegateType));

            var invokeParameters = invoke.GetParameters();
            if (invokeParameters.Length != parameters.Count)
                throw new ArgumentException($"Different number of arguments in delegate {delegateType}", nameof(delegateType));

            for (int i = 0; i < invokeParameters.Length; i++)
            {
                if (!CanAssign(parameters[i].Type, invokeParameters[i].ParameterType))
                {
                    throw new ArgumentException(
                        $"Can not assign a {invokeParameters[i].ParameterType} to a {parameters[i].Type}");
                }
            }

            if (invoke.ReturnType == typeof(void))
                return body;

            if (CanAssign(invoke.ReturnType, body.Type))
                return body;

            if (invoke.ReturnType.IsExpression())
                return Quote(body);

            throw new ArgumentException($"body type {body.Type} can not be assigned to {invoke.ReturnType}");
        }

        public static Expression<TDelegate> Lambda<TDelegate>(Expression body, params ParameterExpression[] parameters)
        {
            return Lambda<TDelegate>(body, parameters as IEnumerable<ParameterExpression>);
        }

        public static Expression<TDelegate> Lambda<TDelegate>(Expression body, IEnumerable<ParameterExpression> parameters)
        {
            if (body == null)
                throw new ArgumentNullException(nameof(body));

            var ps = parameters.ToReadOnlyCollection();
            body = CheckLambda(typeof(TDelegate), body, ps);

            return new Expression<TDelegate>(body, ps);
        }

        public static LambdaExpression Lambda(Expression body, params ParameterExpression[] parameters)
        {
            if (body == null)
                throw new ArgumentNullException(nameof(body));
            if (parameters.Length > 4)
                throw new ArgumentException("Too many parameters");

            return Lambda(GetDelegateType(body.Type, parameters), body, parameters);
        }

        private static Type GetDelegateType(Type returnType, ParameterExpression[] parameters)
        {
            if (parameters == null)
                parameters = new ParameterExpression[0];

            if (returnType == typeof(void))
                return GetActionType(parameters.Select(p => p.Type).ToArray());

            var types = new Type[parameters.Length + 1];
            for (int i = 0; i < types.Length - 1; i++)
                types[i] = parameters[i].Type;

            types[types.Length - 1] = returnType;
            return GetFuncType(types);
        }

        public static LambdaExpression Lambda(Type delegateType, Expression body, params ParameterExpression[] parameters)
        {
            return Lambda(delegateType, body, parameters as IEnumerable<ParameterExpression>);
        }

        private static LambdaExpression CreateExpressionOf(Type type, Expression body, ReadOnlyCollection<ParameterExpression> parameters)
        {
            return (LambdaExpression)typeof(Expression<>).MakeGenericType(type).GetConstructor(
                NonPublicInstance, null, new[] { typeof(Expression), typeof(ReadOnlyCollection<ParameterExpression>) }, null).Invoke(new object[] { body, parameters });
        }

        public static LambdaExpression Lambda(Type delegateType, Expression body, IEnumerable<ParameterExpression> parameters)
        {
            if (delegateType == null)
                throw new ArgumentNullException(nameof(delegateType));
            if (body == null)
                throw new ArgumentNullException(nameof(body));

            var ps = parameters.ToReadOnlyCollection();

            body = CheckLambda(delegateType, body, ps);

            return CreateExpressionOf(delegateType, body, ps);
        }

        public static MemberListBinding ListBind(MemberInfo member, params ElementInit[] initializers)
        {
            return ListBind(member, initializers as IEnumerable<ElementInit>);
        }

        private static void CheckIsAssignableToIEnumerable(Type t)
        {
            if (!t.IsAssignableTo(typeof(IEnumerable)))
                throw new ArgumentException($"Type {t} doesn't implemen IEnumerable");
        }

        public static MemberListBinding ListBind(MemberInfo member, IEnumerable<ElementInit> initializers)
        {
            if (member == null)
                throw new ArgumentNullException(nameof(member));
            if (initializers == null)
                throw new ArgumentNullException(nameof(initializers));

            var inits = initializers.ToReadOnlyCollection();
            CheckForNull(inits, "initializers");

            member.OnFieldOrProperty(
                field => CheckIsAssignableToIEnumerable(field.FieldType),
                prop => CheckIsAssignableToIEnumerable(prop.PropertyType));

            return new MemberListBinding(member, inits);
        }

        public static MemberListBinding ListBind(MethodInfo propertyAccessor, params ElementInit[] initializers)
        {
            return ListBind(propertyAccessor, initializers as IEnumerable<ElementInit>);
        }

        private static void CheckForNull<T>(ReadOnlyCollection<T> collection, string name) where T : class
        {
            foreach (T t in collection)
            {
                if (t == null)
                    throw new ArgumentNullException(name);
            }
        }

        public static MemberListBinding ListBind(MethodInfo propertyAccessor, IEnumerable<ElementInit> initializers)
        {
            if (propertyAccessor == null)
                throw new ArgumentNullException(nameof(propertyAccessor));
            if (initializers == null)
                throw new ArgumentNullException(nameof(initializers));

            var inits = initializers.ToReadOnlyCollection();
            CheckForNull(inits, "initializers");

            PropertyInfo prop = GetAssociatedProperty(propertyAccessor);
            if (prop == null)
                throw new ArgumentException("propertyAccessor");

            CheckIsAssignableToIEnumerable(prop.PropertyType);

            return new MemberListBinding(prop, inits);
        }

        public static ListInitExpression ListInit(NewExpression newExpression, params ElementInit[] initializers)
        {
            return ListInit(newExpression, initializers as IEnumerable<ElementInit>);
        }

        public static ListInitExpression ListInit(NewExpression newExpression, IEnumerable<ElementInit> initializers)
        {
            var inits = CheckListInit(newExpression, initializers);
            return new ListInitExpression(newExpression, inits);
        }

        public static ListInitExpression ListInit(NewExpression newExpression, params Expression[] initializers)
        {
            return ListInit(newExpression, initializers as IEnumerable<Expression>);
        }

        public static ListInitExpression ListInit(NewExpression newExpression, IEnumerable<Expression> initializers)
        {
            var inits = CheckListInit(newExpression, initializers);

            MethodInfo addMethod = GetAddMethod(newExpression.Type, inits[0].Type);
            if (addMethod == null)
                throw new InvalidOperationException("No suitable add method found");

            return new ListInitExpression(newExpression, CreateInitializers(addMethod, inits));
        }

        private static ReadOnlyCollection<ElementInit> CreateInitializers(MethodInfo addMethod, ReadOnlyCollection<Expression> initializers)
        {
            return (from init in initializers select ElementInit(addMethod, init)).ToReadOnlyCollection();
        }

        private static MethodInfo GetAddMethod(Type type, Type arg)
        {
            return type.GetMethod("Add", PublicInstance | BindingFlags.IgnoreCase, null, new[] { arg }, null);
        }

        public static ListInitExpression ListInit(NewExpression newExpression, MethodInfo addMethod, params Expression[] initializers)
        {
            return ListInit(newExpression, addMethod, initializers as IEnumerable<Expression>);
        }

        private static ReadOnlyCollection<T> CheckListInit<T>(NewExpression newExpression, IEnumerable<T> initializers) where T : class
        {
            if (newExpression == null)
                throw new ArgumentNullException(nameof(newExpression));
            if (initializers == null)
                throw new ArgumentNullException(nameof(initializers));
            if (!newExpression.Type.IsAssignableTo(typeof(IEnumerable)))
                throw new InvalidOperationException("The type of the new expression does not implement IEnumerable");

            var inits = initializers.ToReadOnlyCollection();
            if (inits.Count == 0)
                throw new ArgumentException("Empty initializers");

            CheckForNull(inits, "initializers");

            return inits;
        }

        public static ListInitExpression ListInit(NewExpression newExpression, MethodInfo addMethod, IEnumerable<Expression> initializers)
        {
            var inits = CheckListInit(newExpression, initializers);

            if (addMethod != null)
            {
                if (addMethod.Name.ToLower(CultureInfo.InvariantCulture) != "add")
                    throw new ArgumentException("addMethod");

                var parameters = addMethod.GetParameters();
                if (parameters.Length != 1)
                    throw new ArgumentException("addMethod");

                foreach (Expression expression in inits)
                {
                    if (!IsAssignableToParameterType(expression.Type, parameters[0]))
                        throw new InvalidOperationException("Initializer not assignable to the add method parameter type");
                }
            }

            if (addMethod == null)
                addMethod = GetAddMethod(newExpression.Type, inits[0].Type);

            if (addMethod == null)
                throw new InvalidOperationException("No suitable add method found");

            return new ListInitExpression(newExpression, CreateInitializers(addMethod, inits));
        }

        public static MemberExpression MakeMemberAccess(Expression expression, MemberInfo member)
        {
            if (expression == null)
                throw new ArgumentNullException(nameof(expression));
            if (member == null)
                throw new ArgumentNullException(nameof(member));

            if (member is FieldInfo field)
                return Field(expression, field);

            if (member is PropertyInfo property)
                return Property(expression, property);

            throw new ArgumentException("Member should either be a field or a property");
        }

        public static UnaryExpression MakeUnary(ExpressionType unaryType, Expression operand, Type type)
        {
            return MakeUnary(unaryType, operand, type, null);
        }

        public static UnaryExpression MakeUnary(ExpressionType unaryType, Expression operand, Type type, MethodInfo method)
        {
            switch (unaryType)
            {
                case ExpressionType.ArrayLength:
                    return ArrayLength(operand);
                case ExpressionType.Convert:
                    return Convert(operand, type, method);
                case ExpressionType.ConvertChecked:
                    return ConvertChecked(operand, type, method);
                case ExpressionType.Negate:
                    return Negate(operand, method);
                case ExpressionType.NegateChecked:
                    return NegateChecked(operand, method);
                case ExpressionType.Not:
                    return Not(operand, method);
                case ExpressionType.Quote:
                    return Quote(operand);
                case ExpressionType.TypeAs:
                    return TypeAs(operand, type);
                case ExpressionType.UnaryPlus:
                    return UnaryPlus(operand, method);
            }

            throw new ArgumentException("MakeUnary expect an unary operator");
        }

        public static MemberMemberBinding MemberBind(MemberInfo member, params MemberBinding[] bindings)
        {
            return MemberBind(member, bindings as IEnumerable<MemberBinding>);
        }

        public static MemberMemberBinding MemberBind(MemberInfo member, IEnumerable<MemberBinding> bindings)
        {
            if (member == null)
                throw new ArgumentNullException(nameof(member));

            Type type = member.OnFieldOrProperty(
                field => field.FieldType,
                prop => prop.PropertyType);

            return new MemberMemberBinding(member, CheckMemberBindings(type, bindings));
        }

        public static MemberMemberBinding MemberBind(MethodInfo propertyAccessor, params MemberBinding[] bindings)
        {
            return MemberBind(propertyAccessor, bindings as IEnumerable<MemberBinding>);
        }

        public static MemberMemberBinding MemberBind(MethodInfo propertyAccessor, IEnumerable<MemberBinding> bindings)
        {
            if (propertyAccessor == null)
                throw new ArgumentNullException(nameof(propertyAccessor));

            var bds = bindings.ToReadOnlyCollection();
            CheckForNull(bds, "bindings");

            PropertyInfo prop = GetAssociatedProperty(propertyAccessor);
            if (prop == null)
                throw new ArgumentException("propertyAccessor");

            return new MemberMemberBinding(prop, CheckMemberBindings(prop.PropertyType, bindings));
        }

        private static ReadOnlyCollection<MemberBinding> CheckMemberBindings(Type type, IEnumerable<MemberBinding> bindings)
        {
            if (bindings == null)
                throw new ArgumentNullException(nameof(bindings));

            var bds = bindings.ToReadOnlyCollection();
            CheckForNull(bds, "bindings");

            foreach (MemberBinding binding in bds)
            {
                if (!type.IsAssignableTo(binding.Member.DeclaringType))
                    throw new ArgumentException("Type not assignable to member type");
            }

            return bds;
        }

        public static MemberInitExpression MemberInit(NewExpression newExpression, params MemberBinding[] bindings)
        {
            return MemberInit(newExpression, bindings as IEnumerable<MemberBinding>);
        }

        public static MemberInitExpression MemberInit(NewExpression newExpression, IEnumerable<MemberBinding> bindings)
        {
            if (newExpression == null)
                throw new ArgumentNullException(nameof(newExpression));

            return new MemberInitExpression(newExpression, CheckMemberBindings(newExpression.Type, bindings));
        }

        public static UnaryExpression Negate(Expression expression)
        {
            return Negate(expression, null);
        }

        public static UnaryExpression Negate(Expression expression, MethodInfo method)
        {
            method = UnaryCoreCheck("op_UnaryNegation", expression, method, type => IsSignedNumber(type));
            return MakeSimpleUnary(ExpressionType.Negate, expression, method);
        }

        public static UnaryExpression NegateChecked(Expression expression)
        {
            return NegateChecked(expression, null);
        }

        public static UnaryExpression NegateChecked(Expression expression, MethodInfo method)
        {
            method = UnaryCoreCheck("op_UnaryNegation", expression, method, type => IsSignedNumber(type));
            return MakeSimpleUnary(ExpressionType.NegateChecked, expression, method);
        }

        public static NewExpression New(ConstructorInfo constructor)
        {
            if (constructor == null)
                throw new ArgumentNullException(nameof(constructor));

            if (constructor.GetParameters().Length > 0)
                throw new ArgumentException("Constructor must be parameter less");

            return new NewExpression(constructor, (null as IEnumerable<Expression>).ToReadOnlyCollection(), null);
        }

        public static NewExpression New(Type type)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            CheckNotVoid(type);

            var args = (null as IEnumerable<Expression>).ToReadOnlyCollection();

            if (type.IsValueType)
                return new NewExpression(type, args);

            ConstructorInfo ctor = type.GetConstructor(new Type[0]);
            if (ctor == null)
                throw new ArgumentException("Type doesn't have a parameter less constructor");

            return new NewExpression(ctor, args, null);
        }

        public static NewExpression New(ConstructorInfo constructor, params Expression[] arguments)
        {
            return New(constructor, arguments as IEnumerable<Expression>);
        }

        public static NewExpression New(ConstructorInfo constructor, IEnumerable<Expression> arguments)
        {
            if (constructor == null)
                throw new ArgumentNullException(nameof(constructor));

            var args = CheckMethodArguments(constructor, arguments);
            return new NewExpression(constructor, args, null);
        }

        private static IList<Expression> CreateArgumentList(IEnumerable<Expression> arguments)
        {
            if (arguments == null)
                return arguments.ToReadOnlyCollection();

            return arguments.ToList();
        }

        private static ReadOnlyCollection<Expression> CheckMethodArguments(MethodBase method, IEnumerable<Expression> args)
        {
            var arguments = CreateArgumentList(args);
            var parameters = method.GetParameters();

            if (arguments.Count != parameters.Length)
                throw new ArgumentException("The number of arguments doesn't match the number of parameters");

            for (int i = 0; i < parameters.Length; i++)
            {
                if (arguments[i] == null)
                    throw new ArgumentNullException("arguments");

                if (IsAssignableToParameterType(arguments[i].Type, parameters[i]))
                    continue;

                if (!parameters[i].ParameterType.IsExpression())
                    throw new ArgumentException("arguments");

                arguments[i] = Quote(arguments[i]);
            }

            return arguments.ToReadOnlyCollection();
        }

        public static NewExpression New(ConstructorInfo constructor, IEnumerable<Expression> arguments, params MemberInfo[] members)
        {
            return New(constructor, arguments, members as IEnumerable<MemberInfo>);
        }

        public static NewExpression New(ConstructorInfo constructor, IEnumerable<Expression> arguments, IEnumerable<MemberInfo> members)
        {
            if (constructor == null)
                throw new ArgumentNullException(nameof(constructor));

            var args = arguments.ToReadOnlyCollection();
            var mmbs = members.ToReadOnlyCollection();

            CheckForNull(args, "arguments");
            CheckForNull(mmbs, "members");

            args = CheckMethodArguments(constructor, arguments);

            if (args.Count != mmbs.Count)
                throw new ArgumentException("Arguments count does not match members count");

            for (int i = 0; i < mmbs.Count; i++)
            {
                MemberInfo member = mmbs[i];
                Type type;
                switch (member.MemberType)
                {
                    case MemberTypes.Field:
                        type = (member as FieldInfo).FieldType;
                        break;
                    case MemberTypes.Method:
                        type = (member as MethodInfo).ReturnType;
                        break;
                    case MemberTypes.Property:
                        var prop = member as PropertyInfo;
                        if (prop.GetGetMethod(true) == null)
                            throw new ArgumentException("Property must have a getter");

                        type = (member as PropertyInfo).PropertyType;
                        break;
                    default:
                        throw new ArgumentException("Member type not allowed");
                }

                if (!args[i].Type.IsAssignableTo(type))
                    throw new ArgumentException("Argument type not assignable to member type");
            }

            return new NewExpression(constructor, args, mmbs);
        }

        public static NewArrayExpression NewArrayBounds(Type type, params Expression[] bounds)
        {
            return NewArrayBounds(type, bounds as IEnumerable<Expression>);
        }

        public static NewArrayExpression NewArrayBounds(Type type, IEnumerable<Expression> bounds)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));
            if (bounds == null)
                throw new ArgumentNullException(nameof(bounds));

            CheckNotVoid(type);

            var arrayBounds = bounds.ToReadOnlyCollection();
            foreach (Expression expression in arrayBounds)
                if (!IsInt(expression.Type))
                    throw new ArgumentException("The bounds collection can only contain expression of integers types");

            return new NewArrayExpression(ExpressionType.NewArrayBounds, type.MakeArrayType(arrayBounds.Count), arrayBounds);
        }

        public static NewArrayExpression NewArrayInit(Type type, params Expression[] initializers)
        {
            return NewArrayInit(type, initializers as IEnumerable<Expression>);
        }

        public static NewArrayExpression NewArrayInit(Type type, IEnumerable<Expression> initializers)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));
            if (initializers == null)
                throw new ArgumentNullException(nameof(initializers));

            CheckNotVoid(type);

            var inits = initializers.ToReadOnlyCollection();

            foreach (Expression expression in inits)
            {
                if (expression == null)
                    throw new ArgumentNullException(nameof(initializers));

                if (!expression.Type.IsAssignableTo(type))
                    throw new InvalidOperationException(
                        $"{expression.Type} IsAssignableTo {type}, expression [ {expression.NodeType} ] : {expression}");

                // TODO: Quote elements if type == typeof (Expression)
            }

            return new NewArrayExpression(ExpressionType.NewArrayInit, type.MakeArrayType(), inits);
        }

        public static UnaryExpression Not(Expression expression)
        {
            return Not(expression, null);
        }

        public static UnaryExpression Not(Expression expression, MethodInfo method)
        {
            Func<Type, bool> validator = IsIntOrBool;

            method = UnaryCoreCheck("op_LogicalNot", expression, method, validator)
                     ?? UnaryCoreCheck("op_OnesComplement", expression, method, validator);

            return MakeSimpleUnary(ExpressionType.Not, expression, method);
        }

        private static void CheckNotVoid(Type type)
        {
            if (type == typeof(void))
                throw new ArgumentException("Type can't be void");
        }

        public static ParameterExpression Parameter(Type type, string name)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            CheckNotVoid(type);

            return new ParameterExpression(type, name);
        }

        public static MemberExpression Property(Expression expression, MethodInfo propertyAccessor)
        {
            if (propertyAccessor == null)
                throw new ArgumentNullException(nameof(propertyAccessor));

            if (!propertyAccessor.IsStatic)
            {
                if (expression == null)
                    throw new ArgumentNullException(nameof(expression));
                if (!expression.Type.IsAssignableTo(propertyAccessor.DeclaringType))
                    throw new ArgumentException("expression");
            }
            else if (expression != null)
            {
                throw new ArgumentException("expression");
            }

            PropertyInfo prop = GetAssociatedProperty(propertyAccessor);
            if (prop == null)
                throw new ArgumentException($"Method {propertyAccessor} has no associated property");

            return new MemberExpression(expression, prop, prop.PropertyType);
        }

        private static PropertyInfo GetAssociatedProperty(MethodInfo method)
        {
            if (method == null)
                return null;

            foreach (PropertyInfo prop in method.DeclaringType.GetProperties(All))
            {
                if (method.Equals(prop.GetGetMethod(true)))
                    return prop;

                if (method.Equals(prop.GetSetMethod(true)))
                    return prop;
            }

            return null;
        }

        public static MemberExpression Property(Expression expression, PropertyInfo property)
        {
            if (property == null)
                throw new ArgumentNullException(nameof(property));

            MethodInfo getter = property.GetGetMethod(true);
            if (getter == null)
                throw new ArgumentException("getter");

            if (!getter.IsStatic)
            {
                if (expression == null)
                    throw new ArgumentNullException(nameof(expression));
                if (!expression.Type.IsAssignableTo(property.DeclaringType))
                    throw new ArgumentException("expression");
            }
            else if (expression != null)
            {
                throw new ArgumentException("expression");
            }

            return new MemberExpression(expression, property, property.PropertyType);
        }

        public static MemberExpression Property(Expression expression, string propertyName)
        {
            if (expression == null)
                throw new ArgumentNullException(nameof(expression));

            PropertyInfo prop = expression.Type.GetProperty(propertyName, AllInstance);
            if (prop == null)
                throw new ArgumentException($"No property named {propertyName} on {expression.Type}");

            return new MemberExpression(expression, prop, prop.PropertyType);
        }

        public static MemberExpression PropertyOrField(Expression expression, string propertyOrFieldName)
        {
            if (expression == null)
                throw new ArgumentNullException(nameof(expression));
            if (propertyOrFieldName == null)
                throw new ArgumentNullException(nameof(propertyOrFieldName));

            PropertyInfo prop = expression.Type.GetProperty(propertyOrFieldName, AllInstance);
            if (prop != null)
                return new MemberExpression(expression, prop, prop.PropertyType);

            FieldInfo field = expression.Type.GetField(propertyOrFieldName, AllInstance);
            if (field != null)
                return new MemberExpression(expression, field, field.FieldType);

            throw new ArgumentException($"No field or property named {propertyOrFieldName} on {expression.Type}");
        }

        public static UnaryExpression Quote(Expression expression)
        {
            if (expression == null)
                throw new ArgumentNullException(nameof(expression));

            return new UnaryExpression(ExpressionType.Quote, expression, expression.GetType());
        }

        public static UnaryExpression TypeAs(Expression expression, Type type)
        {
            if (expression == null)
                throw new ArgumentNullException(nameof(expression));
            if (type == null)
                throw new ArgumentNullException(nameof(type));
            if (type.IsValueType && !type.IsNullable())
                throw new ArgumentException("TypeAs expect a reference or a nullable type");

            return new UnaryExpression(ExpressionType.TypeAs, expression, type);
        }

        public static TypeBinaryExpression TypeIs(Expression expression, Type type)
        {
            if (expression == null)
                throw new ArgumentNullException(nameof(expression));
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            CheckNotVoid(type);

            return new TypeBinaryExpression(ExpressionType.TypeIs, expression, type, typeof(bool));
        }

        public static UnaryExpression UnaryPlus(Expression expression)
        {
            return UnaryPlus(expression, null);
        }

        public static UnaryExpression UnaryPlus(Expression expression, MethodInfo method)
        {
            method = UnaryCoreCheck("op_UnaryPlus", expression, method, type => IsNumber(type));

            return MakeSimpleUnary(ExpressionType.UnaryPlus, expression, method);
        }

        private static bool IsInt(Type t)
        {
            return t == typeof(byte) || t == typeof(sbyte) ||
                t == typeof(short) || t == typeof(ushort) ||
                t == typeof(int) || t == typeof(uint) ||
                t == typeof(long) || t == typeof(ulong);
        }

        private static bool IsIntOrBool(Type t)
        {
            return IsInt(t) || t == typeof(bool);
        }

        private static bool IsNumber(Type t)
        {
            return IsInt(t) || t == typeof(float) || t == typeof(double) || t == typeof(decimal);
        }

        private static bool IsSignedNumber(Type t)
        {
            return IsNumber(t) && !IsUnsigned(t);
        }

        internal static bool IsUnsigned(Type t)
        {
            while (true)
            {
                if (!t.IsPointer)
                {
                    return t == typeof(ushort)
                           || t == typeof(uint)
                           || t == typeof(ulong)
                           || t == typeof(byte);
                }

                t = t.GetElementType();
            }
        }
    }
}
