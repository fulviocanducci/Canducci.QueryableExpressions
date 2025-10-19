using System;
using System.Text;

namespace Canducci.QueryableExpressions.Orders.Models
{
    public sealed class Order
    {
        public string PropertyName { get; }
        public bool Descending { get; }
        public Order(string propertyName) : this(propertyName, OrderDirection.Ascending) { }
        public Order(string propertyName, OrderDirection orderDirection)
        {
            if (string.IsNullOrWhiteSpace(propertyName))
            {
                throw new ArgumentException("propertyName is required.", nameof(propertyName));
            }
            PropertyName = propertyName;
            Descending = orderDirection == OrderDirection.Descending;
        }

        public static Order By(string propertyName)
        {
            return new Order(propertyName, OrderDirection.Ascending);
        }
        public static Order ByDescending(string propertyName)
        {
            return new Order(propertyName, OrderDirection.Descending);
        }
    }
}