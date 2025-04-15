
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

    public bool Reserve(WithdrawalRequestId withdrawalRequestId, TaxStampQuantitySet quantitiesToReserve)
    {
        var affectedItems = quantitiesToReserve
            .GroupJoin(_items,
                r => r.TaxStampTypeId,
                a => a.TaxStampTypeId,
                (r, a) => (r.TaxStampTypeId, r.Quantity, Item: a.SingleOrDefault()))
            .GroupJoin(_reservations.SelectMany(x => x.RemainingItems),
                x => x.TaxStampTypeId,
                r => r.TaxStampTypeId,
                (x, r) => (x.Item, QuantityToReserve: x.Quantity, ReservedQuantity: new Quantity(r.Select(x => x.Quantity.Value).Sum())));
        
        if (affectedItems.Any(x => x.Item is null
            || !x.Item.CanApply(QuantityChange.NegativeChange(x.ReservedQuantity + x.QuantityToReserve))))
        {
            return false;
        }

        _reservations.Add(StockReservation.Create(withdrawalRequestId, quantitiesToReserve));

        return true;
    }

    public bool Handle(ArrivalEvent arrival)
    {
        var transaction = StockTransaction.CreateArrival(arrival.Quantities);

        // add stock items for missing types
        _items.AddRange(transaction.Items
            .Where(ti => !_items.Any(si => ti.TaxStampTypeId == si.TaxStampTypeId))
            .Select(ti => new StockItem(ti.TaxStampTypeId)));

        if (!Apply(transaction))
        {
            return false;
        }

        _transactions.Add(transaction);
        return true;
    }

    private bool Apply(StockTransaction transaction)
    {
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

    public bool Handle(DispatchEvent dispatch)
    {
        var reservation = _reservations.FirstOrDefault(x => x.WithdrawalRequestId == dispatch.WithdrawalRequestId);
        if (reservation is null)
        {
            return false;
        }

        var transaction = StockTransaction.CreateDispatch(dispatch.WithdrawalRequestId, dispatch.Quantities);

        if (!Apply(transaction))
        {
            return false;
        }

        reservation.Release(dispatch.Quantities);
        _transactions.Add(transaction);
        return true;
    }

    public static Stock Create(ScanningLocationId scanningLocationId)
    {
        return new Stock(scanningLocationId);
    }
}
