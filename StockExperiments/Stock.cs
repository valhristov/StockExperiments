

namespace StockExperiments;
public class Stock
{
    private readonly List<StockItem> _items = new();
    private readonly List<StockTransaction> _transactions = new();
    private readonly List<StockReservation> _reservations = new();

    private Stock(ScanningLocationId scanningLocationId)
    {
        ScanningLocationId = scanningLocationId;
    }

    public ScanningLocationId ScanningLocationId { get; private set; }

    public IReadOnlyCollection<StockItem> Items => _items;
    public IReadOnlyCollection<StockTransaction> Transactions => _transactions;
    public IReadOnlyCollection<StockReservation> Reservations => _reservations;

    public bool Dispatch(WithdrawalRequestId withdrawalRequestId, TaxStampQuantitySet quantities)
    {
        var transaction = StockTransaction.CreateDispatch(withdrawalRequestId, quantities);

        if (!Apply(transaction))
        {
            return false;
        }

        _transactions.Add(transaction);
        return true;
    }

    public bool Handle(ArrivalEvent arrival)
    {
        var transaction = StockTransaction.CreateArrival(arrival.Quantities);

        if (!Apply(transaction))
        {
            return false;
        }

        _transactions.Add(transaction);
        return true;
    }

    private bool Apply(StockTransaction transaction)
    {
        if (transaction.Type == StockTransactionType.Arrival)
        {
            _items.AddRange(transaction.Items
                .Where(ti => !_items.Any(si => ti.TaxStampTypeId == si.TaxStampTypeId))
                .Select(ti => new StockItem(ti.TaxStampTypeId)));
        }

        var toChange = transaction.Items
            .GroupJoin(Items,
                ti => ti.TaxStampTypeId,
                si => si.TaxStampTypeId,
                (ti, si) => (ti.QuantityChange, StockItem: si.SingleOrDefault()))
            .ToList();

        if (toChange.Any(x => x.StockItem is null || !x.StockItem.CanApply(x.QuantityChange)))
        {
            return false;
        }

        foreach (var item in toChange)
        {
            item.StockItem!.Apply(item.QuantityChange);
        }

        return true;
    }

    public void Handle(DispatchEvent arrival)
    {
    }

    public static Stock Create(ScanningLocationId scanningLocationId)
    {
        return new Stock(scanningLocationId);
    }
}
