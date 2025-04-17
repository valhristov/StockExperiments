using FluentAssertions;

namespace StockExperiments.Tests;

public class Stock_Handle_DispatchEvent_More_Than_Once
{
    private const int OriginalQuantity1 = 1000;
    private const int OriginalQuantity2 = 3000;
    private const int OriginallyDispatchedQuantity = 55;
    private readonly Stock _stock;
    private readonly ArrivalEventId _arrivalEventId = new (Guid.NewGuid());
    private readonly DispatchEventId _dispatchEventId = new (Guid.NewGuid());
    private readonly TaxStampTypeId _taxStampTypeId1 = new (Guid.NewGuid());
    private readonly TaxStampTypeId _taxStampTypeId2 = new (Guid.NewGuid());
    private readonly WithdrawalRequestId _withdrawalRequestId = new (Guid.NewGuid());

    public Stock_Handle_DispatchEvent_More_Than_Once()
    {
        _stock = Stock.Create(new ScanningLocationId(Guid.NewGuid()));

        _stock.Handle(new ArrivalEvent(
            _arrivalEventId,
            [
                new(_taxStampTypeId1, new Quantity(OriginalQuantity1)),
                new(_taxStampTypeId2, new Quantity(OriginalQuantity2)),
            ]));

        _stock.Handle(new DispatchEvent(
            _dispatchEventId,
            _withdrawalRequestId,
            [
                new(_taxStampTypeId1, new Quantity(OriginallyDispatchedQuantity)),
            ]));
    }

    // TODO:
    // second dispatch has more items than available

    [Fact]
    public void Dispatch_Change_Type()
    {
        // Act
        _stock.Handle(new DispatchEvent(
            _dispatchEventId,
            _withdrawalRequestId,
            [
                new(_taxStampTypeId2, new Quantity(OriginallyDispatchedQuantity)),
            ]));

        // Assert
        _stock.Should().BeEquivalentTo(
        new
        {
            Items = new object[]
            {
                new { TaxStampTypeId = _taxStampTypeId1, Quantity = new Quantity(OriginalQuantity1), },
                new { TaxStampTypeId = _taxStampTypeId2, Quantity = new Quantity(OriginalQuantity2 - OriginallyDispatchedQuantity), },
            },
            Reservations = Array.Empty<object>(),
            Transactions = new object[]
            {
                new // dispatch 2
                {
                    Type = StockTransactionType.Dispatch,
                    DispatchEventId = _dispatchEventId,
                    ArrivalEventId = default(ArrivalEventId?),
                    Items = new object[]
                    {
                        new { TaxStampTypeId = _taxStampTypeId2, QuantityChange = new QuantityChange(-OriginallyDispatchedQuantity), },
                    },
                },
                new // dispatch 1 revert
                {
                    Type = StockTransactionType.Revert,
                    DispatchEventId = _dispatchEventId,
                    ArrivalEventId = default(ArrivalEventId?),
                    Items = new object[]
                    {
                        new { TaxStampTypeId = _taxStampTypeId1, QuantityChange = new QuantityChange(OriginallyDispatchedQuantity), },
                    },
                },
                new // dispatch 1
                {
                    Type = StockTransactionType.Dispatch,
                    DispatchEventId = _dispatchEventId,
                    ArrivalEventId = default(ArrivalEventId?),
                    Items = new object[]
                    {
                        new { TaxStampTypeId = _taxStampTypeId1, QuantityChange = new QuantityChange(-OriginallyDispatchedQuantity), },
                    },
                },
                new // arrival
                {
                    Type = StockTransactionType.Arrival,
                    DispatchEventId = default(DispatchEventId?),
                    ArrivalEventId = _arrivalEventId,
                    Items = new object[]
                    {
                        new { TaxStampTypeId = _taxStampTypeId1, QuantityChange = new QuantityChange(OriginalQuantity1), },
                        new { TaxStampTypeId = _taxStampTypeId2, QuantityChange = new QuantityChange(OriginalQuantity2), },
                    },
                },
            },
        });
    }

