

namespace StockExperiments;

public class StockTransaction
{
    private readonly List<StockTransactionItem> _items;

    private StockTransaction(StockTransactionType type, IEnumerable<StockTransactionItem> items,
        DispatchEventId? dispatchEventId,
        ArrivalEventId? arrivalEventId)
    {
        _items = items.ToList();
        Type = type;
        DispatchEventId = dispatchEventId;
        ArrivalEventId = arrivalEventId;
    }

    public StockTransactionType Type { get; private set; }
    public IReadOnlyCollection<StockTransactionItem> Items => _items;

    public DispatchEventId? DispatchEventId { get; private set; }
    public ArrivalEventId? ArrivalEventId { get; private set; }

    public static StockTransaction CreateArrival(ArrivalEventId arrivalEventId, TaxStampQuantitySet quantities) =>
        new(StockTransactionType.Arrival,
            quantities.Select(StockTransactionItem.CreateArrival),
            null,
            arrivalEventId);

    public static StockTransaction CreateArrivalCorrection(ArrivalEventId arrivalEventId, 
        IEnumerable<StockTransactionItem> existingTransactionItems, TaxStampQuantitySet quantities)
    {
        var correctedQuantities = quantities.FullOuterGroupJoin(existingTransactionItems,
            q => q.TaxStampTypeId,
            i => i.TaxStampTypeId,
            (q, i, taxStampTypeId) =>
            (
                NewQuantity: q.SingleOrDefault()?.Quantity,
                ExistingChange: i.Any() ? new QuantityChange(i.Select(x => x.QuantityChange.Value).Sum()) : null,
                TaxStampTypeId: taxStampTypeId
            ));

        return new StockTransaction(StockTransactionType.Arrival,
            correctedQuantities
                .Where(x => QuantityChanged(x.NewQuantity, x.ExistingChange))
                .Select(x => StockTransactionItem.CreateArrivalCorrection(x.TaxStampTypeId, x.ExistingChange, x.NewQuantity)),
            null,
            arrivalEventId);

        static bool QuantityChanged(Quantity? newQuantity, QuantityChange? existingChange) =>
            newQuantity is null ||
            existingChange is null ||
            QuantityChange.PositiveChange(newQuantity) != existingChange;
    }

    public static StockTransaction CreateDispatch(DispatchEventId dispatchEventId, TaxStampQuantitySet quantities) =>
        new(StockTransactionType.Dispatch,
            quantities.Select(StockTransactionItem.CreateDispatch),
            dispatchEventId,
            null);
}
