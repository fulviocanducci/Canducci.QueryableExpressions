using Canducci.QueryableExpressions.Filters.Extensions.Operators;
namespace Canducci.QueryableExpressions.Filters.Extensions.Bys
{
    public static class FilterBy
    {
        public static FilterOperator Contains => FilterOperator.Contains;
        public static FilterOperator StartsWith => FilterOperator.StartsWith;
        public static FilterOperator EndsWith => FilterOperator.EndsWith;
        public static FilterOperator Equal => FilterOperator.Equal;
        public static FilterOperator GreaterThan => FilterOperator.GreaterThan;
        public static FilterOperator GreaterThanOrEqual => FilterOperator.GreaterThanOrEqual;
        public static FilterOperator LessThan => FilterOperator.LessThan;
        public static FilterOperator LessThanOrEqual => FilterOperator.LessThanOrEqual;
        public static FilterOperator IsNull => FilterOperator.IsNull;
        public static FilterOperator IsNotNull => FilterOperator.IsNotNull;
    }
}