    [Theory]
    [InlineData(1)]
    [InlineData(OriginalQuantity1)]
    [InlineData(OriginallyDispatchedQuantity + 1)]
    [InlineData(OriginallyDispatchedQuantity - 1)]
    public void Dispatch_Change_Quantity(int newDispatchedQuantity)
    {
        // Act
        _stock.Handle(new DispatchEvent(
            _dispatchEventId,
            _withdrawalRequestId,
            [
                new(_taxStampTypeId1, new Quantity(newDispatchedQuantity)),
            ]));

        // Assert
        _stock.Should().BeEquivalentTo(
        new
        {
            Items = new object[]
            {
                new { TaxStampTypeId = _taxStampTypeId1, Quantity = new Quantity(OriginalQuantity1 - newDispatchedQuantity), },
                new { TaxStampTypeId = _taxStampTypeId2, Quantity = new Quantity(OriginalQuantity2), },
            },
            Reservations = Array.Empty<object>(),
            Transactions = new object[]
            {
                new // dispatch 2
                {
                    Type = StockTransactionType.Dispatch,
                    DispatchEventId = _dispatchEventId,
                    ArrivalEventId = default(ArrivalEventId?),
                    Items = new object[]
                    {
                        new { TaxStampTypeId = _taxStampTypeId1, QuantityChange = new QuantityChange(-newDispatchedQuantity), },
                    },
                },
                new // dispatch 1 revert
                {
                    Type = StockTransactionType.Revert,
                    DispatchEventId = _dispatchEventId,
                    ArrivalEventId = default(ArrivalEventId?),
                    Items = new object[]
                    {
                        new { TaxStampTypeId = _taxStampTypeId1, QuantityChange = new QuantityChange(OriginallyDispatchedQuantity), },
                    },
                },
                new // dispatch 1
                {
                    Type = StockTransactionType.Dispatch,
                    DispatchEventId = _dispatchEventId,
                    ArrivalEventId = default(ArrivalEventId?),
                    Items = new object[]
                    {
                        new { TaxStampTypeId = _taxStampTypeId1, QuantityChange = new QuantityChange(-OriginallyDispatchedQuantity), },
                    },
                },
                new // arrival
                {
                    Type = StockTransactionType.Arrival,
                    DispatchEventId = default(DispatchEventId?),
                    ArrivalEventId = _arrivalEventId,
                    Items = new object[]
                    {
                        new { TaxStampTypeId = _taxStampTypeId1, QuantityChange = new QuantityChange(OriginalQuantity1), },
                        new { TaxStampTypeId = _taxStampTypeId2, QuantityChange = new QuantityChange(OriginalQuantity2), },
                    },
                },
            },
        });
    }

    [Fact]
    public void Dispatch_Same_Quantity()
    {
        // Act
        _stock.Handle(new DispatchEvent(
            _dispatchEventId,
            _withdrawalRequestId,
            [
                new(_taxStampTypeId1, new Quantity(OriginallyDispatchedQuantity)),
            ]));

        // Assert
        _stock.Should().BeEquivalentTo(
        new
        {
            Items = new object[]
            {
                new { TaxStampTypeId = _taxStampTypeId1, Quantity = new Quantity(OriginalQuantity1 - OriginallyDispatchedQuantity), },
                new { TaxStampTypeId = _taxStampTypeId2, Quantity = new Quantity(OriginalQuantity2), },
            },
            Reservations = Array.Empty<object>(),
            Transactions = new object[]
            {
                new // dispatch 2
                {
                    Type = StockTransactionType.Dispatch,
                    DispatchEventId = _dispatchEventId,
                    ArrivalEventId = default(ArrivalEventId?),
                    Items = new object[]
                    {
                        new { TaxStampTypeId = _taxStampTypeId1, QuantityChange = new QuantityChange(-OriginallyDispatchedQuantity), },
                    },
                },
                new // dispatch 1 revert
                {
                    Type = StockTransactionType.Revert,
                    DispatchEventId = _dispatchEventId,
                    ArrivalEventId = default(ArrivalEventId?),
                    Items = new object[]
                    {
                        new { TaxStampTypeId = _taxStampTypeId1, QuantityChange = new QuantityChange(OriginallyDispatchedQuantity), },
                    },
                },
                new // dispatch 1
                {
                    Type = StockTransactionType.Dispatch,
                    DispatchEventId = _dispatchEventId,
                    ArrivalEventId = default(ArrivalEventId?),
                    Items = new object[]
                    {
                        new { TaxStampTypeId = _taxStampTypeId1, QuantityChange = new QuantityChange(-OriginallyDispatchedQuantity), },
                    },
                },
                new // arrival
                {
                    Type = StockTransactionType.Arrival,
                    DispatchEventId = default(DispatchEventId?),
                    ArrivalEventId = _arrivalEventId,
                    Items = new object[]
                    {
                        new { TaxStampTypeId = _taxStampTypeId1, QuantityChange = new QuantityChange(OriginalQuantity1), },
                        new { TaxStampTypeId = _taxStampTypeId2, QuantityChange = new QuantityChange(OriginalQuantity2), },
                    },
                },
            },
        });
    }

