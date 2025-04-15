using FluentAssertions;

namespace StockExperiments.Tests;

public class Stock_Handle_ArrivalEvent
{
    private readonly Stock _stock;
    private readonly TaxStampTypeId _taxStampTypeId;

    public Stock_Handle_ArrivalEvent()
    {
        _stock = Stock.Create(new ScanningLocationId(Guid.NewGuid()));

        _taxStampTypeId = new TaxStampTypeId(Guid.NewGuid());
    }

    [Fact]
    public void Arrival_Not_Existing_Type()
    {
        var arrivalEventId = new ArrivalEventId(Guid.NewGuid());

        // Act
        _stock.Handle(new ArrivalEvent(
            arrivalEventId,
            [
                new(_taxStampTypeId, new Quantity(100)),
            ]));

        // Assert
        _stock.Should().BeEquivalentTo(
        new
        {
            Items = new object[]
            {
                new { TaxStampTypeId = _taxStampTypeId, Quantity = new Quantity(100), },
            },
            Reservations = Array.Empty<object>(),
            Transactions = new object[]
            {
                new
                {
                    DispatchEventId = default(DispatchEventId?),
                    ArrivalEventId = arrivalEventId,
                    Items = new object[]
                    {
                        new { TaxStampTypeId = _taxStampTypeId, QuantityChange = new QuantityChange(100), },
                    },
                },
            },
        });
    }

    [Fact]
    public void Arrival_Existing_Type()
    {
        var arrivalEventId1 = new ArrivalEventId(Guid.NewGuid());
        var arrivalEventId2 = new ArrivalEventId(Guid.NewGuid());

        // Arrange
        _stock.Handle(new ArrivalEvent(
            arrivalEventId1,
            [
                new(_taxStampTypeId, new(100)),
            ]));

        // Act
        _stock.Handle(new ArrivalEvent(
            arrivalEventId2,
            [
                new(_taxStampTypeId, new(50)),
            ]));

        // Assert
        _stock.Should().BeEquivalentTo(
        new
        {
            Items = new object[]
            {
                new { TaxStampTypeId = _taxStampTypeId, Quantity = new Quantity(150), },
            },
            Reservations = Array.Empty<object>(),
            Transactions = new object[]
            {
                new
                {
                    DispatchEventId = default(DispatchEventId?),
                    ArrivalEventId = arrivalEventId1,
                    Items = new object[]
                    {
                        new { TaxStampTypeId = _taxStampTypeId, QuantityChange = new QuantityChange(100), }
                    },
                },
                new
                {
                    DispatchEventId = default(DispatchEventId?),
                    ArrivalEventId = arrivalEventId2,
                    Items = new object[]
                    {
                        new { TaxStampTypeId = _taxStampTypeId, QuantityChange = new QuantityChange(50), }
                    },
                }
            },
        });
    }

    [Fact]
    public void Arrival_Existing_Other_Type()
    {
        // Arrange
        var arrivalEventId1 = new ArrivalEventId(Guid.NewGuid());
        var arrivalEventId2 = new ArrivalEventId(Guid.NewGuid());
        var otherTaxStampTypeId = new TaxStampTypeId(Guid.NewGuid());
        _stock.Handle(new ArrivalEvent(
            arrivalEventId1,
            [
                new (_taxStampTypeId, new Quantity(100)),
            ]));

        // Act
        _stock.Handle(new ArrivalEvent(
            arrivalEventId2,
            [
                new (otherTaxStampTypeId, new Quantity(50)),
            ]));

        // Assert
        _stock.Should().BeEquivalentTo(
        new
        {
            Items = new object[]
            {
                new { TaxStampTypeId = _taxStampTypeId, Quantity = new Quantity(100), },
                new { TaxStampTypeId = otherTaxStampTypeId, Quantity = new Quantity(50), },
            },
            Reservations = Array.Empty<object>(),
            Transactions = new object[]
            {
                new
                {
                    DispatchEventId = default(DispatchEventId?),
                    ArrivalEventId = arrivalEventId1,
                    Items = new object[]
                    {
                        new { TaxStampTypeId = _taxStampTypeId, QuantityChange = new QuantityChange(100), }
                    },
                },
                new
                {
                    DispatchEventId = default(DispatchEventId?),
                    ArrivalEventId = arrivalEventId2,
                    Items = new object[]
                    {
                        new { TaxStampTypeId = otherTaxStampTypeId, QuantityChange = new QuantityChange(50), }
                    },
                }
            },
        });
    }
}
