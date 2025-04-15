namespace StockExperiments;

public class StockReservation
{
    private readonly List<StockReservationItem> _items;

    private StockReservation(WithdrawalRequestId withdrawalRequestId, IEnumerable<StockReservationItem> items)
    {
        _items = items.ToList();
        WithdrawalRequestId = withdrawalRequestId;
    }

    public IReadOnlyCollection<StockReservationItem> Items => _items;
    public WithdrawalRequestId? WithdrawalRequestId { get; private set; }

    public static StockReservation Create(WithdrawalRequestId withdrawalRequestId, TaxStampQuantitySet quantities) =>
        new(withdrawalRequestId, quantities.Select(StockReservationItem.Create));
}

public class StockReservationItem
{
    private StockReservationItem(TaxStampTypeId taxStampTypeId, Quantity quantity)
    {
        TaxStampTypeId = taxStampTypeId;
        Quantity = quantity;
    }

    public TaxStampTypeId TaxStampTypeId { get; private set; }
    public Quantity Quantity { get; private set; }

    public static StockReservationItem Create(TaxStampQuantity quantity) =>
        new(quantity.TaxStampTypeId, quantity.Quantity);
}