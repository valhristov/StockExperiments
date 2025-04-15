namespace StockExperiments;

public sealed class ArrivalEventId(Guid value) : SimpleValueObject<Guid, ArrivalEventId>(value)
{
}
