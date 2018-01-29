//
// ExpressionTransformer.cs
//
// Authors:
//	Roei Erez (roeie@mainsoft.com)
//
// Copyright (C) 2007 Novell, Inc (http://www.novell.com)
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

using System.Collections.ObjectModel;

namespace System.Linq.Expressions
{
    internal abstract class ExpressionTransformer
    {
        public Expression Transform(Expression e)
        {
            return Visit(e);
        }

        protected virtual Expression Visit(Expression expression)
        {
            if (expression == null)
                return null;

            switch (expression.NodeType)
            {
                case ExpressionType.Negate:
                case ExpressionType.NegateChecked:
                case ExpressionType.Not:
                case ExpressionType.Convert:
                case ExpressionType.ConvertChecked:
                case ExpressionType.ArrayLength:
                case ExpressionType.Quote:
                case ExpressionType.TypeAs:
                case ExpressionType.UnaryPlus:
                    return VisitUnary((UnaryExpression)expression);
                case ExpressionType.Add:
                case ExpressionType.AddChecked:
                case ExpressionType.Subtract:
                case ExpressionType.SubtractChecked:
                case ExpressionType.Multiply:
                case ExpressionType.MultiplyChecked:
                case ExpressionType.Divide:
                case ExpressionType.Modulo:
                case ExpressionType.Power:
                case ExpressionType.And:
                case ExpressionType.AndAlso:
                case ExpressionType.Or:
                case ExpressionType.OrElse:
                case ExpressionType.LessThan:
                case ExpressionType.LessThanOrEqual:
                case ExpressionType.GreaterThan:
                case ExpressionType.GreaterThanOrEqual:
                case ExpressionType.Equal:
                case ExpressionType.NotEqual:
                case ExpressionType.Coalesce:
                case ExpressionType.ArrayIndex:
                case ExpressionType.RightShift:
                case ExpressionType.LeftShift:
                case ExpressionType.ExclusiveOr:
                    return VisitBinary((BinaryExpression)expression);
                case ExpressionType.TypeIs:
                    return VisitTypeIs((TypeBinaryExpression)expression);
                case ExpressionType.Conditional:
                    return VisitConditional((ConditionalExpression)expression);
                case ExpressionType.Constant:
                    return VisitConstant((ConstantExpression)expression);
                case ExpressionType.Parameter:
                    return VisitParameter((ParameterExpression)expression);
                case ExpressionType.MemberAccess:
                    return VisitMemberAccess((MemberExpression)expression);
                case ExpressionType.Call:
                    return VisitMethodCall((MethodCallExpression)expression);
                case ExpressionType.Lambda:
                    return VisitLambda((LambdaExpression)expression);
                case ExpressionType.New:
                    return VisitNew((NewExpression)expression);
                case ExpressionType.NewArrayInit:
                case ExpressionType.NewArrayBounds:
                    return VisitNewArray((NewArrayExpression)expression);
                case ExpressionType.Invoke:
                    return VisitInvocation((InvocationExpression)expression);
                case ExpressionType.MemberInit:
                    return VisitMemberInit((MemberInitExpression)expression);
                case ExpressionType.ListInit:
                    return VisitListInit((ListInitExpression)expression);
                default:
                    throw new ArgumentException(string.Format("Unhandled expression type: '{0}'", expression.NodeType));
            }
        }

        protected virtual MemberBinding VisitBinding(MemberBinding binding)
        {
            switch (binding.BindingType)
            {
                case MemberBindingType.Assignment:
                    return VisitMemberAssignment((MemberAssignment)binding);
                case MemberBindingType.MemberBinding:
                    return VisitMemberMemberBinding((MemberMemberBinding)binding);
                case MemberBindingType.ListBinding:
                    return VisitMemberListBinding((MemberListBinding)binding);
                default:
                    throw new ArgumentException(string.Format("Unhandled binding type '{0}'", binding.BindingType));
            }
        }

        protected virtual ElementInit VisitElementInitializer(ElementInit initializer)
        {
            var transformed = VisitExpressionList(initializer.Arguments);
            return transformed != initializer.Arguments
                ? Expression.ElementInit(initializer.AddMethod, transformed)
                : initializer;
        }

        protected virtual UnaryExpression VisitUnary(UnaryExpression unary)
        {
            Expression transformedOperand = Visit(unary.Operand);
            return transformedOperand != unary.Operand
                ? Expression.MakeUnary(unary.NodeType, transformedOperand, unary.Type, unary.Method)
                : unary;
        }

        protected virtual BinaryExpression VisitBinary(BinaryExpression binary)
        {
            Expression left = Visit(binary.Left);
            Expression right = Visit(binary.Right);
            LambdaExpression conversion = VisitLambda(binary.Conversion);

            return left != binary.Left || right != binary.Right || conversion != binary.Conversion
                ? Expression.MakeBinary(binary.NodeType, left, right, binary.IsLiftedToNull, binary.Method, conversion)
                : binary;
        }

        protected virtual TypeBinaryExpression VisitTypeIs(TypeBinaryExpression type)
        {
            Expression inner = Visit(type.Expression);
            return inner != type.Expression
                ? Expression.TypeIs(inner, type.TypeOperand)
                : type;
        }

