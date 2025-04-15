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
        Quantity = new Quantity(Quantity.Value + quantityChange.Value);
    }

    public bool CanApply(QuantityChange quantityChange) =>
        Quantity.Value + quantityChange.Value >= 0;

    internal bool CanReserve(Quantity toReserveQuantity, Quantity reservedQuantity)
    {
        throw new NotImplementedException();
    }
}
