

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
        if (!CanReserve(quantitiesToReserve))
        {
            return false;
        }

        _reservations.Add(StockReservation.Create(withdrawalRequestId, quantitiesToReserve));

        return true;

        bool CanReserve(TaxStampQuantitySet quantities)
        {
            var affectedItems = quantities
                .GroupJoin(_items,
                    r => r.TaxStampTypeId,
                    a => a.TaxStampTypeId,
                    (r, a) => (r.TaxStampTypeId, r.Quantity, Item: a.SingleOrDefault()))
                .GroupJoin(_reservations.Where(x => x.IsActive).SelectMany(x => x.RemainingItems),
                    x => x.TaxStampTypeId,
                    r => r.TaxStampTypeId,
                    (x, reservationItems) =>
                        (
                            StockItem: x.Item,
                            QuantityToReserve: x.Quantity,
                            ReservedQuantity: new Quantity(reservationItems.Select(x => x.Quantity.Value).Sum())
                        ));

            return affectedItems.All(x =>
                x.StockItem != null && x.StockItem.CanReserve(x.ReservedQuantity + x.QuantityToReserve));
        }
    }

    public bool Handle(ArrivalEvent arrival)
    {
        RevertLastTransaction(x => x.ArrivalEventId == arrival.ArrivalEventId);

        var transaction = StockTransaction.CreateArrival(
            arrival.ArrivalEventId,
            arrival.Quantities);

        _items.AddRange(GetNotExistingTaxStampTypeIds(arrival.Quantities).Select(x => new StockItem(x)));

        if (!Apply(transaction))
        {
            return false;
        }

        _transactions.Add(transaction);
        return true;
    }

    public bool Handle(DispatchEvent dispatch)
    {
        RevertLastTransaction(x => x.DispatchEventId == dispatch.DispatchEventId);

        var transaction = StockTransaction.CreateDispatch(dispatch.DispatchEventId, dispatch.Quantities);

        if (!Apply(transaction))
        {
            return false;
        }

        _transactions.Add(transaction);

        var reservation = _reservations.SingleOrDefault(x => x.WithdrawalRequestId == dispatch.WithdrawalRequestId);
        reservation?.Release(dispatch.Quantities);

        return true;
    }

    void RevertLastTransaction(Func<StockTransaction, bool> condition)
    {
        var revertTransaction = Transactions
            .Where(x => x.Type != StockTransactionType.Revert)
            .LastOrDefault(condition)
            ?.CreateRevert();

        if (revertTransaction != null)
        {
            Apply(revertTransaction);

            _transactions.Add(revertTransaction);
        }
    }

    private IEnumerable<TaxStampTypeId> GetNotExistingTaxStampTypeIds(IReadOnlyCollection<TaxStampQuantity> quantities) =>
        quantities
            .Where(x => !_items.Any(si => x.TaxStampTypeId == si.TaxStampTypeId))
            .Select(x => x.TaxStampTypeId);

    private bool Apply(StockTransaction transaction)
    {
        var affectedItems = transaction.Items
            .GroupJoin(Items,
                ti => ti.TaxStampTypeId,
                si => si.TaxStampTypeId,
                (ti, si) => (ti.QuantityChange, StockItem: si.SingleOrDefault()))
            .ToList();

        if (!affectedItems.All(x => x.StockItem != null && x.StockItem.CanApply(x.QuantityChange)))
        {
            return false;
        }

        foreach (var item in affectedItems)
        {
            item.StockItem!.Apply(item.QuantityChange);
        }

        return true;
    }

    public static Stock Create(ScanningLocationId scanningLocationId)
    {
        return new Stock(scanningLocationId);
    }
}
