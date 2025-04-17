namespace StockExperiments;

public sealed class StockItem
{
#pragma warning disable CS0169
    private int _id;
#pragma warning restore CS0169
#pragma warning disable CS8618
    private StockItem() { } // required by EF
#pragma warning restore CS8618

    internal StockItem(TaxStampTypeId taxStampTypeId)
    {
        TaxStampTypeId = taxStampTypeId;
        Quantity = Quantity.Zero;
    }

    public TaxStampTypeId TaxStampTypeId { get; private set; }
    public Quantity Quantity { get; private set; }
    public byte[] Version { get; private set; } = null!; // set by EF

    internal void Apply(QuantityChange quantityChange)
    {
        if (!CanApply(quantityChange)) throw new InvalidOperationException();
        Quantity = new Quantity(Quantity + quantityChange);
    }

    internal bool CanApply(QuantityChange quantityChange) =>
        Quantity >= -quantityChange;

    internal bool CanReserve(Quantity toReserveQuantity) =>
        Quantity >= toReserveQuantity;
}
