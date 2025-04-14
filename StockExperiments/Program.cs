
namespace StockExperiments;

internal class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("Hello, World!");
    }
}


public class DeliveryNote
{
}

public class Stock
{
    private readonly List<StockItem> _items = new();

    private Stock(ScanningLocationId scanningLocationId)
    {
        ScanningLocationId = scanningLocationId;
    }

    public ScanningLocationId ScanningLocationId { get; private set; }

    public IReadOnlyCollection<StockItem> Items => _items;

    public bool Reserve(ICollection<TaxStampQuantity> quantities)
    {
        return true;
    }

    public void Handle(ArrivalEvent arrival)
    {
        var item = _items.FirstOrDefault(x => x.TaxStampTypeId == arrival.TaxStampTypeId);
        if (item is null)
        {
            item = new StockItem(arrival.TaxStampTypeId);
            _items.Add(item);
        }
        item.Add(arrival.Quantity);
    }

    public void Handle(DispatchEvent arrival)
    {
    }

    public static Stock Create(ScanningLocationId scanningLocationId)
    {
        return new Stock(scanningLocationId);
    }
}

public sealed class StockItem
{
    public StockItem(TaxStampTypeId taxStampTypeId)
    {
        TaxStampTypeId = taxStampTypeId;
        Quantity = new Quantity(0);
    }

    public TaxStampTypeId TaxStampTypeId { get; private set; }
    public Quantity Quantity { get; private set; }

    public void Add(Quantity quantity)
    {
        Quantity = new Quantity(Quantity.Value + quantity.Value);
    }
}
