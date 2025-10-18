using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Canducci.QueryableExpressions.Filters.Extensions
{
    internal static class ParameterExpressionBuilder
    {
        public static Expression CreateParameterExpression(object value, Type targetType)
        {
            if (value == null) return Expression.Constant(null, targetType);

            Type holderValueType = GetUnderlyingClrType(targetType);
            Type genericHolderType = typeof(ClosureHolder<>).MakeGenericType(holderValueType);
            object holderInstance = Activator.CreateInstance(genericHolderType);

            var valueField = genericHolderType.GetField("Value", BindingFlags.Instance | BindingFlags.Public);
            if (valueField != null)
            {
                object converted = ConvertToTarget(value, holderValueType);
                valueField.SetValue(holderInstance, converted);
                Expression constHolder = Expression.Constant(holderInstance, genericHolderType);
                Expression valueExpr = Expression.Field(constHolder, valueField);
                return valueExpr.Type != targetType ? Expression.Convert(valueExpr, targetType) : valueExpr;
            }

            var valueProp = genericHolderType.GetProperty("Value", BindingFlags.Instance | BindingFlags.Public);
            if (valueProp != null)
            {
                object converted = ConvertToTarget(value, holderValueType);
                valueProp.SetValue(holderInstance, converted);
                Expression constHolder = Expression.Constant(holderInstance, genericHolderType);
                Expression valueExpr = Expression.Property(constHolder, valueProp);
                return valueExpr.Type != targetType ? Expression.Convert(valueExpr, targetType) : valueExpr;
            }

            throw new InvalidOperationException("ClosureHolder<T> must expose a public field 'Value' or property 'Value'.");
        }

        private static Type GetUnderlyingClrType(Type targetType)
        {
            return Nullable.GetUnderlyingType(targetType) ?? targetType;
        }

        private static object ConvertToTarget(object value, Type targetClrType)
        {
            if (value == null) return null;

            Type nonNullable = Nullable.GetUnderlyingType(targetClrType) ?? targetClrType;

            if (nonNullable.IsInstanceOfType(value))
            {
                return value;
            }

            if (nonNullable.IsEnum)
            {
                if (value is string s) return Enum.Parse(nonNullable, s);
                return Enum.ToObject(nonNullable, value);
            }

            return Convert.ChangeType(value, nonNullable);
        }

        private sealed class ClosureHolder<T>
        {
            public T Value { get; set; }
        }
    }
}