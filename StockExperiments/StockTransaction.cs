namespace StockExperiments;

public class StockTransaction
{
    private readonly List<StockTransactionItem> _items;

    private StockTransaction(StockTransactionType type, List<StockTransactionItem> items, WithdrawalRequestId? withdrawalRequestId)
    {
        _items = items;
        Type = type;
        WithdrawalRequestId = withdrawalRequestId;
    }

    public StockTransactionType Type { get; private set; }
    public IReadOnlyCollection<StockTransactionItem> Items => _items;

    public WithdrawalRequestId? WithdrawalRequestId { get; private set; }

    public static StockTransaction CreateArrival(TaxStampQuantitySet quantities) =>
        new(StockTransactionType.Arrival,
            quantities.Select(x => new StockTransactionItem(x.TaxStampTypeId, QuantityChange.PositiveChange(x.Quantity))).ToList(),
            null);

    public static StockTransaction CreateDispatch(WithdrawalRequestId withdrawalRequestId, TaxStampQuantitySet quantities) =>
        new(StockTransactionType.Dispatch,
            quantities.Select(x => new StockTransactionItem(x.TaxStampTypeId, QuantityChange.NegativeChange(x.Quantity))).ToList(),
            withdrawalRequestId);
}
