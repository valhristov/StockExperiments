namespace StockExperiments;

public sealed class Quantity(int value) : SimpleValueObject<int, Quantity>(value)
{
}
