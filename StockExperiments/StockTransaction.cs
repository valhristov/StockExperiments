namespace StockExperiments;

public class StockTransaction
{
    private readonly List<TaxStampQuantity> _quantities;

    private StockTransaction(WithdrawalRequestId? withdrawalRequestId, TaxStampQuantitySet quantities)
    {
        _quantities = quantities.ToList();

        WithdrawalRequestId = withdrawalRequestId;
    }

    public IReadOnlyCollection<TaxStampQuantity> Quantities => _quantities;

    public WithdrawalRequestId? WithdrawalRequestId { get; private set; }

    public static StockTransaction CreateArrival(TaxStampQuantitySet quantities) =>
        new(null, quantities);

    public static StockTransaction CreateWithdrawal(WithdrawalRequestId withdrawalRequestId, TaxStampQuantitySet quantities) =>
        new(withdrawalRequestId, new (quantities.Select(x => new TaxStampQuantity(x.TaxStampTypeId, new(-x.Quantity)))));
}