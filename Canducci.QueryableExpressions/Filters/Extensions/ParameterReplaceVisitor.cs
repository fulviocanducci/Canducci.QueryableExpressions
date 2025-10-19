using System;
using System.Linq.Expressions;

namespace Canducci.QueryableExpressions.Filters.Extensions
{
    internal sealed class ParameterReplaceVisitor : ExpressionVisitor
    {
        private readonly ParameterExpression _old;
        private readonly ParameterExpression _new;

        public ParameterReplaceVisitor(ParameterExpression oldParam, ParameterExpression newParam)
        {
            _old = oldParam ?? throw new ArgumentNullException(nameof(oldParam));
            _new = newParam ?? throw new ArgumentNullException(nameof(newParam));
        }

        protected override Expression VisitParameter(ParameterExpression node)
        {
            return node == _old ? _new : base.VisitParameter(node);
        }

        public static ParameterReplaceVisitor Create(ParameterExpression oldParam, ParameterExpression newParam)
        {
            return new ParameterReplaceVisitor(oldParam, newParam);
        }
    }
}