namespace StockExperiments;

public sealed class WithdrawalRequestId(Guid value) : SimpleValueObject<Guid, WithdrawalRequestId>(value)
{
}
