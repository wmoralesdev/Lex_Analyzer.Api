namespace Lex_Analyzer.Common.Extensions;

public static class CollectionExtensions
{
    public struct EnumeratedInstance<T>
    {
        public long Index;
        public T Item;

        public void Deconstruct(out long index, out T item)
        {
            index = Index;
            item = Item;
        }
    }

    public static IEnumerable<EnumeratedInstance<T>> Enumerate<T>(this IEnumerable<T> collection)
    {
        long counter = 0;
        foreach (var item in collection)
        {
            yield return new EnumeratedInstance<T>()
            {
                Index = counter,
                Item = item
            };
            counter++;
        }
    }
}