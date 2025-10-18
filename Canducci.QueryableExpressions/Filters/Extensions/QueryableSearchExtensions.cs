using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Canducci.QueryableExpressions.Filters.Extensions
{
    public static class QueryableSearchExtensions
    {
        public static IQueryable<T> ApplySearch<T>(this IQueryable<T> query, string search, SearchComparisonMode mode = SearchComparisonMode.Contains, params Expression<Func<T, string>>[] properties)
        {
            if (string.IsNullOrWhiteSpace(search) || properties == null || properties.Length == 0)
            {
                return query;
            }

            search = search.Trim();
            Expression combined = null;
            ParameterExpression parameter = Expression.Parameter(typeof(T), "x");

            foreach (var property in properties)
            {
                // Replace the lambda parameter with the query parameter to get a real member access
                Expression member = ReplaceParameter(property.Body, property.Parameters[0], parameter);

                // Null check and comparison using the shared parameter builder
                BinaryExpression notNull = Expression.NotEqual(member, Expression.Constant(null, typeof(string)));
                Expression comparison = GetExpressionComparison(mode, member, search);
                BinaryExpression andAlso = Expression.AndAlso(notNull, comparison);
                combined = combined == null ? andAlso : Expression.OrElse(combined, andAlso);
            }

            if (combined == null) return query;
            Expression<Func<T, bool>> lambda = Expression.Lambda<Func<T, bool>>(combined, parameter);
            return query.Where(lambda);
        }

        public static IQueryable<T> ApplySearch<T>(this IQueryable<T> query, string search, SearchComparisonMode mode = SearchComparisonMode.Contains, params string[] propertyNames)
        {
            if (string.IsNullOrWhiteSpace(search) || propertyNames == null || propertyNames.Length == 0)
            {
                return query;
            }

            search = search.Trim();
            ParameterExpression parameter = Expression.Parameter(typeof(T), "x");
            Expression combined = null;

            foreach (var propertyName in propertyNames)
            {
                PropertyInfo propertyInfo = typeof(T).GetProperty(propertyName);
                if (propertyInfo == null || propertyInfo.PropertyType != typeof(string)) continue;

                MemberExpression member = Expression.Property(parameter, propertyInfo);
                BinaryExpression notNull = Expression.NotEqual(member, Expression.Constant(null, typeof(string)));
                Expression comparison = GetExpressionComparison(mode, member, search);
                BinaryExpression andAlso = Expression.AndAlso(notNull, comparison);
                combined = combined == null ? andAlso : Expression.OrElse(combined, andAlso);
            }

            if (combined == null) return query;
            Expression<Func<T, bool>> lambda = Expression.Lambda<Func<T, bool>>(combined, parameter);
            return query.Where(lambda);
        }

        private static Expression GetExpressionComparison(SearchComparisonMode mode, Expression member, string search)
        {
            // Build a strongly-typed parameter expression for the search string so EF emits a DbParameter
            Expression searchConstant = ParameterExpressionBuilder.CreateParameterExpression(search, typeof(string));

            switch (mode)
            {
                case SearchComparisonMode.StartsWith:
                    return Expression.Call(member, typeof(string).GetMethod(nameof(string.StartsWith), new[] { typeof(string) }), searchConstant);
                case SearchComparisonMode.EndsWith:
                    return Expression.Call(member, typeof(string).GetMethod(nameof(string.EndsWith), new[] { typeof(string) }), searchConstant);
                case SearchComparisonMode.Exactly:
                    return Expression.Equal(member, searchConstant);
                default:
                    return Expression.Call(member, typeof(string).GetMethod(nameof(string.Contains), new[] { typeof(string) }), searchConstant);
            }
        }

        // Replace occurrences of oldParam with newParam inside expression
        private static Expression ReplaceParameter(Expression expression, ParameterExpression oldParam, ParameterExpression newParam)
        {
            return new ParameterReplaceVisitor(oldParam, newParam).Visit(expression);
        }

        private sealed class ParameterReplaceVisitor : ExpressionVisitor
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
        }
    }
}