namespace Canducci.QueryableExpressions.Filters.Extensions.Internals
{
    internal sealed class ClosureHolder<T>
    {
        public T Value { get; set; }
        public ClosureHolder() { }
        public ClosureHolder(T value)
        {
            Value = value;
        }
    }
}