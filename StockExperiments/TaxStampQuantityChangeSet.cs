using System.Runtime.CompilerServices;

namespace StockExperiments;

[CollectionBuilder(typeof(TaxStampQuantitySetBuilder), nameof(TaxStampQuantitySetBuilder.Create))]
public sealed class TaxStampQuantitySet : NonEmptyReadOnlySet<TaxStampQuantity, TaxStampTypeId>
{
    public TaxStampQuantitySet(params TaxStampQuantity[] items) : base(items, x => x.TaxStampTypeId)
    {
    }

    public TaxStampQuantitySet(IEnumerable<TaxStampQuantity> items) : base(items, x => x.TaxStampTypeId)
    {
    }

    public static class TaxStampQuantitySetBuilder // support for collection expressions
    {
        public static TaxStampQuantitySet Create(ReadOnlySpan<TaxStampQuantity> items) => new(items.ToArray());
    }
}
