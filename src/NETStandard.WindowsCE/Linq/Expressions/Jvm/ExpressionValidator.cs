using System;

#if NET35_CF
namespace System.Linq.Expressions.Jvm
#else
namespace Mock.System.Linq.Expressions.Jvm
#endif
{
    internal class ExpressionValidator : ExpressionVisitor
    {
        private readonly LambdaExpression exp;

        internal ExpressionValidator(LambdaExpression exp)
        {
            this.exp = exp;
        }

        protected override void Visit(Expression expression)
        {
            if (expression == null)
            {
                return;
            }
            if (expression.NodeType == ExpressionType.Power)
            {
                VisitBinary((BinaryExpression)expression);
            }
            else
            {
                base.Visit(expression);
            }
        }

        protected override void VisitParameter(ParameterExpression parameter)
        {
            foreach (ParameterExpression pe in exp.Parameters)
            {
                if (pe.Name.Equals(parameter.Name) &&
                    !ReferenceEquals(parameter, pe))
                {
                    throw new InvalidOperationException("Lambda Parameter not in scope");
                }
            }
        }

        internal void Validate()
        {
            Visit(exp);
        }
    }
}
