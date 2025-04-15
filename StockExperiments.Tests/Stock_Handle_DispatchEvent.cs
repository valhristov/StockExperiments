using FluentAssertions;

namespace StockExperiments.Tests;

public class Stock_Handle_DispatchEvent
{
    private const int OriginalQuantity = 100;
    private const int ReservedQuantity = 50;
    private readonly Stock _stock;
    private readonly TaxStampTypeId _taxStampTypeId;
    private readonly WithdrawalRequestId _withdrawalRequestId;
    private readonly DispatchEventId _dispatchEventId;
    private readonly ArrivalEventId _arrivalEventId;

    public Stock_Handle_DispatchEvent()
    {
        _taxStampTypeId = new TaxStampTypeId(Guid.NewGuid());
        _withdrawalRequestId = new WithdrawalRequestId(Guid.NewGuid());
        _dispatchEventId = new DispatchEventId(Guid.NewGuid());
        _arrivalEventId = new ArrivalEventId(Guid.NewGuid());

        _stock = Stock.Create(new ScanningLocationId(Guid.NewGuid()));
        _stock.Handle(new ArrivalEvent(
            _arrivalEventId,
            [
                new(_taxStampTypeId, new(OriginalQuantity)),
            ]));

        _stock.Reserve(_withdrawalRequestId,
            [
                new(_taxStampTypeId, new(ReservedQuantity)),
            ]);

        Stock_Not_Changed(); // sanity check
    }

    [Fact]
    public void Missing_TaxStampType()
    {
        // Act
        var result = _stock.Handle(new DispatchEvent(
            _dispatchEventId,
            _withdrawalRequestId,
            [
                new (new TaxStampTypeId(Guid.NewGuid()), new(20))
            ]));

        // Assert
        result.Should().BeFalse();

        Stock_Not_Changed();
    }

    [Fact]
    public void Too_Much()
    {
        // Act
        var result = _stock.Handle(new DispatchEvent(
            new DispatchEventId(Guid.NewGuid()),
            _withdrawalRequestId,
            [
                new (_taxStampTypeId, new(OriginalQuantity + 1))
            ]));

        // Assert
        result.Should().BeFalse();

        Stock_Not_Changed();
    }

    [Fact]
    public void No_Reservation()
    {
        var result = _stock.Handle(new DispatchEvent(
            _dispatchEventId,
            new WithdrawalRequestId(Guid.NewGuid()),
            [
                new (_taxStampTypeId, new(1))
            ]));

        // Assert
        result.Should().BeFalse();

        Stock_Not_Changed();
    }

    [Theory]
    [InlineData(1)]
    [InlineData(ReservedQuantity - 1)]
    public void Partial_Reservation(int toDispatch)
    {
        // Arrange

        // Act
        _stock.Handle(new DispatchEvent(
            _dispatchEventId,
            _withdrawalRequestId,
            [
                new(_taxStampTypeId, new(toDispatch)),
            ]));

        // Assert
        _stock.Should().BeEquivalentTo(
        new
        {
            Items = new object[]
            {
                new { TaxStampTypeId = _taxStampTypeId, Quantity = new Quantity(OriginalQuantity - toDispatch), },
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
                new
                {
                    DispatchEventId = default(DispatchEventId?),
                    ArrivalEventId = _arrivalEventId,
                    Items = new object[]
                    {
                        new { TaxStampTypeId = _taxStampTypeId, QuantityChange = new QuantityChange(OriginalQuantity), },
                    },
                },
                new
                {
                    DispatchEventId = _dispatchEventId,
                    ArrivalEventId = default(ArrivalEventId?),
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
        _stock.Handle(new DispatchEvent(
            _dispatchEventId,
            _withdrawalRequestId,
            [
                new(_taxStampTypeId, new Quantity(toDispatch)),
            ]));

        // Assert
        _stock.Should().BeEquivalentTo(
        new
        {
            Items = new object[]
            {
                new { TaxStampTypeId = _taxStampTypeId, Quantity = new Quantity(OriginalQuantity - toDispatch), },
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
                new
                {
                    DispatchEventId = default(DispatchEventId?),
                    ArrivalEventId = _arrivalEventId,
                    Items = new object[]
                    {
                        new { TaxStampTypeId = _taxStampTypeId, QuantityChange = new QuantityChange(OriginalQuantity), },
                    },
                },
                new
                {
                    DispatchEventId = _dispatchEventId,
                    ArrivalEventId = default(ArrivalEventId?),
                    Items = new object[]
                    {
                        new { TaxStampTypeId = _taxStampTypeId, QuantityChange = new QuantityChange(-toDispatch), },
                    },
                },
            },
        });
    }

    private void Stock_Not_Changed()
    {
        _stock.Should().BeEquivalentTo(
        new
        {
            Items = new object[]
            {
                new { TaxStampTypeId = _taxStampTypeId, Quantity = new Quantity(OriginalQuantity), },
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
                        new { TaxStampTypeId = _taxStampTypeId, Quantity = new Quantity(ReservedQuantity), },
                    },
                },
            },
            Transactions = new object[]
            {
                new
                {
                    DispatchEventId = default(DispatchEventId?),
                    ArrivalEventId = _arrivalEventId,
                    Items = new object[]
                    {
                        new { TaxStampTypeId = _taxStampTypeId, QuantityChange = new QuantityChange(OriginalQuantity), },
                    },
                },
            },
        });
    }
}
