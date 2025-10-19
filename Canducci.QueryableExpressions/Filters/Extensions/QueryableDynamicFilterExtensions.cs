using Canducci.QueryableExpressions.Filters.Extensions.Internals;
using Canducci.QueryableExpressions.Filters.Extensions.Models;
using Canducci.QueryableExpressions.Filters.Extensions.Operators;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
namespace Canducci.QueryableExpressions.Filters.Extensions
{
    public static class QueryableDynamicFilterExtensions
    {
        public static IQueryable<T> DynamicFilters<T>(this IQueryable<T> query, IEnumerable<DynamicFilterItem> filters, bool combineWithOr = false)
        {
            if (filters == null || !filters.Any())
            {
                return query;
            }

            ParameterExpression parameter = Expression.Parameter(typeof(T), "x");
            Expression combined = null;
            foreach (var filter in filters)
            {
                if (string.IsNullOrWhiteSpace(filter.PropertyName))
                {
                    continue;
                }
                PropertyInfo property = typeof(T).GetProperty(filter.PropertyName);
                if (property == null)
                {
                    continue;
                }
                if (filter.Operator == FilterOperator.IsNull || filter.Operator == FilterOperator.IsNotNull)
                {
                    if (Nullable.GetUnderlyingType(property.PropertyType) == null && property.PropertyType.IsValueType)
                    {
                        throw new InvalidOperationException($"Operator {filter.Operator} cannot be used on non-nullable value property '{property.Name}'.");
                    }

                    if (property.IsDefined(typeof(RequiredAttribute), inherit: true))
                    {
                        throw new InvalidOperationException($"Operator {filter.Operator} cannot be used on property '{property.Name}' marked with [Required].");
                    }
                }
                Expression member = Expression.Property(parameter, property);
                Expression constant = null;
                if (filter.Operator != FilterOperator.IsNull && filter.Operator != FilterOperator.IsNotNull)
                {
                    if (filter.Value != null)
                    {
                        constant = ParameterExpressionBuilder.CreateParameterExpression(filter.Value, property.PropertyType);
                    }
                    else
                    {
                        constant = Expression.Constant(null, member.Type);
                    }
                }

                Expression comparison = GetComparisonExpression(member, constant, filter.Operator, property.PropertyType);

                if (combined == null)
                {
                    combined = comparison;
                }
                else
                {
                    if (combineWithOr)
                    {
                        combined = Expression.OrElse(combined, comparison);
                    }
                    else
                    {
                        combined = Expression.AndAlso(combined, comparison);
                    }
                }
            }

            if (combined == null)
            {
                return query;
            }

            Expression<Func<T, bool>> lambda = Expression.Lambda<Func<T, bool>>(combined, parameter);
            return query.Where(lambda);
        }

        public static IQueryable<T> DynamicFilter<T>(this IQueryable<T> query, string propertyName, object value, FilterOperator op = FilterOperator.Equal)
        {
            if (string.IsNullOrWhiteSpace(propertyName))
            {
                return query;
            }

            ParameterExpression parameter = Expression.Parameter(typeof(T), "x");
            PropertyInfo property = typeof(T).GetProperty(propertyName);
            if (property == null)
            {
                return query;
            }
            if (op == FilterOperator.IsNull || op == FilterOperator.IsNotNull)
            {
                if (Nullable.GetUnderlyingType(property.PropertyType) == null && property.PropertyType.IsValueType)
                {
                    throw new InvalidOperationException($"Operator {op} cannot be used on non-nullable value property '{property.Name}'.");
                }

                if (property.IsDefined(typeof(RequiredAttribute), inherit: true))
                {
                    throw new InvalidOperationException($"Operator {op} cannot be used on property '{property.Name}' marked with [Required].");
                }
            }

            Expression member = Expression.Property(parameter, property);
            Expression constant = null;

            if (op != FilterOperator.IsNull && op != FilterOperator.IsNotNull)
            {
                if (value != null)
                {
                    constant = ParameterExpressionBuilder.CreateParameterExpression(value, property.PropertyType);
                }
                else
                {
                    constant = Expression.Constant(null, member.Type);
                }
            }

            Expression comparison = GetComparisonExpression(member, constant, op, property.PropertyType);
            Expression<Func<T, bool>> lambda = Expression.Lambda<Func<T, bool>>(comparison, parameter);
            return query.Where(lambda);
        }

        public static IQueryable<T> DynamicFilterNull<T>(this IQueryable<T> query, string propertyName)
        {
            return query.DynamicFilter(propertyName, null, FilterOperator.IsNull);
        }

