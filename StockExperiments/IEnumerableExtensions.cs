namespace StockExperiments;
/// <summary>
/// Taken from https://stackoverflow.com/a/13503860/7156760
/// </summary>
public static class EnumerableExtensions
{
    internal static IEnumerable<TResult> FullOuterJoin<TA, TB, TKey, TResult>(
        this IEnumerable<TA> left,
        IEnumerable<TB> right,
        Func<TA, TKey> selectLeftKey,
        Func<TB, TKey> selectRightKey,
        Func<TA, TB, TKey, TResult> projection,
        IEqualityComparer<TKey>? cmp = null)
    {
        cmp = cmp ?? EqualityComparer<TKey>.Default;
        var alookup = left.ToLookup(selectLeftKey, cmp);
        var blookup = right.ToLookup(selectRightKey, cmp);

        var keys = new HashSet<TKey>(alookup.Select(p => p.Key), cmp);
        keys.UnionWith(blookup.Select(p => p.Key));

        var join = from key in keys
                   from xa in alookup[key].DefaultIfEmpty()
                   from xb in blookup[key].DefaultIfEmpty()
                   select projection(xa, xb, key);

        return join;
    }

    internal static IEnumerable<TResult> FullOuterGroupJoin<TA, TB, TKey, TResult>(
        this IEnumerable<TA> left,
        IEnumerable<TB> right,
        Func<TA, TKey> selectLeftKey,
        Func<TB, TKey> selectRightKey,
        Func<IEnumerable<TA>, IEnumerable<TB>, TKey, TResult> projection,
        IEqualityComparer<TKey>? cmp = null)
    {
        cmp = cmp ?? EqualityComparer<TKey>.Default;
        var alookup = left.ToLookup(selectLeftKey, cmp);
        var blookup = right.ToLookup(selectRightKey, cmp);

        var keys = new HashSet<TKey>(alookup.Select(p => p.Key), cmp);
        keys.UnionWith(blookup.Select(p => p.Key));

        var join = from key in keys
                   let xa = alookup[key]
                   let xb = blookup[key]
                   select projection(xa, xb, key);

        return join;
    }

}