using FluentAssertions;

namespace StockExperiments.Tests;

public class Stock_Reserve
{
    private const int OriginalQuantity = 100;
    private readonly Stock _stock;
    private readonly TaxStampTypeId _taxStampTypeId;

    public Stock_Reserve()
    {
        _taxStampTypeId = new TaxStampTypeId(Guid.NewGuid());

        _stock = Stock.Create(new ScanningLocationId(Guid.NewGuid()));
        _stock.Handle(new ArrivalEvent([new(_taxStampTypeId, new(OriginalQuantity))]));
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

        Stock_Has_NotChanges();
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

        Stock_Has_NotChanges();
    }

    [Fact]
    public void Too_Much_Reserved()
    {
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
                        new { TaxStampTypeId = _taxStampTypeId, Quantity = new Quantity(OriginalQuantity), },
                    },
                    RemainingItems = new object[]
                    {
                        new { TaxStampTypeId = _taxStampTypeId, Quantity = new Quantity(OriginalQuantity), },
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
                        new { TaxStampTypeId = _taxStampTypeId, Quantity = new Quantity(toWithdraw), },
                    },
                    RemainingItems = new object[]
                    {
                        new { TaxStampTypeId = _taxStampTypeId, Quantity = new Quantity(toWithdraw), },
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

    private void Stock_Has_NotChanges()
    {
        _stock.Should().BeEquivalentTo(
        new
        {
            Items = new object[]
            {
                new { TaxStampTypeId = _taxStampTypeId, Quantity = new Quantity(OriginalQuantity), },
            },
            Reservations = Array.Empty<object>(),
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
}