        public static IQueryable<T> DynamicFilterNotNull<T>(this IQueryable<T> query, string propertyName)
        {
            return query.DynamicFilter(propertyName, null, FilterOperator.IsNotNull);
        }

        public static IQueryable<T> DynamicFilterContains<T>(this IQueryable<T> query, string propertyName, object value)
        {
            return query.DynamicFilter(propertyName, value, FilterOperator.Contains);
        }

        public static IQueryable<T> DynamicFilterStartsWith<T>(this IQueryable<T> query, string propertyName, object value)
        {
            return query.DynamicFilter(propertyName, value, FilterOperator.StartsWith);
        }

        public static IQueryable<T> DynamicFilterEndsWith<T>(this IQueryable<T> query, string propertyName, object value)
        {
            return query.DynamicFilter(propertyName, value, FilterOperator.EndsWith);
        }

        public static IQueryable<T> DynamicFilterEqual<T>(this IQueryable<T> query, string propertyName, object value)
        {
            return query.DynamicFilter(propertyName, value, FilterOperator.Equal);
        }

        public static IQueryable<T> DynamicFilterGreaterThan<T>(this IQueryable<T> query, string propertyName, object value)
        {
            return query.DynamicFilter(propertyName, value, FilterOperator.GreaterThan);
        }

        public static IQueryable<T> DynamicFilterGreaterThanOrEqual<T>(this IQueryable<T> query, string propertyName, object value)
        {
            return query.DynamicFilter(propertyName, value, FilterOperator.GreaterThanOrEqual);
        }

        public static IQueryable<T> DynamicFilterLessThan<T>(this IQueryable<T> query, string propertyName, object value)
        {
            return query.DynamicFilter(propertyName, value, FilterOperator.LessThan);
        }

        public static IQueryable<T> DynamicFilterLessThanOrEqual<T>(this IQueryable<T> query, string propertyName, object value)
        {
            return query.DynamicFilter(propertyName, value, FilterOperator.LessThanOrEqual);
        }

        public static IQueryable<T> DynamicFilterIsNull<T>(this IQueryable<T> query, string propertyName)
        {
            return query.DynamicFilter(propertyName, null, FilterOperator.IsNull);
        }

        public static IQueryable<T> DynamicFilterIsNotNull<T>(this IQueryable<T> query, string propertyName)
        {
            return query.DynamicFilter(propertyName, null, FilterOperator.IsNotNull);
        }

        public static IQueryable<T> DynamicFilterContains<T>(this IQueryable<T> query, Expression<Func<T, object>> propertySelector, object value)
        {
            return query.DynamicFilter(GetPropertyName(propertySelector), value, FilterOperator.Contains);
        }

        public static IQueryable<T> DynamicFilterStartsWith<T>(this IQueryable<T> query, Expression<Func<T, object>> propertySelector, object value)
        {
            return query.DynamicFilter(GetPropertyName(propertySelector), value, FilterOperator.StartsWith);
        }

        public static IQueryable<T> DynamicFilterEndsWith<T>(this IQueryable<T> query, Expression<Func<T, object>> propertySelector, object value)
        {
            return query.DynamicFilter(GetPropertyName(propertySelector), value, FilterOperator.EndsWith);
        }

        public static IQueryable<T> DynamicFilterEqual<T>(this IQueryable<T> query, Expression<Func<T, object>> propertySelector, object value)
        {
            return query.DynamicFilter(GetPropertyName(propertySelector), value, FilterOperator.Equal);
        }

        public static IQueryable<T> DynamicFilterGreaterThan<T>(this IQueryable<T> query, Expression<Func<T, object>> propertySelector, object value)
        {
            return query.DynamicFilter(GetPropertyName(propertySelector), value, FilterOperator.GreaterThan);
        }

        public static IQueryable<T> DynamicFilterGreaterThanOrEqual<T>(this IQueryable<T> query, Expression<Func<T, object>> propertySelector, object value)
        {
            return query.DynamicFilter(GetPropertyName(propertySelector), value, FilterOperator.GreaterThanOrEqual);
        }

        public static IQueryable<T> DynamicFilterLessThan<T>(this IQueryable<T> query, Expression<Func<T, object>> propertySelector, object value)
        {
            return query.DynamicFilter(GetPropertyName(propertySelector), value, FilterOperator.LessThan);
        }

        public static IQueryable<T> DynamicFilterLessThanOrEqual<T>(this IQueryable<T> query, Expression<Func<T, object>> propertySelector, object value)
        {
            return query.DynamicFilter(GetPropertyName(propertySelector), value, FilterOperator.LessThanOrEqual);
        }

