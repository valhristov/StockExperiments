
namespace StockExperiments;
public class Stock
{
    private readonly List<StockItem> _available = new();
    private readonly List<ReservationItem> _reserved = new();
    private readonly List<StockTransaction> _transactions = new();
    private readonly List<StockReservation> _reservations = new();

    private Stock(ScanningLocationId scanningLocationId)
    {
        ScanningLocationId = scanningLocationId;
    }

    public ScanningLocationId ScanningLocationId { get; private set; }

    public IReadOnlyCollection<StockItem> Available => _available;
    public IReadOnlyCollection<ReservationItem> Reserved => _reserved;
    public IReadOnlyCollection<StockTransaction> Transactions => _transactions;
    public IReadOnlyCollection<StockReservation> Reservations => _reservations;

    public bool BeginDispatch(WithdrawalRequestId withdrawalRequestId, TaxStampQuantitySet quantities)
    {
        var toReserve = quantities.GroupJoin(
            _available,
            r => r.TaxStampTypeId,
            a => a.TaxStampTypeId,
            (r, a) => (r.Quantity, AvailableItem: a.SingleOrDefault()));
        
        if (toReserve.Any(x => x.AvailableItem is null || !x.AvailableItem.CanSubtract(x.Quantity)))
        {
            return false;
        }

        _reserved.AddRange(quantities
            .Where(ti => !_reserved.Any(si => ti.TaxStampTypeId == si.TaxStampTypeId))
            .Select(ti => new ReservationItem(ti.TaxStampTypeId)));

        var reserved = quantities.GroupJoin(
            _reserved,
            r => r.TaxStampTypeId,
            x => x.TaxStampTypeId,
            (r, x) => (r.Quantity, ReservedItem: x.Single()));

        foreach (var item in reserved)
        {
            item.ReservedItem.Add(item.Quantity);
        }

        _reservations.Add(StockReservation.Create(withdrawalRequestId, quantities));

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
            _available.AddRange(transaction.Items
                .Where(ti => !_available.Any(si => ti.TaxStampTypeId == si.TaxStampTypeId))
                .Select(ti => new StockItem(ti.TaxStampTypeId)));
        }

        var toChange = transaction.Items
            .GroupJoin(Available,
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

        if (!reservation.Release(dispatch.Quantities))
        {
            return false;
        }

        var itemsToRelease = dispatch.Quantities.GroupJoin(_reserved,
            q => q.TaxStampTypeId,
            r => r.TaxStampTypeId,
            (q, r) => (q.Quantity, ExistingItem: r.SingleOrDefault()));

        foreach (var item in itemsToRelease.Where(x => x.ExistingItem != null))
        {
            item.ExistingItem!.Release(item.Quantity);
        }

        _transactions.Add(transaction);
        return true;
    }

    public static Stock Create(ScanningLocationId scanningLocationId)
    {
        return new Stock(scanningLocationId);
    }
}
