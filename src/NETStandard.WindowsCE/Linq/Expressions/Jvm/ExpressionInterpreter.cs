using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;

namespace System.Linq.Expressions.Jvm
{
    internal class ExpressionInterpreter : ExpressionVisitor
    {
        private readonly LambdaExpression lambda;
        private readonly object[] arguments;
        private readonly Stack<object> stack = new Stack<object>();

        private void Push(object value)
        {
            stack.Push(value);
        }

        private object Pop()
        {
            return stack.Pop();
        }

        public ExpressionInterpreter(LambdaExpression lambda, object[] arguments)
        {
            this.lambda = lambda;
            this.arguments = arguments;
        }

        private void VisitCoalesce(BinaryExpression binary)
        {
            Visit(binary.Left);

            var value = Pop();
            if (value == null)
            {
                Visit(binary.Right);
            }
            else
                Push(value);
        }

        private void VisitAndAlso(BinaryExpression binary)
        {
            object left = null;

            Visit(binary.Left);

            object right = Pop();

            if (right == null || ((bool)right))
            {
                Visit(binary.Right);
                left = Pop();
            }

            Push(Math.And(right, left));
        }

        private void VisitOrElse(BinaryExpression binary)
        {
            object left = null;

            Visit(binary.Left);
            object right = Pop();

            if (right == null || !((bool)right))
            {
                Visit(binary.Right);
                left = Pop();
            }

            Push(Math.Or(right, left));
        }

        private void VisitCommonBinary(BinaryExpression binary)
        {
            try
            {
                Visit(binary.Left);
                object left = Pop();
                Visit(binary.Right);
                object right = Pop();

                if (binary.Method != null)
                {
                    Push(binary.Method.Invoke(null, new[] { left, right }));
                    return;
                }

                switch (binary.NodeType)
                {
                    case ExpressionType.ArrayIndex:
                        Push(((Array)left).GetValue((int)right));
                        return;
                    case ExpressionType.Equal:
                        if (typeof(ValueType).IsAssignableFrom(binary.Right.Type))
                            Push(Equals(left, right));
                        else
                            Push(left == right);
                        return;
                    case ExpressionType.NotEqual:
                        Push(left != right);
                        return;
                    case ExpressionType.LessThan:
                        Push(Comparer.Default.Compare(left, right) < 0);
                        return;
                    case ExpressionType.LessThanOrEqual:
                        Push(Comparer.Default.Compare(left, right) <= 0);
                        return;
                    case ExpressionType.GreaterThan:
                        Push(Comparer.Default.Compare(left, right) > 0);
                        return;
                    case ExpressionType.GreaterThanOrEqual:
                        Push(Comparer.Default.Compare(left, right) >= 0);
                        return;
                    case ExpressionType.RightShift:
                        Push(Math.RightShift(left, Convert.ToInt32(right), Type.GetTypeCode(binary.Type)));
                        return;
                    case ExpressionType.LeftShift:
                        Push(Math.LeftShift(left, Convert.ToInt32(right), Type.GetTypeCode(binary.Type)));
                        return;
                    default:
                        Push(Math.Evaluate(left, right, binary.Type, binary.NodeType));
                        break;

                }
            }
            catch (OverflowException)
            {
                throw;
            }
            catch (Exception e)
            {

                throw new NotImplementedException(
                    $"Interpriter for BinaryExpression with NodeType {binary.NodeType} is not implimented", e);
            }
        }

        protected override void VisitBinary(BinaryExpression binary)
        {
            switch (binary.NodeType)
            {
                case ExpressionType.AndAlso:
                    VisitAndAlso(binary);
                    return;
                case ExpressionType.OrElse:
                    VisitOrElse(binary);
                    return;
                case ExpressionType.Coalesce:
                    VisitCoalesce(binary);
                    return;
                default:
                    VisitCommonBinary(binary);
                    break;
            }
        }

        protected override void VisitUnary(UnaryExpression unary)
        {
            if (unary.NodeType == ExpressionType.Quote)
            {
                Push(unary.Operand);
                return;
            }

            Visit(unary.Operand);
            object o = Pop();

            if (unary.Method != null)
            {
                Push(unary.Method.Invoke(null, new[] { o }));
                return;
            }

            switch (unary.NodeType)
            {
                case ExpressionType.TypeAs:
                    if (o == null || !Math.IsType(unary.Type, o))
                    {
                        Push(null);
                    }
                    else
                        Push(o);
                    return;
                case ExpressionType.ArrayLength:
                    Push(((Array)o).Length);
                    return;
                case ExpressionType.Negate:
                    Push(Math.Negete(o, Type.GetTypeCode(unary.Type)));
                    return;
                case ExpressionType.NegateChecked:
                    Push(Math.NegeteChecked(o, Type.GetTypeCode(unary.Type)));
                    return;
                case ExpressionType.Not:
                    if (unary.Type == typeof(bool))
                        Push(!Convert.ToBoolean(o));
                    else
                        Push(~Convert.ToInt32(o));
                    return;
                case ExpressionType.UnaryPlus:
                    Push(o);
                    return;
                case ExpressionType.Convert:
                    Push(Math.ConvertToTypeUnchecked(o, unary.Operand.Type, unary.Type));
                    return;
                case ExpressionType.ConvertChecked:
                    Push(Math.ConvertToTypeChecked(o, unary.Operand.Type, unary.Type));
                    return;
            }
            throw new NotImplementedException(
                $"Interpriter for UnaryExpression with NodeType {unary.NodeType} is not implimented");

        }

