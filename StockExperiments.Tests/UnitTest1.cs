using FluentAssertions;

namespace StockExperiments.Tests;

public class Stock_Handle_Arrival
{
    private readonly Stock _stock;
    private readonly TaxStampTypeId _taxStampTypeId;

    public Stock_Handle_Arrival()
    {
        _stock = Stock.Create(new ScanningLocationId(Guid.NewGuid()));

        _taxStampTypeId = new TaxStampTypeId(Guid.NewGuid());
    }

    [Fact]
    public void Arrival_Not_Existing_Type()
    {
        _stock.Handle(new ArrivalEvent(_taxStampTypeId, new Quantity(100)));

        _stock.Items.Should().BeEquivalentTo(
        [
            new { TaxStampTypeId = _taxStampTypeId, Quantity = new Quantity(100), }
        ]);
    }

    [Fact]
    public void Arrival_Existing_Type()
    {
        _stock.Handle(new ArrivalEvent(_taxStampTypeId, new Quantity(100)));

        _stock.Handle(new ArrivalEvent(_taxStampTypeId, new Quantity(100)));

        _stock.Items.Should().BeEquivalentTo(
        [
            new { TaxStampTypeId = _taxStampTypeId, Quantity = new Quantity(200), }
        ]);
    }
}
