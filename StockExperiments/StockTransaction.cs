namespace StockExperiments;

public class StockTransaction
{
    private readonly List<TaxStampQuantity> _quantities;

    private StockTransaction(WithdrawalRequestId? withdrawalRequestId, IEnumerable<TaxStampQuantity> quantities)
    {
        _quantities = quantities.ToList();

        WithdrawalRequestId = withdrawalRequestId;
    }

    public IReadOnlyCollection<TaxStampQuantity> Quantities => _quantities;

    public WithdrawalRequestId? WithdrawalRequestId { get; private set; }

    public static StockTransaction CreateArrival(IEnumerable<TaxStampQuantity> quantities) =>
        new(null, quantities);

    public static StockTransaction CreateWithdrawal(WithdrawalRequestId withdrawalRequestId, IEnumerable<TaxStampQuantity> quantities) =>
        new(withdrawalRequestId, quantities);
}