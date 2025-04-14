
namespace StockExperiments;

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
