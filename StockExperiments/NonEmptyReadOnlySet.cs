using System.Collections;
using System.Collections.Immutable;

namespace StockExperiments;

public abstract class NonEmptyReadOnlySet<T, TKey> : IReadOnlyCollection<T>
    where T : notnull
    where TKey : notnull
{
    private readonly ImmutableDictionary<TKey, T> _items;

    public int Count => _items.Count;

    public NonEmptyReadOnlySet(IEnumerable<T> items, Func<T, TKey> getKey)
    {
        if (!items.Any())
        {
            throw new ArgumentException("Collection must contain at least one item.", nameof(items));
        }

        var builder = ImmutableDictionary.CreateBuilder<TKey, T>();
        foreach (var item in items)
        {
            var key = getKey(item);
            if (builder.ContainsKey(key))
            {
                throw new ArgumentException("Collection must not contain duplicate items.", nameof(items));
            }
            builder.Add(getKey(item), item);
        }

        _items = builder.ToImmutable();
    }

    public IEnumerator<T> GetEnumerator() =>
        _items.Values.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() =>
        GetEnumerator();
}
