using FluentAssertions;

namespace StockExperiments.Tests;

public class Stock_Reserve
{
    private const int OriginalQuantity = 100;
    private const int ReservedQuantity = 50;
    private readonly Stock _stock;
    private readonly ArrivalEventId _arrivalEventId;
    private readonly TaxStampTypeId _taxStampTypeId;

    public Stock_Reserve()
    {
        _taxStampTypeId = new TaxStampTypeId(Guid.NewGuid());
        _arrivalEventId = new ArrivalEventId(Guid.NewGuid());

        _stock = Stock.Create(new ScanningLocationId(Guid.NewGuid()));
        _stock.Handle(new ArrivalEvent(
            _arrivalEventId,
            [
                new(_taxStampTypeId, new(OriginalQuantity)),
            ]));
    }

    [Fact]
    public void Missing_TaxStampType()
    {
        // Act
        var result = _stock.Reserve(new WithdrawalRequestId(Guid.NewGuid()),
        [
            new (new(Guid.NewGuid()), new(20))
        ]);

        // Assert
        result.Should().BeFalse();

        Stock_Not_Changed();
    }

    [Fact]
    public void Too_Much()
    {
        // Act
        var result = _stock.Reserve(new WithdrawalRequestId(Guid.NewGuid()),
        [
            new (_taxStampTypeId, new (OriginalQuantity + 1))
        ]);

        // Assert
        result.Should().BeFalse();

        Stock_Not_Changed();
    }

    [Fact]
    public void Too_Much_Reserved()
    {
        // Arrange
        _stock.Reserve(new WithdrawalRequestId(Guid.NewGuid()),
        [
            new (_taxStampTypeId, new (OriginalQuantity))
        ]);

        // Act
        var result = _stock.Reserve(new WithdrawalRequestId(Guid.NewGuid()),
        [
            new (_taxStampTypeId, new (1))
        ]);

        // Assert
        result.Should().BeFalse();

        _stock.Should().BeEquivalentTo(
        new
        {
            Reservations = new object[]
            {
                new
                {
                    Status = StockReservationStatus.Active,
                    OriginalItems = new object[]
                    {
                        new { TaxStampTypeId = _taxStampTypeId, Quantity = new Quantity(OriginalQuantity), },
                    },
                    RemainingItems = new object[]
                    {
                        new { TaxStampTypeId = _taxStampTypeId, Quantity = new Quantity(OriginalQuantity), },
                    },
                },
            },
        });
        Transactions_And_Items_Not_Changed();
    }

    [Theory]
    [InlineData(1)]
    [InlineData(OriginalQuantity - ReservedQuantity)]
    public void Success_Some_Reserved(int toWithdraw)
    {
        // Arrange
        _stock.Reserve(new WithdrawalRequestId(Guid.NewGuid()),
        [
            new (_taxStampTypeId, new(ReservedQuantity))
        ]);

        // Act
        var result = _stock.Reserve(new WithdrawalRequestId(Guid.NewGuid()),
        [
            new (_taxStampTypeId, new(toWithdraw))
        ]);

        // Assert
        result.Should().BeTrue();

        _stock.Should().BeEquivalentTo(
        new
        {
            Reservations = new object[]
            {
                new
                {
                    Status = StockReservationStatus.Active,
                    OriginalItems = new object[]
                    {
                        new { TaxStampTypeId = _taxStampTypeId, Quantity = new Quantity(ReservedQuantity), },
                    },
                    RemainingItems = new object[]
                    {
                        new { TaxStampTypeId = _taxStampTypeId, Quantity = new Quantity(ReservedQuantity), },
                    },
                },
                new
                {
                    Status = StockReservationStatus.Active,
                    OriginalItems = new object[]
                    {
                        new { TaxStampTypeId = _taxStampTypeId, Quantity = new Quantity(toWithdraw), },
                    },
                    RemainingItems = new object[]
                    {
                        new { TaxStampTypeId = _taxStampTypeId, Quantity = new Quantity(toWithdraw), },
                    },
                },
            },
        });
        Transactions_And_Items_Not_Changed();
    }

    [Theory]
    [InlineData(1)]
    [InlineData(OriginalQuantity - 1)]
    [InlineData(OriginalQuantity)]
    public void Success(int toWithdraw)
    {
        // Act
        var result = _stock.Reserve(new WithdrawalRequestId(Guid.NewGuid()),
        [
            new (_taxStampTypeId, new(toWithdraw))
        ]);

        // Assert
        result.Should().BeTrue();

        _stock.Should().BeEquivalentTo(
        new
        {
            Reservations = new object[]
            {
                new
                {
                    Status = StockReservationStatus.Active,
                    OriginalItems = new object[]
                    {
                        new { TaxStampTypeId = _taxStampTypeId, Quantity = new Quantity(toWithdraw), },
                    },
                    RemainingItems = new object[]
                    {
                        new { TaxStampTypeId = _taxStampTypeId, Quantity = new Quantity(toWithdraw), },
                    },
                },
            },
        });
        Transactions_And_Items_Not_Changed();
    }

    private void Stock_Not_Changed()
    {
        _stock.Should().BeEquivalentTo(
        new
        {
            Reservations = Array.Empty<object>(),
        });
        Transactions_And_Items_Not_Changed();
    }

    private void Transactions_And_Items_Not_Changed()
    {
        _stock.Should().BeEquivalentTo(
        new
        {
            Items = new object[]
            {
                new { TaxStampTypeId = _taxStampTypeId, Quantity = new Quantity(OriginalQuantity), },
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