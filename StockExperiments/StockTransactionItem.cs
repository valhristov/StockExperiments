using System.Diagnostics;

namespace StockExperiments;

[DebuggerDisplay("\\{ TaxStampTypeId:{TaxStampTypeId} QuantityChange:{QuantityChange} \\}")]
public class StockTransactionItem
{
    private StockTransactionItem(TaxStampTypeId taxStampTypeId, QuantityChange quantityChange)
    {
        TaxStampTypeId = taxStampTypeId;
        QuantityChange = quantityChange;
    }

    public TaxStampTypeId TaxStampTypeId { get; private set; }
    public QuantityChange QuantityChange { get; private set; }

    public static StockTransactionItem CreateDispatch(TaxStampTypeId taxStampTypeId, Quantity newQuantity) =>
        new(taxStampTypeId, QuantityChange.NegativeChange(newQuantity));

    public static StockTransactionItem CreateArrival(TaxStampTypeId taxStampTypeId, Quantity newQuantity) =>
        new(taxStampTypeId, QuantityChange.PositiveChange(newQuantity));

    public StockTransactionItem CreateRevert() =>
        new(TaxStampTypeId, new QuantityChange(-QuantityChange));
}