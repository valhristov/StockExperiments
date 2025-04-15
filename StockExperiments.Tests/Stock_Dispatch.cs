using FluentAssertions;

namespace StockExperiments.Tests;

public class Stock_Dispatch
{
    private const int OriginalQuantity = 100;
    private readonly Stock _stock;
    private readonly TaxStampTypeId _taxStampTypeId;

    public Stock_Dispatch()
    {
        _taxStampTypeId = new TaxStampTypeId(Guid.NewGuid());

        _stock = Stock.Create(new ScanningLocationId(Guid.NewGuid()));
        _stock.Handle(new ArrivalEvent([new(_taxStampTypeId, new(OriginalQuantity))]));
    }

    [Fact]
    public void Dispatch_Missing_TaxStampType()
    {
        // Act
        var result = _stock.Dispatch(new WithdrawalRequestId(Guid.NewGuid()),
        [
            new (new(Guid.NewGuid()), new(20))
        ]);

        // Assert
        result.Should().BeFalse();

        Stock_Has_NotChanges();
    }

    [Fact]
    public void Dispatch_Too_Much()
    {
        // Act
        var result = _stock.Dispatch(new WithdrawalRequestId(Guid.NewGuid()),
        [
            new (_taxStampTypeId, new (OriginalQuantity + 1))
        ]);

        // Assert
        result.Should().BeFalse();

        Stock_Has_NotChanges();
    }

    [Theory]
    [InlineData(1)]
    [InlineData(OriginalQuantity - 1)]
    [InlineData(OriginalQuantity)]
    public void Dispatch_Success(int toWithdraw)
    {
        // Act
        var result = _stock.Dispatch(new WithdrawalRequestId(Guid.NewGuid()),
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
                new { TaxStampTypeId = _taxStampTypeId, Quantity = new Quantity(OriginalQuantity-toWithdraw), },
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
                new
                {
                    Items = new object[]
                    {
                        new { TaxStampTypeId = _taxStampTypeId, QuantityChange = new QuantityChange(-toWithdraw), },
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