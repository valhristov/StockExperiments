namespace StockExperiments;
public class Stock
{
    private readonly List<StockItem> _quantities = new();
    private readonly List<StockTransaction> _transactions = new();
    private readonly List<StockReservation> _reservations = new();

    private Stock(ScanningLocationId scanningLocationId)
    {
        ScanningLocationId = scanningLocationId;
    }

    public ScanningLocationId ScanningLocationId { get; private set; }

    public IReadOnlyCollection<StockItem> Quantities => _quantities;
    public IReadOnlyCollection<StockTransaction> Transactions => _transactions;
    public IReadOnlyCollection<StockReservation> Reservations => _reservations;

    public bool Withdraw(WithdrawalRequestId withdrawalRequestId, TaxStampQuantitySet quantities)
    {
        if (quantities.Any(x => x.Quantity.Value <= 0))
        {
            return false;
        }
        var itemsToChange = quantities
            .GroupJoin(_quantities,
                w => w.TaxStampTypeId,
                q => q.TaxStampTypeId,
                (w, q) => new { QuantityToWithdraw = w, Item = q.SingleOrDefault() });
        if (itemsToChange.Any(x => x.Item is null))
        {
            return false;
        }
        if (itemsToChange.Any(x => x.Item!.Quantity.Value < x.QuantityToWithdraw.Quantity.Value))
        {
            return false;
        }
        foreach (var item in itemsToChange)
        {
            item.Item!.Subtract(item.QuantityToWithdraw.Quantity);
        }
        _transactions.Add(StockTransaction.CreateWithdrawal(withdrawalRequestId, new (itemsToChange.Select(x => x.QuantityToWithdraw))));
        return true;
    }

    public void Handle(ArrivalEvent arrival)
    {
        foreach (var quantity in arrival.Quantities)
        {
            var existing = _quantities.FirstOrDefault(x => x.TaxStampTypeId == quantity.TaxStampTypeId);
            if (existing is null)
            {
                existing = new StockItem(quantity.TaxStampTypeId);
                _quantities.Add(existing);
            }
            existing.Add(quantity.Quantity); 
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
