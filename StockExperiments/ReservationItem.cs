namespace StockExperiments;

public sealed class ReservationItem
{
    public ReservationItem(TaxStampTypeId taxStampTypeId)
    {
        TaxStampTypeId = taxStampTypeId;
        Quantity = new Quantity(0);
    }

    public TaxStampTypeId TaxStampTypeId { get; private set; }
    public Quantity Quantity { get; private set; }

    public void Add(Quantity quantity)
    {
        Quantity = new(Quantity.Value + quantity);
    }

    public void Remove(Quantity quantity)
    {
        ArgumentOutOfRangeException.ThrowIfGreaterThan(quantity.Value, Quantity, nameof(quantity));
        Quantity = new(quantity.Value - quantity);
    }
}
