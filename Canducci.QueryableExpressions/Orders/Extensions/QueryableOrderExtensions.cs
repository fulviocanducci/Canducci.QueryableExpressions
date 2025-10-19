using Canducci.QueryableExpressions.Filters.Extensions.Internals;
using Canducci.QueryableExpressions.Orders.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
namespace Canducci.QueryableExpressions.Orders.Extensions
{
    public static class QueryableOrderExtensions
    {
        public static IQueryable<T> DynamicOrderBy<T>(this IQueryable<T> source, IEnumerable<Order> orders)
        {
            if (orders == null)
            {
                return source;
            }
            return DynamicOrderBy<T>(source, orders.ToArray());
        }

        public static IQueryable<T> DynamicOrderBy<T>(this IQueryable<T> source, params Order[] orders)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }
            if (orders == null || orders.Length == 0)
            {
                return source;
            }
            IQueryable<T> query = source;
            ParameterExpression parameter = Expression.Parameter(typeof(T), "x");
            for (int i = 0; i < orders.Length; i++)
            {
                if (!(orders.GetValue(i) is Order order))
                {
                    continue;
                }
                PropertyInfo property = ReflectionCache.GetPropertyInfo(typeof(T), order.PropertyName);
                if (property == null)
                {
                    continue;
                }
                Expression propertyAccess = Expression.Property(parameter, property);
                LambdaExpression keySelector = Expression.Lambda(propertyAccess, parameter);
                string methodName;
                methodName = i == 0 ? (order.Descending ? "OrderByDescending" : "OrderBy") : (order.Descending ? "ThenByDescending" : "ThenBy");
                MethodInfo genericMethod = GetQueryableMethod(methodName, typeof(T), property.PropertyType);
                MethodInfo constructed = genericMethod.MakeGenericMethod(typeof(T), property.PropertyType);
                Expression call = Expression.Call(null, constructed, new Expression[] { query.Expression, Expression.Quote(keySelector) });
                query = query.Provider.CreateQuery<T>(call);
            }
            return query;
        }

        private static MethodInfo GetQueryableMethod(string name, Type sourceType, Type keyType)
        {
            MethodInfo[] methods = typeof(Queryable).GetMethods(BindingFlags.Public | BindingFlags.Static);
            MethodInfo method = null;
            foreach (MethodInfo m in methods)
            {
                if (string.Equals(m.Name, name, StringComparison.Ordinal) == false)
                {
                    continue;
                }
                ParameterInfo[] parameters = m.GetParameters();
                if (parameters.Length != 2)
                {
                    continue;
                }
                if (parameters[0].ParameterType.IsGenericType == false)
                {
                    continue;
                }
                method = m;
                break;
            }
            if (method == null)
            {
                throw new InvalidOperationException(string.Format("Could not find Queryable method '{0}'.", name));
            }
            return method;
        }
    }
}
