
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

    public bool Withdraw(WithdrawalRequestId withdrawalRequestId, ICollection<TaxStampQuantity> quantities)
    {
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
        return true;
    }

    public void Handle(ArrivalEvent arrival)
    {
        var item = _quantities.FirstOrDefault(x => x.TaxStampTypeId == arrival.TaxStampTypeId);
        if (item is null)
        {
            item = new StockItem(arrival.TaxStampTypeId);
            _quantities.Add(item);
        }
        item.Add(arrival.Quantity);
        _transactions.Add(StockTransaction.CreateArrival([new (arrival.TaxStampTypeId, arrival.Quantity)]));
    }

    public void Handle(DispatchEvent arrival)
    {
    }

    public static Stock Create(ScanningLocationId scanningLocationId)
    {
        return new Stock(scanningLocationId);
    }
}
