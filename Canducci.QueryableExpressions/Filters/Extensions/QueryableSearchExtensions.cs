using Canducci.QueryableExpressions.Filters.Extensions.Internals;
using Canducci.QueryableExpressions.Filters.Extensions.Operators;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
namespace Canducci.QueryableExpressions.Filters.Extensions
{
    public static class QueryableSearchExtensions
    {
        public static IQueryable<T> ApplySearch<T>(this IQueryable<T> query, string search, SearchOperator mode = SearchOperator.Contains, params Expression<Func<T, string>>[] properties)
        {
            if (string.IsNullOrWhiteSpace(search) || properties == null || properties.Length == 0)
            {
                return query;
            }

            string trimmed = search.Trim();
            Expression combined = null;
            ParameterExpression parameter = Expression.Parameter(typeof(T), "x");
            Expression searchConstant = ParameterExpressionBuilder.CreateParameterExpression(trimmed, typeof(string));

            MethodInfo containsMethod = typeof(string).GetMethod(nameof(string.Contains), new[] { typeof(string) });
            MethodInfo startsWithMethod = typeof(string).GetMethod(nameof(string.StartsWith), new[] { typeof(string) });
            MethodInfo endsWithMethod = typeof(string).GetMethod(nameof(string.EndsWith), new[] { typeof(string) });

            foreach (Expression<Func<T, string>> property in properties)
            {
                Expression member = ReplaceParameter(property.Body, property.Parameters[0], parameter);
                BinaryExpression notNull = Expression.NotEqual(member, Expression.Constant(null, typeof(string)));
                Expression comparison;

                switch (mode)
                {
                    case SearchOperator.StartsWith:
                        comparison = Expression.Call(member, startsWithMethod, searchConstant);
                        break;

                    case SearchOperator.EndsWith:
                        comparison = Expression.Call(member, endsWithMethod, searchConstant);
                        break;

                    case SearchOperator.Exactly:
                        comparison = Expression.Equal(member, searchConstant);
                        break;

                    default:
                        comparison = Expression.Call(member, containsMethod, searchConstant);
                        break;
                }

                BinaryExpression andAlso = Expression.AndAlso(notNull, comparison);
                if (combined == null)
                {
                    combined = andAlso;
                }
                else
                {
                    combined = Expression.OrElse(combined, andAlso);
                }
            }

            if (combined == null)
            {
                return query;
            }

            Expression<Func<T, bool>> lambda = Expression.Lambda<Func<T, bool>>(combined, parameter);
            return query.Where(lambda);
        }

        public static IQueryable<T> ApplySearch<T>(this IQueryable<T> query, string search, SearchOperator mode = SearchOperator.Contains, params string[] propertyNames)
        {
            if (string.IsNullOrWhiteSpace(search) || propertyNames == null || propertyNames.Length == 0)
            {
                return query;
            }

            string trimmed = search.Trim();
            ParameterExpression parameter = Expression.Parameter(typeof(T), "x");
            Expression combined = null;
            Expression searchConstant = ParameterExpressionBuilder.CreateParameterExpression(trimmed, typeof(string));

            MethodInfo containsMethod = typeof(string).GetMethod(nameof(string.Contains), new[] { typeof(string) });
            MethodInfo startsWithMethod = typeof(string).GetMethod(nameof(string.StartsWith), new[] { typeof(string) });
            MethodInfo endsWithMethod = typeof(string).GetMethod(nameof(string.EndsWith), new[] { typeof(string) });

            foreach (string propertyName in propertyNames)
            {
                PropertyInfo propertyInfo = typeof(T).GetProperty(propertyName);
                if (propertyInfo == null || propertyInfo.PropertyType != typeof(string))
                {
                    continue;
                }

                MemberExpression member = Expression.Property(parameter, propertyInfo);
                BinaryExpression notNull = Expression.NotEqual(member, Expression.Constant(null, typeof(string)));
                Expression comparison;

                switch (mode)
                {
                    case SearchOperator.StartsWith:
                        comparison = Expression.Call(member, startsWithMethod, searchConstant);
                        break;

                    case SearchOperator.EndsWith:
                        comparison = Expression.Call(member, endsWithMethod, searchConstant);
                        break;

                    case SearchOperator.Exactly:
                        comparison = Expression.Equal(member, searchConstant);
                        break;

                    default:
                        comparison = Expression.Call(member, containsMethod, searchConstant);
                        break;
                }

                BinaryExpression andAlso = Expression.AndAlso(notNull, comparison);
                if (combined == null)
                {
                    combined = andAlso;
                }
                else
                {
                    combined = Expression.OrElse(combined, andAlso);
                }
            }

            if (combined == null)
            {
                return query;
            }

            Expression<Func<T, bool>> lambda = Expression.Lambda<Func<T, bool>>(combined, parameter);
            return query.Where(lambda);
        }

        public static IQueryable<T> ApplySearchContains<T>(this IQueryable<T> query, string search, params Expression<Func<T, string>>[] properties)
        {
            return ApplySearch(query, search, SearchOperator.Contains, properties);
        }

        public static IQueryable<T> ApplySearchStartsWith<T>(this IQueryable<T> query, string search, params Expression<Func<T, string>>[] properties)
        {
            return ApplySearch(query, search, SearchOperator.StartsWith, properties);
        }

        public static IQueryable<T> ApplySearchEndsWith<T>(this IQueryable<T> query, string search, params Expression<Func<T, string>>[] properties)
        {
            return ApplySearch(query, search, SearchOperator.EndsWith, properties);
        }

        public static IQueryable<T> ApplySearchExactly<T>(this IQueryable<T> query, string search, params Expression<Func<T, string>>[] properties)
        {
            return ApplySearch(query, search, SearchOperator.Exactly, properties);
        }

        public static IQueryable<T> ApplySearchContains<T>(this IQueryable<T> query, string search, params string[] propertyNames)
        {
            return ApplySearch(query, search, SearchOperator.Contains, propertyNames);
        }

        public static IQueryable<T> ApplySearchStartsWith<T>(this IQueryable<T> query, string search, params string[] propertyNames)
        {
            return ApplySearch(query, search, SearchOperator.StartsWith, propertyNames);
        }

        public static IQueryable<T> ApplySearchEndsWith<T>(this IQueryable<T> query, string search, params string[] propertyNames)
        {
            return ApplySearch(query, search, SearchOperator.EndsWith, propertyNames);
        }

        public static IQueryable<T> ApplySearchExactly<T>(this IQueryable<T> query, string search, params string[] propertyNames)
        {
            return ApplySearch(query, search, SearchOperator.Exactly, propertyNames);
        }

        private static Expression ReplaceParameter(Expression expression, ParameterExpression oldParam, ParameterExpression newParam)
        {
            ParameterReplaceVisitor visitor = new ParameterReplaceVisitor(oldParam, newParam);
            return visitor.Visit(expression);
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
                if (node == _old)
                {
                    return _new;
                }

                return base.VisitParameter(node);
            }
        }
    }
}