        protected virtual ConstantExpression VisitConstant(ConstantExpression constant)
        {
            return constant;
        }

        protected virtual ConditionalExpression VisitConditional(ConditionalExpression conditional)
        {
            Expression test = Visit(conditional.Test);
            Expression ifTrue = Visit(conditional.IfTrue);
            Expression ifFalse = Visit(conditional.IfFalse);

            return test != conditional.Test || ifTrue != conditional.IfTrue || ifFalse != conditional.IfFalse
                ? Expression.Condition(test, ifTrue, ifFalse)
                : conditional;
        }

        protected virtual ParameterExpression VisitParameter(ParameterExpression parameter)
        {
            return parameter;
        }

        protected virtual MemberExpression VisitMemberAccess(MemberExpression member)
        {
            Expression memberExp = Visit(member.Expression);
            return memberExp != member.Expression
                ? Expression.MakeMemberAccess(memberExp, member.Member)
                : member;
        }

        protected virtual MethodCallExpression VisitMethodCall(MethodCallExpression methodCall)
        {
            Expression instance = Visit(methodCall.Object);
            var args = VisitExpressionList(methodCall.Arguments);

            return instance != methodCall.Object || args != methodCall.Arguments
                ? Expression.Call(instance, methodCall.Method, args)
                : methodCall;
        }

        protected virtual ReadOnlyCollection<Expression> VisitExpressionList(ReadOnlyCollection<Expression> list)
        {
            return VisitList(list, Visit);
        }

        private static ReadOnlyCollection<T> VisitList<T>(ReadOnlyCollection<T> list, Func<T, T> selector)
            where T : class
        {
            int index = 0;
            T[] arr = null;
            foreach (T e in list)
            {
                T visited = selector(e);
                if (visited != e || arr != null)
                {
                    if (arr == null)
                        arr = new T[list.Count];
                    arr[index] = visited;
                }
                index++;
            }

            return arr != null
                ? arr.ToReadOnlyCollection()
                : list;
        }

        protected virtual MemberAssignment VisitMemberAssignment(MemberAssignment assignment)
        {
            Expression inner = Visit(assignment.Expression);
            return inner != assignment.Expression
                ? Expression.Bind(assignment.Member, inner)
                : assignment;
        }

        protected virtual MemberMemberBinding VisitMemberMemberBinding(MemberMemberBinding binding)
        {
            var bindingExp = VisitBindingList(binding.Bindings);
            return bindingExp != binding.Bindings
                ? Expression.MemberBind(binding.Member, bindingExp)
                : binding;
        }

        protected virtual MemberListBinding VisitMemberListBinding(MemberListBinding binding)
        {
            var initializers = VisitElementInitializerList(binding.Initializers);
            return initializers != binding.Initializers
                ? Expression.ListBind(binding.Member, initializers)
                : binding;
        }

        protected virtual ReadOnlyCollection<MemberBinding> VisitBindingList(ReadOnlyCollection<MemberBinding> list)
        {
            return VisitList(list, VisitBinding);
        }

        protected virtual ReadOnlyCollection<ElementInit> VisitElementInitializerList(ReadOnlyCollection<ElementInit> list)
        {
            return VisitList(list, VisitElementInitializer);
        }

        protected virtual LambdaExpression VisitLambda(LambdaExpression lambda)
        {
            Expression body = Visit(lambda.Body);
            var parameters = VisitList(lambda.Parameters, VisitParameter);

            return body != lambda.Body || parameters != lambda.Parameters
                ? Expression.Lambda(body, parameters.ToArray())
                : lambda;
        }

        protected virtual NewExpression VisitNew(NewExpression nex)
        {
            var args = VisitList(nex.Arguments, Visit);
            return args != nex.Arguments
                ? Expression.New(nex.Constructor, args)
                : nex;
        }

        protected virtual MemberInitExpression VisitMemberInit(MemberInitExpression init)
        {
            NewExpression newExp = VisitNew(init.NewExpression);
            var bindings = VisitBindingList(init.Bindings);

            return newExp != init.NewExpression || bindings != init.Bindings
                ? Expression.MemberInit(newExp, bindings)
                : init;
        }

        protected virtual ListInitExpression VisitListInit(ListInitExpression init)
        {
            NewExpression newExp = VisitNew(init.NewExpression);
            var initializers = VisitElementInitializerList(init.Initializers);

            return newExp != init.NewExpression || initializers != init.Initializers
                ? Expression.ListInit(newExp, initializers.ToArray())
                : init;
        }

        protected virtual NewArrayExpression VisitNewArray(NewArrayExpression newArray)
        {
            var expressions = VisitExpressionList(newArray.Expressions);
            if (expressions == newArray.Expressions)
                return newArray;

            return newArray.NodeType == ExpressionType.NewArrayBounds
                ? Expression.NewArrayBounds(newArray.Type, expressions)
                : Expression.NewArrayInit(newArray.Type, expressions);
        }

        protected virtual InvocationExpression VisitInvocation(InvocationExpression invocation)
        {
            var args = VisitExpressionList(invocation.Arguments);
            Expression invocationExp = Visit(invocation.Expression);

            return args != invocation.Arguments || invocationExp != invocation.Expression
                ? Expression.Invoke(invocationExp, args)
                : invocation;
        }
    }
}
