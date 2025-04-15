
namespace StockExperiments;

public class StockReservation
{
    private readonly List<StockReservationItem> _remainingItems;
    private readonly List<StockReservationItem> _originalItems;

    private StockReservation(WithdrawalRequestId withdrawalRequestId, IEnumerable<StockReservationItem> items)
    {
        Status = StockReservationStatus.Created;
        _remainingItems = items.ToList();
        _originalItems = items.ToList();
        WithdrawalRequestId = withdrawalRequestId;
    }

    public StockReservationStatus Status { get; private set; }
    public IReadOnlyCollection<StockReservationItem> RemainingItems => _remainingItems;
    public IReadOnlyCollection<StockReservationItem> OriginalItems => _originalItems;
    public WithdrawalRequestId? WithdrawalRequestId { get; private set; }

    public static StockReservation Create(WithdrawalRequestId withdrawalRequestId, TaxStampQuantitySet quantities) =>
        new(withdrawalRequestId, quantities.Select(StockReservationItem.Create));

    public bool Release(TaxStampQuantitySet quantities)
    {
        var itemsToRelease = quantities.GroupJoin(_remainingItems,
            q => q.TaxStampTypeId,
            r => r.TaxStampTypeId,
            (q, r) => (q.Quantity, ExistingItem: r.SingleOrDefault()));

        foreach (var item in itemsToRelease.Where(x => x.ExistingItem != null))
        {
            item.ExistingItem!.Release(item.Quantity);
        }

        if (RemainingItems.All(x => x.Quantity.Value == 0))
        {
            Status = StockReservationStatus.Completed;
        }

        return true;
    }
}