    [Fact]
    public void Same_Dispatch_Updated_More_Than_Once()
    {
        // Arrange
        const int NewQuantity1 = 11;
        _stock.Handle(new DispatchEvent(
            _dispatchEventId,
            _withdrawalRequestId,
            [
                new (_taxStampTypeId1, new Quantity(NewQuantity1)),
            ]));

        // Act
        const int NewQuantity2 = 222;
        _stock.Handle(new DispatchEvent(
            _dispatchEventId,
            _withdrawalRequestId,
            [
                new (_taxStampTypeId1, new Quantity(NewQuantity2)),
            ]));

        // Assert
        _stock.Should().BeEquivalentTo(
        new
        {
            Items = new object[]
            {
                new { TaxStampTypeId = _taxStampTypeId1, Quantity = new Quantity(OriginalQuantity1 - NewQuantity2), },
                new { TaxStampTypeId = _taxStampTypeId2, Quantity = new Quantity(OriginalQuantity2), },
            },
            Reservations = Array.Empty<object>(),
            Transactions = new object[]
            {
                new // dispatch 3
                {
                    Type = StockTransactionType.Dispatch,
                    DispatchEventId = _dispatchEventId,
                    ArrivalEventId = default(ArrivalEventId?),
                    Items = new object[]
                    {
                        new { TaxStampTypeId = _taxStampTypeId1, QuantityChange = new QuantityChange(-NewQuantity2), }
                    },
                },
                new // dispatch 2 revert
                {
                    Type = StockTransactionType.Revert,
                    DispatchEventId = _dispatchEventId,
                    ArrivalEventId = default(ArrivalEventId?),
                    Items = new object[]
                    {
                        new { TaxStampTypeId = _taxStampTypeId1, QuantityChange = new QuantityChange(NewQuantity1), }
                    },
                },
                new // dispatch 2
                {
                    Type = StockTransactionType.Dispatch,
                    DispatchEventId = _dispatchEventId,
                    ArrivalEventId = default(ArrivalEventId?),
                    Items = new object[]
                    {
                        new { TaxStampTypeId = _taxStampTypeId1, QuantityChange = new QuantityChange(-NewQuantity1), }
                    },
                },
                new // dispatch 1 revert
                {
                    Type = StockTransactionType.Revert,
                    DispatchEventId = _dispatchEventId,
                    ArrivalEventId = default(ArrivalEventId?),
                    Items = new object[]
                    {
                        new { TaxStampTypeId = _taxStampTypeId1, QuantityChange = new QuantityChange(OriginallyDispatchedQuantity), }
                    },
                },
                new // dispatch 1
                {
                    Type = StockTransactionType.Dispatch,
                    DispatchEventId = _dispatchEventId,
                    ArrivalEventId = default(ArrivalEventId?),
                    Items = new object[]
                    {
                        new { TaxStampTypeId = _taxStampTypeId1, QuantityChange = new QuantityChange(-OriginallyDispatchedQuantity), }
                    },
                },
                new // arrival
                {
                    Type = StockTransactionType.Arrival,
                    DispatchEventId = default(DispatchEventId?),
                    ArrivalEventId = _arrivalEventId,
                    Items = new object[]
                    {
                        new { TaxStampTypeId = _taxStampTypeId1, QuantityChange = new QuantityChange(OriginalQuantity1), },
                        new { TaxStampTypeId = _taxStampTypeId2, QuantityChange = new QuantityChange(OriginalQuantity2), },
                    },
                },
            },
        });
    }

