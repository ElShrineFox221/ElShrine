namespace ElShrine.Common.Interface
{
    public static class IEListContainerHelper
    {
        public static void Add<T>(this IEListContainer<T> list, T item) => list.Items.Add(item);
        public static void AddRange<T>(this IEListContainer<T> list, T[] items) => list.Items.AddRange(items);

        public static void Reverse<T>(this IEListContainer<T> list) => list.Reverse();
        public static void Clear<T>(this IEListContainer<T> list) => list.Clear();

        public static void Remove<T>(this IEListContainer<T> list, T item) => list.Items.Remove(item);
        public static void RemoveAt<T>(this IEListContainer<T> list, int index) => list.Items.RemoveAt(index);

        public static void IndexOf<T>(this IEListContainer<T> list, T item) => list.Items.IndexOf(item);
    }
}
