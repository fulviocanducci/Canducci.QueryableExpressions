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
            Expression searchConstant = ParameterExpressionBuilder.CreateParameterExpression(search, typeof(string));
            foreach (var property in properties)
            {            
                Expression member = ReplaceParameter(property.Body, property.Parameters[0], parameter);                
                BinaryExpression notNull = Expression.NotEqual(member, Expression.Constant(null, typeof(string)));
                Expression comparison = GetExpressionComparison(mode, member, searchConstant);
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
            Expression searchConstant = ParameterExpressionBuilder.CreateParameterExpression(search, typeof(string));
            foreach (var propertyName in propertyNames)
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
                combined = combined == null ? andAlso : Expression.OrElse(combined, andAlso);
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
            return ApplySearch(query, search, SearchComparisonMode.Contains, properties);
        }

        public static IQueryable<T> ApplySearchStartsWith<T>(this IQueryable<T> query, string search, params Expression<Func<T, string>>[] properties)
        {
            return ApplySearch(query, search, SearchComparisonMode.StartsWith, properties);
        }

        public static IQueryable<T> ApplySearchEndsWith<T>(this IQueryable<T> query, string search, params Expression<Func<T, string>>[] properties)
        {
            return ApplySearch(query, search, SearchComparisonMode.EndsWith, properties);
        }

        public static IQueryable<T> ApplySearchExactly<T>(this IQueryable<T> query, string search, params Expression<Func<T, string>>[] properties)
        {
            return ApplySearch(query, search, SearchComparisonMode.Exactly, properties);
        }

        public static IQueryable<T> ApplySearchContains<T>(this IQueryable<T> query, string search, params string[] propertyNames)
        {
            return ApplySearch(query, search, SearchComparisonMode.Contains, propertyNames);
        }

        public static IQueryable<T> ApplySearchStartsWith<T>(this IQueryable<T> query, string search, params string[] propertyNames)
        {
            return ApplySearch(query, search, SearchComparisonMode.StartsWith, propertyNames);
        }

        public static IQueryable<T> ApplySearchEndsWith<T>(this IQueryable<T> query, string search, params string[] propertyNames)
        {
            return ApplySearch(query, search, SearchComparisonMode.EndsWith, propertyNames);
        }

        public static IQueryable<T> ApplySearchExactly<T>(this IQueryable<T> query, string search, params string[] propertyNames)
        {
            return ApplySearch(query, search, SearchComparisonMode.Exactly, propertyNames);
        }

        private static Expression GetExpressionComparison(SearchComparisonMode mode, Expression member, Expression searchConstant)
        {
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

        private static Expression ReplaceParameter(Expression expression, ParameterExpression oldParam, ParameterExpression newParam)
        {
            return ParameterReplaceVisitor
                .Create(oldParam, newParam)
                .Visit(expression);
        }        
    }
}