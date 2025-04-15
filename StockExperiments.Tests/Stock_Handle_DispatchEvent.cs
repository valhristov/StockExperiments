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

        _stock.Reserve(_withdrawalRequestId,
        [
            new(_taxStampTypeId, new Quantity(ReservedQuantity)),
        ]);
    }

    [Fact]
    public void Missing_TaxStampType()
    {
        // Act
        var result = _stock.Handle(new DispatchEvent(_withdrawalRequestId,
        [
            new (new(Guid.NewGuid()), new(20))
        ]));

        // Assert
        result.Should().BeFalse();

        Stock_Has_NotChanges();
    }

    private void Stock_Has_NotChanges()
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
                    Items = new object[]
                    {
                        new { TaxStampTypeId = _taxStampTypeId, QuantityChange = new QuantityChange(OriginalQuantity), },
                    },
                },
            },
        });
    }

    [Fact]
    public void Too_Much()
    {
        // Act
        var result = _stock.Handle(new DispatchEvent(_withdrawalRequestId,
        [
            new (_taxStampTypeId, new(OriginalQuantity + 1))
        ]));

        // Assert
        result.Should().BeFalse();

        Stock_Has_NotChanges();
    }

    [Fact]
    public void No_Reservation()
    {
        var result = _stock.Handle(new DispatchEvent(new(Guid.NewGuid()),
        [
            new (_taxStampTypeId, new(OriginalQuantity + 1))
        ]));

        // Assert
        result.Should().BeFalse();

        Stock_Has_NotChanges();
    }

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
