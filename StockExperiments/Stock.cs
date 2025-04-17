

namespace StockExperiments;
public class Stock
{
    private readonly List<StockItem> _items = new();
    private readonly List<StockTransaction> _transactions = new();
    private readonly List<StockReservation> _reservations = new();

#pragma warning disable CS8618
    private Stock() { } // For EF
#pragma warning restore CS8618

    private Stock(StockId id, DistributionCenterKey distributionCenterKey)
    {
        Id = id;
        DistributionCenterKey = distributionCenterKey;
    }

    public StockId Id { get; private set; }
    public DistributionCenterKey DistributionCenterKey { get; private set; }
    public IReadOnlyCollection<StockItem> Items => _items;
    public IReadOnlyCollection<StockTransaction> Transactions => _transactions;
    public IReadOnlyCollection<StockReservation> Reservations => _reservations;
    public byte[] Version { get; private set; } = null!; // set by EF

    public static Stock Create(DistributionCenterKey distributionCenterKey)
    {
        return new Stock(new StockId(Guid.NewGuid()), distributionCenterKey);
    }

    public bool Reserve(WithdrawalRequestId withdrawalRequestId, TaxStampQuantitySet quantities)
    {
        if (!CanReserve(quantities))
        {
            return false;
        }

        _reservations.Add(StockReservation.Create(withdrawalRequestId, quantities));

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

    private void RevertLastTransaction(Func<StockTransaction, bool> condition)
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
}
