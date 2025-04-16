namespace StockExperiments;

public sealed class StockItem
{
    public StockItem(TaxStampTypeId taxStampTypeId)
    {
        TaxStampTypeId = taxStampTypeId;
        Quantity = new Quantity(0);
    }

    public TaxStampTypeId TaxStampTypeId { get; private set; }
    public Quantity Quantity { get; private set; }

    public void Apply(QuantityChange quantityChange)
    {
        if (!CanApply(quantityChange)) throw new InvalidOperationException();
        Quantity = new Quantity(Quantity + quantityChange);
    }

    public bool CanApply(QuantityChange quantityChange) =>
        Quantity >= -quantityChange;

    internal bool CanReserve(Quantity toReserveQuantity) =>
        Quantity >= toReserveQuantity;
}
