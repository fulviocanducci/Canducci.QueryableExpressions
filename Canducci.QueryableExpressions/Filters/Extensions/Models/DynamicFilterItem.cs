using Canducci.QueryableExpressions.Filters.Extensions.Operators;

namespace Canducci.QueryableExpressions.Filters.Extensions.Models
{
    public sealed class DynamicFilterItem
    {
        public string PropertyName { get; private set; }
        public object Value { get; private set; }
        public FilterOperator Operator { get; private set; }

        public DynamicFilterItem(string propertyName, object value, FilterOperator op = FilterOperator.Equal)
        {
            PropertyName = propertyName;
            Value = value;
            Operator = op;
        }

        public static DynamicFilterItem Create<T>(string propertyName, T value, FilterOperator op = FilterOperator.Equal)
        {
            return new DynamicFilterItem(propertyName, value, op);
        }

        public static DynamicFilterItem CreateContains<T>(string propertyName, T value)
        {
            return new DynamicFilterItem(propertyName, value, FilterOperator.Contains);
        }

        public static DynamicFilterItem CreateStartsWith<T>(string propertyName, T value)
        {
            return new DynamicFilterItem(propertyName, value, FilterOperator.StartsWith);
        }

        public static DynamicFilterItem CreateEndsWith<T>(string propertyName, T value)
        {
            return new DynamicFilterItem(propertyName, value, FilterOperator.EndsWith);
        }

        public static DynamicFilterItem CreateEqual<T>(string propertyName, T value)
        {
            return new DynamicFilterItem(propertyName, value, FilterOperator.Equal);
        }

        public static DynamicFilterItem CreateGreaterThan<T>(string propertyName, T value)
        {
            return new DynamicFilterItem(propertyName, value, FilterOperator.GreaterThan);
        }

        public static DynamicFilterItem CreateGreaterThanOrEqual<T>(string propertyName, T value)
        {
            return new DynamicFilterItem(propertyName, value, FilterOperator.GreaterThanOrEqual);
        }

        public static DynamicFilterItem CreateLessThan<T>(string propertyName, T value)
        {
            return new DynamicFilterItem(propertyName, value, FilterOperator.LessThan);
        }

        public static DynamicFilterItem CreateLessThanOrEqual<T>(string propertyName, T value)
        {
            return new DynamicFilterItem(propertyName, value, FilterOperator.LessThanOrEqual);
        }

        public static DynamicFilterItem CreateIsNull(string propertyName)
        {
            return new DynamicFilterItem(propertyName, null, FilterOperator.IsNull);
        }

        public static DynamicFilterItem CreateIsNotNull(string propertyName)
        {
            return new DynamicFilterItem(propertyName, null, FilterOperator.IsNotNull);
        }
    }
}