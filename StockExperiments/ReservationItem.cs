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

    public void Release(Quantity quantity)
    {
        // TODO: should we allow releasing more than reserved?
        // ArgumentOutOfRangeException.ThrowIfGreaterThan(quantity.Value, Quantity, nameof(quantity));
        Quantity = new(Math.Max(0, quantity.Value - quantity));
    }
}
