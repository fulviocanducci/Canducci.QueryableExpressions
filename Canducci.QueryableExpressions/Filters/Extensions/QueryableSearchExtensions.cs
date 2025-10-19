using Canducci.QueryableExpressions.Filters.Extensions.Internals;
using Canducci.QueryableExpressions.Filters.Extensions.Operators;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
namespace Canducci.QueryableExpressions.Filters.Extensions
{
    public static class QueryableSearchExtensions
    {
        private static readonly ConcurrentDictionary<string, MethodInfo> s_stringMethodCache = new ConcurrentDictionary<string, MethodInfo>();
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

            foreach (Expression<Func<T, string>> property in properties)
            {
                Expression member = ReplaceParameter(property.Body, property.Parameters[0], parameter);
                BinaryExpression notNull = Expression.NotEqual(member, Expression.Constant(null, typeof(string)));
                Expression comparison = GetExpressionComparison(mode, member, searchConstant);
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

            foreach (string propertyName in propertyNames)
            {
                PropertyInfo propertyInfo = typeof(T).GetProperty(propertyName);
                if (propertyInfo == null || propertyInfo.PropertyType != typeof(string))
                {
                    continue;
                }

                MemberExpression member = Expression.Property(parameter, propertyInfo);
                BinaryExpression notNull = Expression.NotEqual(member, Expression.Constant(null, typeof(string)));
                Expression comparison = GetExpressionComparison(mode, member, searchConstant);
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

        private static MethodInfo GetStringMethod(string name)
        {
            MethodInfo method = s_stringMethodCache.GetOrAdd(name, (string methodName) =>
            {
                return typeof(string).GetMethod(methodName, new[] { typeof(string) });
            });

            return method;
        }

        private static Expression GetExpressionComparison(SearchOperator mode, Expression member, Expression searchConstant)
        {
            switch (mode)
            {
                case SearchOperator.StartsWith:
                    return Expression.Call(member, GetStringMethod(nameof(string.StartsWith)), searchConstant);
                case SearchOperator.EndsWith:
                    return Expression.Call(member, GetStringMethod(nameof(string.EndsWith)), searchConstant);
                case SearchOperator.Exactly:
                    return Expression.Equal(member, searchConstant);
                default:
                    return Expression.Call(member, GetStringMethod(nameof(string.Contains)), searchConstant);
            }
        }

        private static Expression ReplaceParameter(Expression expression, ParameterExpression oldParam, ParameterExpression newParam)
        {
            ParameterReplaceVisitor visitor = ParameterReplaceVisitor.Create(oldParam, newParam);
            return visitor.Visit(expression);
        }
    }
}