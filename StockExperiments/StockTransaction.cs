﻿
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
        new (StockTransactionType.Arrival,
            quantities.Select(q => StockTransactionItem.CreateArrival(q.TaxStampTypeId, q.Quantity)),
            null,
            arrivalEventId);

    public static StockTransaction CreateDispatch(DispatchEventId dispatchEventId, TaxStampQuantitySet quantities) =>
        new(StockTransactionType.Dispatch,
            quantities.Select(q => StockTransactionItem.CreateDispatch(q.TaxStampTypeId, q.Quantity)),
            dispatchEventId,
            null);

    public StockTransaction CreateRevert() =>
        new(StockTransactionType.Revert, Items.Select(x => x.CreateRevert()), DispatchEventId, ArrivalEventId);
}
