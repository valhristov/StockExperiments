namespace StockExperiments;

public sealed class QuantityChange : SimpleValueObject<int, QuantityChange>
{
    public QuantityChange(int value) : base(value)
    {
        ArgumentOutOfRangeException.ThrowIfZero(value, nameof(value));
    }

    public static QuantityChange NegativeChange(Quantity quantity) =>
        new QuantityChange(-quantity.Value);

    public static QuantityChange PositiveChange(Quantity quantity) =>
        new QuantityChange(quantity.Value);
}
