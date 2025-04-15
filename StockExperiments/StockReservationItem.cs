namespace StockExperiments;

public class StockReservationItem
{
    private StockReservationItem(TaxStampTypeId taxStampTypeId, Quantity quantity)
    {
        TaxStampTypeId = taxStampTypeId;
        Quantity = quantity;
    }

    public TaxStampTypeId TaxStampTypeId { get; private set; }
    public Quantity Quantity { get; private set; }

    public static StockReservationItem Create(TaxStampQuantity quantity) =>
        new(quantity.TaxStampTypeId, quantity.Quantity);

    public void Release(Quantity quantity) =>
        // be permissive
        Quantity = new(Math.Max(0, Quantity.Value - quantity));
}