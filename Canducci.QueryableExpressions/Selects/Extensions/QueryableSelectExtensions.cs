using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
namespace Canducci.QueryableExpressions.Selects.Extensions
{
    public static class QueryableSelectExtensions
    {
        public static IQueryable<TResult> DynamicSelectBy<T, TResult>(this IQueryable<T> query, string fields)
        {
            return DynamicSelectBy<T, TResult>(query, fields.Split(',').Select(x => x.Trim()).Distinct().ToArray());
        }
        
        public static IQueryable<TResult> DynamicSelectBy<T, TResult>(this IQueryable<T> query, params string[] fields)
        {
            if (fields == null || fields.Length == 0)
            {
                throw new ArgumentException($"No field");
            }
            string[] validFields = fields.Where(f => !string.IsNullOrWhiteSpace(f)).Select(x => x.Trim()).Distinct().ToArray();
            if (validFields.Length == 0)
            {
                throw new ArgumentException($"No field");
            }
            Type entityType = typeof(T);
            List<PropertyInfo> properties = new List<PropertyInfo>();
            List<string> invalidFields = new List<string>();
            foreach (var field in validFields)
            {
                var property = entityType.GetProperty(field, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                if (property != null && property.CanRead)
                {
                    properties.Add(property);
                }
                else
                {
                    invalidFields.Add(field);
                }
            }
            if (invalidFields.Any())
            {
                throw new ArgumentException($"The following fields were not found in the entity {entityType.Name}: {string.Join(", ", invalidFields)}");
            }
            if (properties.Count == 0)
            {
                throw new ArgumentException($"No property");
            }
            ParameterExpression parameter = Expression.Parameter(entityType, "x");
            ConstructorInfo constructor = typeof(TResult).GetConstructor(Type.EmptyTypes);
            if (constructor == null)
            {
                ConstructorInfo[] constructors = typeof(TResult).GetConstructors();
                if (constructors.Length > 0)
                {
                    throw new ArgumentException($"Type {typeof(TResult).Name} does not have a parameterless constructor. Please ensure your result type has a default constructor or specify a different TResult type.");
                }
                else
                {
                    throw new ArgumentException($"Type {typeof(TResult).Name} does not have any public constructors.");
                }
            }
            PropertyInfo[] resultTypeProperties = typeof(TResult).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            List<MemberBinding> validBindings = new List<MemberBinding>();
            foreach (var property in properties)
            {
                PropertyInfo resultProperty = resultTypeProperties
                    .FirstOrDefault
                    (
                        p => p.Name.Equals(property.Name, StringComparison.OrdinalIgnoreCase) && p.PropertyType.IsAssignableFrom(property.PropertyType)
                    );
                if (resultProperty != null && resultProperty.CanWrite)
                {
                    validBindings.Add(Expression.Bind(resultProperty, Expression.Property(parameter, property)));
                }
            }
            if (validBindings.Count == 0)
            {
                throw new ArgumentException($"No compatible properties found between source type {entityType.Name} and result type {typeof(TResult).Name}. Make sure the property names and types match.");
            }
            NewExpression newExpression = Expression.New(constructor);
            MemberInitExpression body = Expression.MemberInit(newExpression, validBindings);
            return query.Select(Expression.Lambda<Func<T, TResult>>(body, parameter));
        }
    }
}
