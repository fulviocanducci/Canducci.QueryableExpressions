using Canducci.QueryableExpressions.Filters.Extensions.Operators;
namespace Canducci.QueryableExpressions.Filters.Extensions.Bys
{
    public static class SearchBy
    {
        public static SearchOperator Contains => SearchOperator.Contains;
        public static SearchOperator StartsWith => SearchOperator.StartsWith;
        public static SearchOperator EndsWith => SearchOperator.EndsWith;
        public static SearchOperator Exactly => SearchOperator.Exactly;
    }
}
