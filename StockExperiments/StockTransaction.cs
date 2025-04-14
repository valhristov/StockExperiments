namespace StockExperiments;

public class StockTransaction
{
    private readonly List<TaxStampQuantity> _quantities;

    public StockTransaction(IEnumerable<TaxStampQuantity> quantities)
    {
        _quantities = quantities.ToList();
    }

    public IReadOnlyCollection<TaxStampQuantity> Quantities => _quantities;
}