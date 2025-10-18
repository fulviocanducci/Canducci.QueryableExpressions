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
                InvocationExpression member = Expression.Invoke(property, parameter);
                BinaryExpression notNull = Expression.NotEqual(member, Expression.Constant(null, typeof(string)));
                Expression comparison = GetExpressionComparison(mode, member, search);
                BinaryExpression andAlso = Expression.AndAlso(notNull, comparison);
                combined = combined == null ? andAlso : Expression.OrElse(combined, andAlso);
            }
            if (combined == null)
            {
                return query;
            }
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
                if (propertyInfo == null || propertyInfo.PropertyType != typeof(string))
                {
                    continue;
                }
                MemberExpression member = Expression.Property(parameter, propertyInfo);
                BinaryExpression notNull = Expression.NotEqual(member, Expression.Constant(null, typeof(string)));
                Expression comparison = GetExpressionComparison(mode, member, search);
                BinaryExpression andAlso = Expression.AndAlso(notNull, comparison);
                combined = combined == null ? andAlso : Expression.OrElse(combined, andAlso);
            }
            if (combined == null)
            {
                return query;
            }
            Expression<Func<T, bool>> lambda = Expression.Lambda<Func<T, bool>>(combined, parameter);
            return query.Where(lambda);
        }

        private static Expression GetExpressionComparison(SearchComparisonMode mode, Expression member, string search)
        {
            Expression comparison = null;
            switch (mode)
            {
                case SearchComparisonMode.StartsWith:
                    {
                        comparison = Expression.Call(member, typeof(string).GetMethod(nameof(string.StartsWith), new[] { typeof(string) }), Expression.Constant(search));
                        break;
                    }
                case SearchComparisonMode.EndsWith:
                    {
                        comparison = Expression.Call(member, typeof(string).GetMethod(nameof(string.EndsWith), new[] { typeof(string) }), Expression.Constant(search));
                        break;
                    }
                case SearchComparisonMode.Exactly:
                    {
                        comparison = Expression.Equal(member, Expression.Constant(search));
                        break;
                    }
                default:
                    {
                        comparison = Expression.Call(member, typeof(string).GetMethod(nameof(string.Contains), new[] { typeof(string) }), Expression.Constant(search));
                        break;
                    }
            }
            return comparison;
        }
    }
}