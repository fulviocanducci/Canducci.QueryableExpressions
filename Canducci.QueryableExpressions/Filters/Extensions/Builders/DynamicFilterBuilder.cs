using Canducci.QueryableExpressions.Filters.Extensions.Models;
using Canducci.QueryableExpressions.Filters.Extensions.Operators;
using System.Collections.Generic;
namespace Canducci.QueryableExpressions.Filters.Extensions.Builders
{
    public sealed class DynamicFilterBuilder
    {
        internal List<DynamicFilterItem> Filters { get; set; }

        public DynamicFilterBuilder()
        {
           Filters = new List<DynamicFilterItem>();
        }

        public DynamicFilterBuilder Add(DynamicFilterItem item)
        {
            Filters.Add(item);
            return this;
        }

        public DynamicFilterBuilder Add<T>(string propertyName, T value, FilterOperator op)
        {
            return Add(DynamicFilterItem.Create(propertyName, value, op));
        }

        public DynamicFilterBuilder AddEqual<T>(string propertyName, T value)
        {
            return Add(DynamicFilterItem.CreateEqual(propertyName, value));
        }

        public DynamicFilterBuilder AddContains<T>(string propertyName, T value)
        {
            return Add(DynamicFilterItem.CreateContains(propertyName, value));
        }

        public DynamicFilterBuilder AddStartsWith<T>(string propertyName, T value)
        {
            return Add(DynamicFilterItem.CreateStartsWith(propertyName, value));
        }

        public DynamicFilterBuilder AddEndsWith<T>(string propertyName, T value)
        {
            return Add(DynamicFilterItem.CreateEndsWith(propertyName, value));
        }

        public DynamicFilterBuilder AddGreaterThan<T>(string propertyName, T value)
        {
            return Add(DynamicFilterItem.CreateGreaterThan(propertyName, value));
        }

        public DynamicFilterBuilder AddGreaterThanOrEqual<T>(string propertyName, T value)
        {
            return Add(DynamicFilterItem.CreateGreaterThanOrEqual(propertyName, value));
        }

        public DynamicFilterBuilder AddLessThan<T>(string propertyName, T value)
        {
            return Add(DynamicFilterItem.CreateLessThan(propertyName, value));
        }

        public DynamicFilterBuilder AddLessThanOrEqual<T>(string propertyName, T value)
        {
            return Add(DynamicFilterItem.CreateLessThanOrEqual(propertyName, value));
        }

        public DynamicFilterBuilder AddNull(string propertyName)
        {
            return Add(DynamicFilterItem.CreateIsNull(propertyName));
        }

        public DynamicFilterBuilder AddNotNull(string propertyName)
        {
            return Add(DynamicFilterItem.CreateIsNotNull(propertyName));
        }

        public void Clear()
        {
            Filters.Clear();
        }

        public List<DynamicFilterItem> Build()
        {
            return Filters;
        }

        public static DynamicFilterBuilder Create()
        {
            return new DynamicFilterBuilder();
        }
    }
}
