using FluentAssertions;

namespace StockExperiments.Tests;

public class Stock_Withdraw
{
    private readonly Stock _stock;
    private readonly TaxStampTypeId _taxStampTypeId;

    public Stock_Withdraw()
    {
        _taxStampTypeId = new TaxStampTypeId(Guid.NewGuid());

        _stock = Stock.Create(new ScanningLocationId(Guid.NewGuid()));
        _stock.Handle(new ArrivalEvent(_taxStampTypeId, new Quantity(100)));
    }

    [Fact]
    public void Withdraw_Missing_TaxStampType()
    {
        // Act
        var result = _stock.Withdraw(new WithdrawalRequestId(Guid.NewGuid()),
        [
            new (new TaxStampTypeId(Guid.NewGuid()), new Quantity(100))
        ]);

        // Assert
        result.Should().BeFalse();

        _stock.Should().BeEquivalentTo(
        new
        {
            Quantities = new object[]
            {
                new { TaxStampTypeId = _taxStampTypeId, Quantity = new Quantity(100), },
            },
            Reservations = Array.Empty<object>(),
            Transactions = new object[]
            {
                new
                {
                    Quantities = new object[]
                    {
                        new { TaxStampTypeId = _taxStampTypeId, Quantity = new Quantity(100), },
                    },
                },
            },
        });
}
}