        protected override void VisitNew(NewExpression nex)
        {
            Push(nex.Constructor == null
                ? Activator.CreateInstance(nex.Type)
                : nex.Constructor.Invoke(VisitListExpressions(nex.Arguments)));
        }

        protected override void VisitTypeIs(TypeBinaryExpression type)
        {
            Visit(type.Expression);
            Push(Math.IsType(type.TypeOperand, Pop()));
        }

        private void VisitMemberInfo(MemberInfo mi)
        {
            mi.OnFieldOrProperty(
                field => Push(field.GetValue(field.IsStatic ? null : Pop())),
                property => Push(property.GetValue(property.GetGetMethod(true).IsStatic ? null : Pop(), null)));
        }

        protected override void VisitMemberAccess(MemberExpression member)
        {
            Visit(member.Expression);
            VisitMemberInfo(member.Member);
        }

        protected override void VisitNewArray(NewArrayExpression newArray)
        {
            switch (newArray.NodeType)
            {
                case ExpressionType.NewArrayInit:
                    VisitNewArrayInit(newArray);
                    return;
                case ExpressionType.NewArrayBounds:
                    VisitNewArrayBounds(newArray);
                    return;
            }

            throw new NotSupportedException();
        }

        private void VisitNewArrayBounds(NewArrayExpression newArray)
        {
            var lengths = new int[newArray.Expressions.Count];
            for (int i = 0; i < lengths.Length; i++)
            {
                Visit(newArray.Expressions[i]);
                lengths[i] = (int)Pop();
            }

            Push(Array.CreateInstance(newArray.Type.GetElementType(), lengths));
        }

        private void VisitNewArrayInit(NewArrayExpression newArray)
        {
            var array = Array.CreateInstance(
                newArray.Type.GetElementType(), newArray.Expressions.Count);

            for (int i = 0; i < array.Length; i++)
            {
                Visit(newArray.Expressions[i]);
                array.SetValue(Pop(), i);
            }

            Push(array);
        }

        protected override void VisitConditional(ConditionalExpression conditional)
        {
            Visit(conditional.Test);

            if ((bool)Pop())
                Visit(conditional.IfTrue);
            else
                Visit(conditional.IfFalse);
        }

        protected override void VisitMethodCall(MethodCallExpression call)
        {
            object instance = null;
            if (call.Object != null)
            {
                Visit(call.Object);
                instance = Pop();
            }

            Push(call.Method.Invoke(instance, VisitListExpressions(call.Arguments)));
        }

        protected override void VisitParameter(ParameterExpression parameter)
        {
            for (int i = 0; i < lambda.Parameters.Count; i++)
            {
                if (lambda.Parameters[i] != parameter)
                    continue;

                Push(arguments[i]);
                return;
            }

            throw new ArgumentException();
        }

        protected override void VisitConstant(ConstantExpression constant)
        {
            Push(constant.Value);
        }

        protected override void VisitInvocation(InvocationExpression invocation)
        {
            Visit(invocation.Expression);
            var dlg = (Delegate)Pop();
            Push(Invoke(dlg, VisitListExpressions(invocation.Arguments)));
        }

        private static object Invoke(Delegate dlg, object[] arguments)
        {
            return dlg.Method.Invoke(null, arguments);
        }

        protected override void VisitMemberListBinding(MemberListBinding binding)
        {
            object o = Pop();
            try
            {
                VisitMemberInfo(binding.Member);
                base.VisitMemberListBinding(binding);
            }
            finally
            {
                Push(o);
            }
        }

        protected override void VisitElementInitializer(ElementInit initializer)
        {
            object o = Pop();
            try
            {
                object[] visitedArguments = VisitListExpressions(initializer.Arguments);
                initializer.AddMethod.Invoke(o, visitedArguments);
            }
            finally
            {
                Push(o);
            }
        }

        protected override void VisitMemberMemberBinding(MemberMemberBinding binding)
        {
            object o = Pop();
            try
            {
                VisitMemberInfo(binding.Member);
                base.VisitMemberMemberBinding(binding);
            }
            finally
            {
                Push(o);
            }
        }

        protected override void VisitMemberAssignment(MemberAssignment assignment)
        {
            object o = Pop();
            try
            {
                Visit(assignment.Expression);
                assignment.Member.OnFieldOrProperty(
                    field => field.SetValue(o, Pop()), property => property.SetValue(o, Pop(), null));
            }
            finally
            {
                Push(o);
            }
        }

        protected override void VisitLambda(LambdaExpression lambda)
        {
            Push(lambda.Compile());
        }

        private object[] VisitListExpressions(ReadOnlyCollection<Expression> collection)
        {
            object[] results = new object[collection.Count];
            for (int i = 0; i < results.Length; i++)
            {
                Visit(collection[i]);
                results[i] = Pop();
            }

            return results;
        }

        public static object Interpret(LambdaExpression lambda, object[] arguments)
        {
            var interpreter = new ExpressionInterpreter(lambda, arguments);
            interpreter.Visit(lambda.Body);

            return lambda.GetReturnType() != typeof(void)
                ? interpreter.Pop()
                : null;
        }
    }
}
