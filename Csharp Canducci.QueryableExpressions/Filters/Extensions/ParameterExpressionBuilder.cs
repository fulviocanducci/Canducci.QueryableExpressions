using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Canducci.QueryableExpressions.Filters.Extensions
{
    internal static class ParameterExpressionBuilder
    {
        public static Expression CreateParameterExpression(object value, Type targetType)
        {
            if (value == null)
            {
                return Expression.Constant(null, targetType);
            }

            Type holderValueType = GetUnderlyingClrType(targetType);
            Type genericHolderType = typeof(ClosureHolder<>).MakeGenericType(holderValueType);

            // Convert the provided value to the holder's CLR type
            object converted = ConvertToTarget(value, holderValueType);

            // Create ClosureHolder<T> using the ctor that accepts T so the field is assigned in ctor (no reflection write)
            object holderInstance = Activator.CreateInstance(genericHolderType, new[] { converted });

            Expression constHolder = Expression.Constant(holderInstance, genericHolderType);

            // Access the strongly-typed Value field
            FieldInfo valueField = genericHolderType.GetField("Value", BindingFlags.Instance | BindingFlags.Public);
            Expression valueFieldExpr = Expression.Field(constHolder, valueField);

            // If the holder field type doesn't exactly match targetType (e.g., targetType is Nullable<T>), convert it
            if (valueFieldExpr.Type != targetType)
            {
                return Expression.Convert(valueFieldExpr, targetType);
            }

            return valueFieldExpr;
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

        // ClosureHolder<T> assigns Value in ctor so compiler recognizes the assignment (no CS0649).
        private sealed class ClosureHolder<T>
        {
            public T Value;

            public ClosureHolder(T value)
            {
                Value = value;
            }
        }
    }
}