        public static IQueryable<T> DynamicFilterIsNull<T>(this IQueryable<T> query, Expression<Func<T, object>> propertySelector)
        {
            return query.DynamicFilter(GetPropertyName(propertySelector), null, FilterOperator.IsNull);
        }

        public static IQueryable<T> DynamicFilterIsNotNull<T>(this IQueryable<T> query, Expression<Func<T, object>> propertySelector)
        {
            return query.DynamicFilter(GetPropertyName(propertySelector), null, FilterOperator.IsNotNull);
        }

        private static bool IsNotAssignableFrom(Type type)
        {
            return typeof(string).IsAssignableFrom(type) == false;
        }

        private static Expression GetComparisonExpression(Expression member, Expression constant, FilterOperator op, Type propertyType)
        {
            Expression originalMember = member;
            Type underlyingType = Nullable.GetUnderlyingType(propertyType);
            if (underlyingType != null && op != FilterOperator.IsNull && op != FilterOperator.IsNotNull)
            {
                member = Expression.Property(member, "Value");
                if (constant != null && constant.Type != underlyingType)
                {
                    constant = Expression.Convert(constant, underlyingType);
                }
            }
            else
            {
                underlyingType = propertyType;
            }

            Expression comparison = null;
            switch (op)
            {
                case FilterOperator.Contains:
                    {
                        if (IsNotAssignableFrom(underlyingType))
                        {
                            throw new InvalidOperationException("Contains operator can only be used on string properties.");
                        }
                        comparison = Expression.Call(member, typeof(string).GetMethod(nameof(string.Contains), new[] { typeof(string) }), constant);
                        break;
                    }
                case FilterOperator.StartsWith:
                    {
                        if (IsNotAssignableFrom(underlyingType))
                        {
                            throw new InvalidOperationException("StartsWith operator can only be used on string properties.");
                        }
                        comparison = Expression.Call(member, typeof(string).GetMethod(nameof(string.StartsWith), new[] { typeof(string) }), constant);
                        break;
                    }
                case FilterOperator.EndsWith:
                    {
                        if (IsNotAssignableFrom(underlyingType))
                        {
                            throw new InvalidOperationException("EndsWith operator can only be used on string properties.");
                        }
                        comparison = Expression.Call(member, typeof(string).GetMethod(nameof(string.EndsWith), new[] { typeof(string) }), constant);
                        break;
                    }
                case FilterOperator.Equal:
                    {
                        comparison = Expression.Equal(member, constant);
                        break;
                    }
                case FilterOperator.GreaterThan:
                    {
                        comparison = Expression.GreaterThan(member, constant);
                        break;
                    }
                case FilterOperator.GreaterThanOrEqual:
                    {
                        comparison = Expression.GreaterThanOrEqual(member, constant);
                        break;
                    }
                case FilterOperator.LessThan:
                    {
                        comparison = Expression.LessThan(member, constant);
                        break;
                    }
                case FilterOperator.LessThanOrEqual:
                    {
                        comparison = Expression.LessThanOrEqual(member, constant);
                        break;
                    }
                case FilterOperator.IsNull:
                    {
                        if (Nullable.GetUnderlyingType(propertyType) == null && propertyType.IsValueType)
                        {
                            throw new InvalidOperationException("IsNull operator can only be used on nullable or reference properties.");
                        }

                        comparison = Expression.Equal(originalMember, Expression.Constant(null, propertyType));
                        break;
                    }
                case FilterOperator.IsNotNull:
                    {
                        if (Nullable.GetUnderlyingType(propertyType) == null && propertyType.IsValueType)
                        {
                            throw new InvalidOperationException("IsNotNull operator can only be used on nullable or reference properties.");
                        }

                        comparison = Expression.NotEqual(originalMember, Expression.Constant(null, propertyType));
                        break;
                    }
                default:
                    throw new NotSupportedException($"Operator {op} is not supported.");
            }
            return comparison;
        }

        private static string GetPropertyName<T>(Expression<Func<T, object>> selector)
        {
            if (selector == null) throw new ArgumentNullException(nameof(selector));
            Expression body = selector.Body;
            if (body is UnaryExpression unary && unary.Operand is MemberExpression memberUnary)
            {
                return memberUnary.Member.Name;
            }
            if (body is MemberExpression member)
            {
                return member.Member.Name;
            }
            throw new ArgumentException("Expression must be a simple member access", nameof(selector));
        }
    }
}