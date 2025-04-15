using FluentAssertions;

namespace StockExperiments.Tests;

public class Stock_Handle_DispatchEvent
{
    private const int OriginalQuantity = 100;
    private const int ReservedQuantity = 50;
    private readonly Stock _stock;
    private readonly TaxStampTypeId _taxStampTypeId;
    private readonly WithdrawalRequestId _withdrawalRequestId;

    public Stock_Handle_DispatchEvent()
    {
        _taxStampTypeId = new TaxStampTypeId(Guid.NewGuid());
        _withdrawalRequestId = new WithdrawalRequestId(Guid.NewGuid());

        _stock = Stock.Create(new ScanningLocationId(Guid.NewGuid()));
        _stock.Handle(new ArrivalEvent([new(_taxStampTypeId, new Quantity(OriginalQuantity))]));

        _stock.BeginDispatch(_withdrawalRequestId,
        [
            new(_taxStampTypeId, new Quantity(ReservedQuantity)),
        ]);
    }

    // 1) dispatch event is greater than reservation
    // 2) dispatch event is smaller than reservation

    [Theory]
    [InlineData(1)]
    [InlineData(ReservedQuantity - 1)]
    public void Partial_Reservation(int toDispatch)
    {
        // Arrange

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
                new { TaxStampTypeId = _taxStampTypeId, Quantity = new Quantity(ReservedQuantity - toDispatch), },
            },
            Reservations = new object[]
            {
                new
                {
                    Status = StockReservationStatus.Created,
                    OriginalItems = new object[]
                    {
                        new { TaxStampTypeId = _taxStampTypeId, Quantity = new Quantity(ReservedQuantity), },
                    },
                    RemainingItems = new object[]
                    {
                        new { TaxStampTypeId = _taxStampTypeId, Quantity = new Quantity(ReservedQuantity - toDispatch), },
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

    [Theory]
    // equal to the reservation
    [InlineData(ReservedQuantity)]
    // greater than the reservation
    [InlineData(ReservedQuantity + 1)]
    [InlineData(OriginalQuantity)]
    public void Full_Reservation(int toDispatch)
    {
        // Arrange

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
                        new { TaxStampTypeId = _taxStampTypeId, Quantity = new Quantity(ReservedQuantity), },
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
