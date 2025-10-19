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

            foreach (DynamicFilterItem filter in filters)
            {
                if (string.IsNullOrWhiteSpace(filter.PropertyName))
                {
                    continue;
                }

                PropertyInfo property = GetPropertyInfo<T>(filter.PropertyName);
                if (property == null)
                {
                    continue;
                }

                ValidateNullOperators(property, filter.Operator);

                Expression member = Expression.Property(parameter, property);
                Expression constant = BuildConstantExpression(filter.Value, property.PropertyType, member);

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

            return query.Where(Expression.Lambda<Func<T, bool>>(combined, parameter));
        }

        public static IQueryable<T> DynamicFilter<T>(this IQueryable<T> query, string propertyName, object value, FilterOperator op = FilterOperator.Equal)
        {
            if (string.IsNullOrWhiteSpace(propertyName))
            {
                return query;
            }

            PropertyInfo property = GetPropertyInfo<T>(propertyName);
            if (property == null)
            {
                return query;
            }

            ValidateNullOperators(property, op);

            ParameterExpression parameter = Expression.Parameter(typeof(T), "x");
            Expression member = Expression.Property(parameter, property);
            Expression constant = BuildConstantExpression(value, property.PropertyType, member);

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

        public static IQueryable<T> DynamicFilterContains<T, TKey>(this IQueryable<T> query, Expression<Func<T, TKey>> propertySelector, object value)
        {
            return query.DynamicFilter(GetPropertyName(propertySelector), value, FilterOperator.Contains);
        }

        public static IQueryable<T> DynamicFilterStartsWith<T, TKey>(this IQueryable<T> query, Expression<Func<T, TKey>> propertySelector, object value)
        {
            return query.DynamicFilter(GetPropertyName(propertySelector), value, FilterOperator.StartsWith);
        }

        public static IQueryable<T> DynamicFilterEndsWith<T, TKey>(this IQueryable<T> query, Expression<Func<T, TKey>> propertySelector, object value)
        {
            return query.DynamicFilter(GetPropertyName(propertySelector), value, FilterOperator.EndsWith);
        }

        public static IQueryable<T> DynamicFilterEqual<T, TKey>(this IQueryable<T> query, Expression<Func<T, TKey>> propertySelector, object value)
        {
            return query.DynamicFilter(GetPropertyName(propertySelector), value, FilterOperator.Equal);
        }

        public static IQueryable<T> DynamicFilterGreaterThan<T, TKey>(this IQueryable<T> query, Expression<Func<T, TKey>> propertySelector, object value)
        {
            return query.DynamicFilter(GetPropertyName(propertySelector), value, FilterOperator.GreaterThan);
        }

        public static IQueryable<T> DynamicFilterGreaterThanOrEqual<T, TKey>(this IQueryable<T> query, Expression<Func<T, TKey>> propertySelector, object value)
        {
            return query.DynamicFilter(GetPropertyName(propertySelector), value, FilterOperator.GreaterThanOrEqual);
        }

        public static IQueryable<T> DynamicFilterLessThan<T, TKey>(this IQueryable<T> query, Expression<Func<T, TKey>> propertySelector, object value)
        {
            return query.DynamicFilter(GetPropertyName(propertySelector), value, FilterOperator.LessThan);
        }

        public static IQueryable<T> DynamicFilterLessThanOrEqual<T, TKey>(this IQueryable<T> query, Expression<Func<T, TKey>> propertySelector, object value)
        {
            return query.DynamicFilter(GetPropertyName(propertySelector), value, FilterOperator.LessThanOrEqual);
        }

        public static IQueryable<T> DynamicFilterIsNull<T, TKey>(this IQueryable<T> query, Expression<Func<T, TKey>> propertySelector)
        {
            return query.DynamicFilter(GetPropertyName(propertySelector), null, FilterOperator.IsNull);
        }

        public static IQueryable<T> DynamicFilterIsNotNull<T, TKey>(this IQueryable<T> query, Expression<Func<T, TKey>> propertySelector)
        {
            return query.DynamicFilter(GetPropertyName(propertySelector), null, FilterOperator.IsNotNull);
        }

        #region private_methods
        private static PropertyInfo GetPropertyInfo<T>(string propertyName)
        {
            return ReflectionCache.GetPropertyInfo(typeof(T), propertyName);
        }

        private static void ValidateNullOperators(PropertyInfo property, FilterOperator op)
        {
            if (op != FilterOperator.IsNull && op != FilterOperator.IsNotNull)
            {
                return;
            }

            NullableGetUnderlyingTypeOrInvalidOperationException(property.PropertyType, string.Format("Operator {0} cannot be used on non-nullable value property '{1}'.", op, property.Name));
            PropertyIsDefinedOrInvalidOperationException(property, string.Format("Operator {0} cannot be used on property '{1}' marked with [Required].", op, property.Name));
        }

        private static void PropertyIsDefinedOrInvalidOperationException(PropertyInfo property, string errorMessage)
        {
            if (property.IsDefined(typeof(RequiredAttribute), inherit: true))
            {
                throw new InvalidOperationException(errorMessage);
            }
        }

        private static Expression BuildConstantExpression(object value, Type propertyType, Expression member)
        {
            if (value == null)
            {
                return Expression.Constant(null, member.Type);
            }

            return ParameterExpressionBuilder.CreateParameterExpression(value, propertyType);
        }

        private static bool IsNotStringType(Type type)
        {
            return typeof(string).IsAssignableFrom(type) == false;
        }

        private static void IsNotStringTypeOrInvalidOperationException(Type type, string errorMessage)
        {
            if (IsNotStringType(type))
            {
                throw new InvalidOperationException(errorMessage);
            }
        }

        private static MethodInfo GetStringMethod(string name)
        {
            return ReflectionCache.GetStringMethod(name);
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
                        IsNotStringTypeOrInvalidOperationException(underlyingType, "Contains operator can only be used on string properties.");
                        MethodInfo containsMethod = GetStringMethod(nameof(string.Contains));
                        comparison = Expression.Call(member, containsMethod, constant);
                        break;
                    }

                case FilterOperator.StartsWith:
                    {
                        IsNotStringTypeOrInvalidOperationException(underlyingType, "StartsWith operator can only be used on string properties.");
                        MethodInfo startsWithMethod = GetStringMethod(nameof(string.StartsWith));
                        comparison = Expression.Call(member, startsWithMethod, constant);
                        break;
                    }

                case FilterOperator.EndsWith:
                    {
                        IsNotStringTypeOrInvalidOperationException(underlyingType, "EndsWith operator can only be used on string properties.");
                        MethodInfo endsWithMethod = GetStringMethod(nameof(string.EndsWith));
                        comparison = Expression.Call(member, endsWithMethod, constant);
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
                        NullableGetUnderlyingTypeOrInvalidOperationException(propertyType, "NotNull operator can only be used on nullable or reference properties.");
                        comparison = Expression.Equal(originalMember, Expression.Constant(null, propertyType));
                        break;
                    }

                case FilterOperator.IsNotNull:
                    {
                        NullableGetUnderlyingTypeOrInvalidOperationException(propertyType, "IsNotNull operator can only be used on nullable or reference properties.");
                        comparison = Expression.NotEqual(originalMember, Expression.Constant(null, propertyType));
                        break;
                    }

                default:
                    {
                        throw new NotSupportedException(string.Format("Operator {0} is not supported.", op));
                    }
            }

            return comparison;
        }

        private static void NullableGetUnderlyingTypeOrInvalidOperationException(Type propertyType, string errorMessage)
        {
            if (Nullable.GetUnderlyingType(propertyType) == null && propertyType.IsValueType)
            {
                throw new InvalidOperationException(errorMessage);
            }
        }

        private static string GetPropertyName<T, TKey>(Expression<Func<T, TKey>> selector)
        {
            if (selector == null)
            {
                throw new ArgumentNullException(nameof(selector));
            }

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
        #endregion
    }
}