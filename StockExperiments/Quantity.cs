using System.Numerics;

namespace StockExperiments;

public sealed class Quantity : SimpleValueObject<int, Quantity>,
    IAdditionOperators<Quantity, QuantityChange, Quantity>
{
    public Quantity(int value) : base(value)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(value, nameof(value));
    }

    public static Quantity operator +(Quantity left, Quantity right) =>
        new(left.Value + right.Value);

    public static Quantity operator +(Quantity left, QuantityChange right) =>
        new(left.Value + right.Value);

    public static bool operator <(Quantity left, Quantity right) =>
        left.Value < Math.Abs(right.Value);

    public static bool operator >(Quantity left, Quantity right) =>
        left.Value > Math.Abs(right.Value);

    public static bool operator <=(Quantity left, Quantity right) =>
        left.Value <= Math.Abs(right.Value);

    public static bool operator >=(Quantity left, Quantity right) =>
        left.Value >= Math.Abs(right.Value);
}