    [Fact]
    public void Dispatch_Another_Dispatch_Exists()
    {
        // Arrange
        var otherDispatchEventId = new DispatchEventId(Guid.NewGuid());
        const int OtherDispatchQuantity = 77;
        _stock.Handle(new DispatchEvent(
            otherDispatchEventId,
            _withdrawalRequestId,
            [
                new (_taxStampTypeId1, new Quantity(OtherDispatchQuantity)),
            ]));

        // Act
        const int NewDispatchQuantity = 37;
        _stock.Handle(new DispatchEvent(
            _dispatchEventId,
            _withdrawalRequestId,
            [
                new (_taxStampTypeId1, new Quantity(NewDispatchQuantity)),
            ]));

        // Assert
        _stock.Should().BeEquivalentTo(
        new
        {
            Items = new object[]
            {
                new { TaxStampTypeId = _taxStampTypeId1, Quantity = new Quantity(OriginalQuantity1 - OtherDispatchQuantity - NewDispatchQuantity), },
                new { TaxStampTypeId = _taxStampTypeId2, Quantity = new Quantity(OriginalQuantity2), },
            },
            Reservations = Array.Empty<object>(),
            Transactions = new object[]
            {
                new // dispatch 2
                {
                    Type = StockTransactionType.Dispatch,
                    DispatchEventId = _dispatchEventId,
                    ArrivalEventId = default(ArrivalEventId?),
                    Items = new object[]
                    {
                        new { TaxStampTypeId = _taxStampTypeId1, QuantityChange = new QuantityChange(-NewDispatchQuantity), }
                    },
                },
                new // dispatch 1 revert
                {
                    Type = StockTransactionType.Revert,
                    DispatchEventId = _dispatchEventId,
                    ArrivalEventId = default(ArrivalEventId?),
                    Items = new object[]
                    {
                        new { TaxStampTypeId = _taxStampTypeId1, QuantityChange = new QuantityChange(OriginallyDispatchedQuantity), }
                    },
                },
                new // dispatch 1
                {
                    Type = StockTransactionType.Dispatch,
                    DispatchEventId = _dispatchEventId,
                    ArrivalEventId = default(ArrivalEventId?),
                    Items = new object[]
                    {
                        new { TaxStampTypeId = _taxStampTypeId1, QuantityChange = new QuantityChange(-OriginallyDispatchedQuantity), }
                    },
                },
                new // dispatch other
                {
                    Type = StockTransactionType.Dispatch,
                    DispatchEventId = otherDispatchEventId,
                    ArrivalEventId = default(ArrivalEventId?),
                    Items = new object[]
                    {
                        new { TaxStampTypeId = _taxStampTypeId1, QuantityChange = new QuantityChange(-OtherDispatchQuantity), }
                    },
                },
                new // arrival
                {
                    Type = StockTransactionType.Arrival,
                    DispatchEventId = default(DispatchEventId?),
                    ArrivalEventId = _arrivalEventId,
                    Items = new object[]
                    {
                        new { TaxStampTypeId = _taxStampTypeId1, QuantityChange = new QuantityChange(OriginalQuantity1), },
                        new { TaxStampTypeId = _taxStampTypeId2, QuantityChange = new QuantityChange(OriginalQuantity2), },
                    },
                },
            },
        });
    }
}
