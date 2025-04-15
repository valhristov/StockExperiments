using FluentAssertions;

namespace StockExperiments.Tests;

public class Stock_Handle_DispatchEvent
{
    private const int OriginalQuantity = 100;
    private readonly Stock _stock;
    private readonly TaxStampTypeId _taxStampTypeId;
    private readonly WithdrawalRequestId _withdrawalRequestId;

    public Stock_Handle_DispatchEvent()
    {
        _taxStampTypeId = new TaxStampTypeId(Guid.NewGuid());
        _withdrawalRequestId = new WithdrawalRequestId(Guid.NewGuid());

        _stock = Stock.Create(new ScanningLocationId(Guid.NewGuid()));
        _stock.Handle(new ArrivalEvent([new(_taxStampTypeId, new Quantity(OriginalQuantity))]));
    }

    // 1) dispatch event is greater than reservation
    // 2) dispatch event is smaller than reservation

    [Theory]
    [InlineData(1)]
    [InlineData(OriginalQuantity - 1)]
    [InlineData(OriginalQuantity)]
    public void Full_Reservation(int toDispatch)
    {
        // Arrange
        _stock.BeginDispatch(_withdrawalRequestId,
        [
            new(_taxStampTypeId, new Quantity(toDispatch)),
        ]);

        // Act
        _stock.Handle(new DispatchEvent(_withdrawalRequestId,
        [
            new(_taxStampTypeId, new Quantity(toDispatch)),
        ]));

        // Assert
        _stock.Should().BeEquivalentTo(
        new
        {
            Available = new object[]
            {
                new { TaxStampTypeId = _taxStampTypeId, Quantity = new Quantity(OriginalQuantity - toDispatch), },
            },
            Reserved = new object[]
            {
                new { TaxStampTypeId = _taxStampTypeId, Quantity = new Quantity(0), },
            },
            Reservations = new object[]
            {
                new
                {
                    Status = StockReservationStatus.Completed,
                    OriginalItems = new object[]
                    {
                        new { TaxStampTypeId = _taxStampTypeId, Quantity = new Quantity(toDispatch), },
                    },
                    RemainingItems = new object[]
                    {
                        new { TaxStampTypeId = _taxStampTypeId, Quantity = new Quantity(0), },
                    },
                },
            },
            Transactions = new object[]
            {
                new // arrival
                {
                    Items = new object[]
                    {
                        new { TaxStampTypeId = _taxStampTypeId, QuantityChange = new QuantityChange(OriginalQuantity), },
                    },
                },
                new // dispatch
                {
                    Items = new object[]
                    {
                        new { TaxStampTypeId = _taxStampTypeId, QuantityChange = new QuantityChange(-toDispatch), },
                    },
                },
            },
        });
    }
}
