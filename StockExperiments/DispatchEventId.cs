namespace StockExperiments;

public sealed class DispatchEventId(Guid value) : SimpleValueObject<Guid, DispatchEventId>(value)
{
}
