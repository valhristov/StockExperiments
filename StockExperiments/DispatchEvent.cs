namespace StockExperiments;

public record DispatchEvent(
    DispatchEventId DispatchEventId,
    WithdrawalRequestId WithdrawalRequestId,
    TaxStampQuantitySet Quantities);
