namespace Canducci.QueryableExpressions.Filters.Extensions
{
    public static class SearchBy
    {
        public static SearchComparisonMode Contains => SearchComparisonMode.Contains;
        public static SearchComparisonMode StartsWith => SearchComparisonMode.StartsWith;
        public static SearchComparisonMode EndsWith => SearchComparisonMode.EndsWith;
        public static SearchComparisonMode Exactly => SearchComparisonMode.Exactly;
    }
}
