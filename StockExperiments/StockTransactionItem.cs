namespace StockExperiments;

public class StockTransactionItem
{
    private StockTransactionItem(TaxStampTypeId taxStampTypeId, QuantityChange quantityChange)
    {
        TaxStampTypeId = taxStampTypeId;
        QuantityChange = quantityChange;
    }

    public TaxStampTypeId TaxStampTypeId { get; private set; }
    public QuantityChange QuantityChange { get; private set; }

    public static StockTransactionItem CreateArrival(TaxStampQuantity quantity) =>
        new(quantity.TaxStampTypeId, QuantityChange.PositiveChange(quantity.Quantity));

    public static StockTransactionItem CreateDispatch(TaxStampQuantity quantity) =>
        new(quantity.TaxStampTypeId, QuantityChange.NegativeChange(quantity.Quantity));
}