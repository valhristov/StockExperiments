using FluentAssertions;

namespace StockExperiments.Tests;

public class Stock_Handle_ArrivalEvent_More_Than_Once
{
    private const int OriginalQuantity = 100;
    private readonly Stock _stock;
    private readonly ArrivalEventId _arrivalEventId;
    private readonly TaxStampTypeId _taxStampTypeId;

    public Stock_Handle_ArrivalEvent_More_Than_Once()
    {
        _stock = Stock.Create(new ScanningLocationId(Guid.NewGuid()));

        _arrivalEventId = new ArrivalEventId(Guid.NewGuid());
        _taxStampTypeId = new TaxStampTypeId(Guid.NewGuid());

        _stock.Handle(new ArrivalEvent(
            _arrivalEventId,
            [
                new(_taxStampTypeId, new Quantity(OriginalQuantity)),
            ]));
    }

    [Fact]
    public void Arrival_Change_Type()
    {
        var newTaxStampTypeId = new TaxStampTypeId(Guid.NewGuid());

        // Act
        _stock.Handle(new ArrivalEvent(
            _arrivalEventId,
            [
                new(newTaxStampTypeId, new Quantity(OriginalQuantity)),
            ]));

        // Assert
        _stock.Should().BeEquivalentTo(
        new
        {
            Items = new object[]
            {
                new { TaxStampTypeId = _taxStampTypeId, Quantity = Quantity.Zero, },
                new { TaxStampTypeId = newTaxStampTypeId, Quantity = new Quantity(OriginalQuantity), },
            },
            Reservations = Array.Empty<object>(),
            Transactions = new object[]
            {
                new // arrival 2
                {
                    Type = StockTransactionType.Arrival,
                    DispatchEventId = default(DispatchEventId?),
                    ArrivalEventId = _arrivalEventId,
                    Items = new object[]
                    {
                        new { TaxStampTypeId = newTaxStampTypeId, QuantityChange = new QuantityChange(OriginalQuantity), },
                    },
                },
                new // arrival 1 revert
                {
                    Type = StockTransactionType.Revert,
                    DispatchEventId = default(DispatchEventId?),
                    ArrivalEventId = _arrivalEventId,
                    Items = new object[]
                    {
                        new { TaxStampTypeId = _taxStampTypeId, QuantityChange = new QuantityChange(-OriginalQuantity), },
                    },
                },
                new // arrival 1
                {
                    Type = StockTransactionType.Arrival,
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

    [Theory]
    [InlineData(1)]
    [InlineData(int.MaxValue)]
    [InlineData(OriginalQuantity + 1)]
    [InlineData(OriginalQuantity - 1)]
    public void Arrival_Change_Quantity(int newQuantity)
    {
        // Act
        _stock.Handle(new ArrivalEvent(
            _arrivalEventId,
            [
                new(_taxStampTypeId, new Quantity(newQuantity)),
            ]));

        // Assert
        _stock.Should().BeEquivalentTo(
        new
        {
            Items = new object[]
            {
                new { TaxStampTypeId = _taxStampTypeId, Quantity = new Quantity(newQuantity), },
            },
            Reservations = Array.Empty<object>(),
            Transactions = new object[]
            {
                new // arrival 2
                {
                    Type = StockTransactionType.Arrival,
                    DispatchEventId = default(DispatchEventId?),
                    ArrivalEventId = _arrivalEventId,
                    Items = new object[]
                    {
                        new { TaxStampTypeId = _taxStampTypeId, QuantityChange = new QuantityChange(newQuantity), },
                    },
                },
                new // arrival 1 revert
                {
                    Type = StockTransactionType.Revert,
                    DispatchEventId = default(DispatchEventId?),
                    ArrivalEventId = _arrivalEventId,
                    Items = new object[]
                    {
                        new { TaxStampTypeId = _taxStampTypeId, QuantityChange = new QuantityChange(-OriginalQuantity), },
                    },
                },
                new // arrival 1
                {
                    Type = StockTransactionType.Arrival,
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

    [Fact]
    public void Arrival_Same_Quantity()
    {
        // Act
        _stock.Handle(new ArrivalEvent(
            _arrivalEventId,
            [
                new(_taxStampTypeId, new Quantity(OriginalQuantity)),
            ]));

        // Assert
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
                new // arrival 2
                {
                    Type = StockTransactionType.Arrival,
                    DispatchEventId = default(DispatchEventId?),
                    ArrivalEventId = _arrivalEventId,
                    Items = new object[]
                    {
                        new { TaxStampTypeId = _taxStampTypeId, QuantityChange = new QuantityChange(OriginalQuantity), },
                    },
                },
                new // arrival 1 revert
                {
                    Type = StockTransactionType.Revert,
                    DispatchEventId = default(DispatchEventId?),
                    ArrivalEventId = _arrivalEventId,
                    Items = new object[]
                    {
                        new { TaxStampTypeId = _taxStampTypeId, QuantityChange = new QuantityChange(-OriginalQuantity), },
                    },
                },
                new // arrival 1
                {
                    Type = StockTransactionType.Arrival,
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

    [Fact]
    public void Same_Arrival_Updated_More_Than_Once()
    {
        // Arrange
        const int NewQuantity2 = 11;
        _stock.Handle(new ArrivalEvent(
            _arrivalEventId,
            [
                new (_taxStampTypeId, new Quantity(NewQuantity2)),
            ]));

        // Act
        const int NewQuantity3 = 222;
        _stock.Handle(new ArrivalEvent(
            _arrivalEventId,
            [
                new (_taxStampTypeId, new Quantity(NewQuantity3)),
            ]));

        // Assert
        _stock.Should().BeEquivalentTo(
        new
        {
            Items = new object[]
            {
                new { TaxStampTypeId = _taxStampTypeId, Quantity = new Quantity(NewQuantity3), },
            },
            Reservations = Array.Empty<object>(),
            Transactions = new object[]
            {
                new // arrival 3
                {
                    Type = StockTransactionType.Arrival,
                    DispatchEventId = default(DispatchEventId?),
                    ArrivalEventId = _arrivalEventId,
                    Items = new object[]
                    {
                        new { TaxStampTypeId = _taxStampTypeId, QuantityChange = new QuantityChange(NewQuantity3), }
                    },
                },
                new // arrival 2 revert
                {
                    Type = StockTransactionType.Revert,
                    DispatchEventId = default(DispatchEventId?),
                    ArrivalEventId = _arrivalEventId,
                    Items = new object[]
                    {
                        new { TaxStampTypeId = _taxStampTypeId, QuantityChange = new QuantityChange(-NewQuantity2), }
                    },
                },
                new // arrival 2
                {
                    Type = StockTransactionType.Arrival,
                    DispatchEventId = default(DispatchEventId?),
                    ArrivalEventId = _arrivalEventId,
                    Items = new object[]
                    {
                        new { TaxStampTypeId = _taxStampTypeId, QuantityChange = new QuantityChange(NewQuantity2), }
                    },
                },
                new // arrival 1 revert
                {
                    Type = StockTransactionType.Revert,
                    DispatchEventId = default(DispatchEventId?),
                    ArrivalEventId = _arrivalEventId,
                    Items = new object[]
                    {
                        new { TaxStampTypeId = _taxStampTypeId, QuantityChange = new QuantityChange(-OriginalQuantity), }
                    },
                },
                new // arrival 1
                {
                    Type = StockTransactionType.Arrival,
                    DispatchEventId = default(DispatchEventId?),
                    ArrivalEventId = _arrivalEventId,
                    Items = new object[]
                    {
                        new { TaxStampTypeId = _taxStampTypeId, QuantityChange = new QuantityChange(OriginalQuantity), }
                    },
                },
            },
        });
    }

    [Fact]
    public void Another_Arrival_Exists()
    {
        // Arrange
        var otherArrivalEventId = new ArrivalEventId(Guid.NewGuid());
        const int OtherArrivalQuantity = 555;
        _stock.Handle(new ArrivalEvent(
            otherArrivalEventId,
            [
                new (_taxStampTypeId, new Quantity(OtherArrivalQuantity)),
            ]));

        // Act
        const int NewQuantity = 111;
        _stock.Handle(new ArrivalEvent(
            _arrivalEventId,
            [
                new (_taxStampTypeId, new Quantity(NewQuantity)),
            ]));

        // Assert
        _stock.Should().BeEquivalentTo(
        new
        {
            Items = new object[]
            {
                new { TaxStampTypeId = _taxStampTypeId, Quantity = new Quantity(OtherArrivalQuantity + NewQuantity), },
            },
            Reservations = Array.Empty<object>(),
            Transactions = new object[]
            {
                new
                {
                    Type = StockTransactionType.Arrival,
                    DispatchEventId = default(DispatchEventId?),
                    ArrivalEventId = _arrivalEventId,
                    Items = new object[]
                    {
                        new { TaxStampTypeId = _taxStampTypeId, QuantityChange = new QuantityChange(NewQuantity), }
                    },
                },
                new // arrival 1 revert
                {
                    Type = StockTransactionType.Revert,
                    DispatchEventId = default(DispatchEventId?),
                    ArrivalEventId = _arrivalEventId,
                    Items = new object[]
                    {
                        new { TaxStampTypeId = _taxStampTypeId, QuantityChange = new QuantityChange(-OriginalQuantity), }
                    },
                },
                new // arrival 1
                {
                    Type = StockTransactionType.Arrival,
                    DispatchEventId = default(DispatchEventId?),
                    ArrivalEventId = _arrivalEventId,
                    Items = new object[]
                    {
                        new { TaxStampTypeId = _taxStampTypeId, QuantityChange = new QuantityChange(OriginalQuantity), }
                    },
                },
                new // another arrival
                {
                    Type = StockTransactionType.Arrival,
                    DispatchEventId = default(DispatchEventId?),
                    ArrivalEventId = otherArrivalEventId,
                    Items = new object[]
                    {
                        new { TaxStampTypeId = _taxStampTypeId, QuantityChange = new QuantityChange(OtherArrivalQuantity), }
                    },
                }
            },
        });
    }
}
