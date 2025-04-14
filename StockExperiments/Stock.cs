
namespace StockExperiments;

public class Stock
{
    private readonly List<StockItem> _quantities = new();
    private readonly List<StockTransaction> _transactions = new();

    private Stock(ScanningLocationId scanningLocationId)
    {
        ScanningLocationId = scanningLocationId;
    }

    public ScanningLocationId ScanningLocationId { get; private set; }

    public IReadOnlyCollection<StockItem> Quantities => _quantities;
    public IReadOnlyCollection<StockTransaction> Transactions => _transactions;

    public bool Reserve(ICollection<TaxStampQuantity> quantities)
    {
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
        _transactions.Add(new StockTransaction([new (arrival.TaxStampTypeId, arrival.Quantity)]));
    }

    public void Handle(DispatchEvent arrival)
    {
    }

    public static Stock Create(ScanningLocationId scanningLocationId)
    {
        return new Stock(scanningLocationId);
    }
}
