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
        var itemsToChange = quantities
            .GroupJoin(_items,
                w => w.TaxStampTypeId,
                q => q.TaxStampTypeId,
                (w, q) => new { ToDispatch = w, Item = q.SingleOrDefault() });
        if (itemsToChange.Any(x => x.Item is null))
        {
            return false;
        }
        if (itemsToChange.Any(x => x.Item!.Quantity < x.ToDispatch.Quantity))
        {
            return false;
        }
        foreach (var item in itemsToChange)
        {
            item.Item!.Apply(QuantityChange.NegativeChange(item.ToDispatch.Quantity));
        }
        _transactions.Add(StockTransaction.CreateDispatch(withdrawalRequestId, new (itemsToChange.Select(x => x.ToDispatch))));
        return true;
    }

    public void Handle(ArrivalEvent arrival)
    {
        foreach (var arrivedTaxStampType in arrival.Quantities)
        {
            var existing = _items.FirstOrDefault(x => x.TaxStampTypeId == arrivedTaxStampType.TaxStampTypeId);
            if (existing is null)
            {
                existing = new StockItem(arrivedTaxStampType.TaxStampTypeId);
                _items.Add(existing);
            }
            existing.Apply(QuantityChange.PositiveChange(arrivedTaxStampType.Quantity)); 
        }
        _transactions.Add(StockTransaction.CreateArrival(arrival.Quantities));
    }

    public void Handle(DispatchEvent arrival)
    {
    }

    public static Stock Create(ScanningLocationId scanningLocationId)
    {
        return new Stock(scanningLocationId);
    }
}
