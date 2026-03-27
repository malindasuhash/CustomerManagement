namespace ExternalAdapter.Extensions
{
    public static class IEnumerableExtension
    {
        public static void ForEach<T>(this IEnumerable<T> items, Action<T> action)
        {
            foreach (var item in items) {
                action(item);
            }
        }
    }
}
