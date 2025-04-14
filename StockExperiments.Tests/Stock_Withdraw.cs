using FluentAssertions;

namespace StockExperiments.Tests;

public class Stock_Withdraw
{
    private const int OriginalQuantity = 100;
    private readonly Stock _stock;
    private readonly TaxStampTypeId _taxStampTypeId;

    public Stock_Withdraw()
    {
        _taxStampTypeId = new TaxStampTypeId(Guid.NewGuid());

        _stock = Stock.Create(new ScanningLocationId(Guid.NewGuid()));
        _stock.Handle(new ArrivalEvent(_taxStampTypeId, new Quantity(OriginalQuantity)));
    }

    [Fact]
    public void Withdraw_Missing_TaxStampType()
    {
        // Act
        var result = _stock.Withdraw(new WithdrawalRequestId(Guid.NewGuid()),
        [
            new (new TaxStampTypeId(Guid.NewGuid()), new Quantity(20))
        ]);

        // Assert
        result.Should().BeFalse();

        Stock_Has_NotChanges();
    }

    [Fact]
    public void Withdraw_Too_Much()
    {
        // Act
        var result = _stock.Withdraw(new WithdrawalRequestId(Guid.NewGuid()),
        [
            new (_taxStampTypeId, new Quantity(OriginalQuantity + 1))
        ]);

        // Assert
        result.Should().BeFalse();

        Stock_Has_NotChanges();
    }

    private void Stock_Has_NotChanges()
    {
        _stock.Should().BeEquivalentTo(
        new
        {
            Quantities = new object[]
            {
                new { TaxStampTypeId = _taxStampTypeId, Quantity = new Quantity(OriginalQuantity), },
            },
            Reservations = Array.Empty<object>(),
            Transactions = new object[]
            {
                new
                {
                    Quantities = new object[]
                    {
                        new { TaxStampTypeId = _taxStampTypeId, Quantity = new Quantity(OriginalQuantity), },
                    },
                },
            },
        });
    }
}