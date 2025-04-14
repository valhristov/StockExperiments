namespace StockExperiments;

public abstract class SimpleValueObject<TValue, TValueObject>(TValue value)
    where TValue : notnull
    where TValueObject : SimpleValueObject<TValue, TValueObject>
{
    public TValue Value { get; } = value;

    public sealed override bool Equals(object? obj) => obj is TValueObject other &&  Value.Equals(other.Value);
    public sealed override int GetHashCode() => Value.GetHashCode();
    public override string ToString() => Value.ToString()!;

    public static bool operator ==(SimpleValueObject<TValue, TValueObject>? left, SimpleValueObject<TValue, TValueObject>? right) => Equals(left, right);
    public static bool operator !=(SimpleValueObject<TValue, TValueObject>? left, SimpleValueObject<TValue, TValueObject>? right) => !(left == right);
    public static implicit operator TValue(SimpleValueObject<TValue, TValueObject> vo) => vo.Value;
}
