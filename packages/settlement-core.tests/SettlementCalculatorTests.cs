// 파일 용도: 정산 엔진의 핵심 금액 분배 규칙을 검증한다.
// 파일 목적: 공개용 라이브러리 사용자가 회귀 없이 알고리즘을 개선할 수 있게 한다.
using MeetSettle.SettlementCore;

namespace MeetSettle.SettlementCore.Tests;

public sealed class SettlementCalculatorTests
{
    private readonly SettlementCalculator _calculator = new();

    [Fact]
    public void Calculate_WhenSinglePayerPaidForThreePeople_CreatesTwoTransfersToPayer()
    {
        var result = _calculator.Calculate(
            Participants("A", "B", "C"),
            [
                new SettlementExpense("expense-1", "Dinner", "A", 60000m, ["A", "B", "C"])
            ]);

        Assert.Equal(2, result.Transfers.Count);
        Assert.Contains(result.Transfers, transfer =>
            transfer.FromParticipantId == "B" && transfer.ToParticipantId == "A" && transfer.Amount == 20000m);
        Assert.Contains(result.Transfers, transfer =>
            transfer.FromParticipantId == "C" && transfer.ToParticipantId == "A" && transfer.Amount == 20000m);
    }

    [Fact]
    public void Calculate_WhenMultiplePeoplePaid_NetsBalancesBeforeCreatingTransfers()
    {
        var result = _calculator.Calculate(
            Participants("A", "B", "C"),
            [
                new SettlementExpense("expense-1", "Dinner", "A", 60000m, ["A", "B", "C"]),
                new SettlementExpense("expense-2", "Coffee", "B", 30000m, ["A", "B", "C"])
            ]);

        var transfer = Assert.Single(result.Transfers);
        Assert.Equal("C", transfer.FromParticipantId);
        Assert.Equal("A", transfer.ToParticipantId);
        Assert.Equal(30000m, transfer.Amount);
    }

    [Fact]
    public void Calculate_WhenParticipantIsExcluded_DoesNotChargeExcludedParticipant()
    {
        var result = _calculator.Calculate(
            Participants("A", "B", "C"),
            [
                new SettlementExpense("expense-1", "Taxi", "A", 30000m, ["A", "B", "C"], ExcludedParticipantIds: ["C"])
            ]);

        var transfer = Assert.Single(result.Transfers);
        Assert.Equal("B", transfer.FromParticipantId);
        Assert.Equal("A", transfer.ToParticipantId);
        Assert.Equal(15000m, transfer.Amount);
        Assert.Equal(0m, result.Balances.Single(balance => balance.ParticipantId == "C").OwedAmount);
    }

    [Fact]
    public void Calculate_WhenAmountDoesNotDivideEvenly_DistributesRemainderDeterministically()
    {
        var result = _calculator.Calculate(
            Participants("A", "B", "C"),
            [
                new SettlementExpense("expense-1", "Snacks", "A", 100m, ["A", "B", "C"])
            ]);

        Assert.Equal(34m, result.Balances.Single(balance => balance.ParticipantId == "A").OwedAmount);
        Assert.Equal(33m, result.Balances.Single(balance => balance.ParticipantId == "B").OwedAmount);
        Assert.Equal(33m, result.Balances.Single(balance => balance.ParticipantId == "C").OwedAmount);
        Assert.Equal(66m, result.Transfers.Sum(transfer => transfer.Amount));
    }

    [Fact]
    public void Calculate_WhenOnlyOneParticipantExists_CreatesNoTransfers()
    {
        var result = _calculator.Calculate(
            Participants("A"),
            [
                new SettlementExpense("expense-1", "Solo meal", "A", 12000m, ["A"])
            ]);

        Assert.Empty(result.Transfers);
        Assert.Equal(0m, result.Balances.Single().NetAmount);
    }

    [Fact]
    public void Calculate_WhenThereAreNoExpenses_ReturnsZeroBalances()
    {
        var result = _calculator.Calculate(Participants("A", "B"), []);

        Assert.Empty(result.Transfers);
        Assert.All(result.Balances, balance => Assert.Equal(0m, balance.NetAmount));
    }

    [Fact]
    public void Calculate_WhenThereAreMultipleDebtorsAndCreditors_UsesMinimalNetTransfers()
    {
        var result = _calculator.Calculate(
            Participants("A", "B", "C", "D"),
            [
                new SettlementExpense("expense-1", "Room", "A", 40000m, ["A", "B", "C", "D"]),
                new SettlementExpense("expense-2", "Food", "B", 20000m, ["A", "B", "C", "D"])
            ]);

        Assert.Equal(3, result.Transfers.Count);
        Assert.Equal(30000m, result.Transfers.Sum(transfer => transfer.Amount));
    }

    [Fact]
    public void Calculate_WhenExpenseAmountIsNegative_ThrowsValidationError()
    {
        Assert.Throws<ArgumentException>(() => _calculator.Calculate(
            Participants("A", "B"),
            [
                new SettlementExpense("expense-1", "Refund mistake", "A", -1000m, ["A", "B"])
            ]));
    }

    [Fact]
    public void Calculate_WhenPayerDoesNotExist_ThrowsValidationError()
    {
        Assert.Throws<ArgumentException>(() => _calculator.Calculate(
            Participants("A", "B"),
            [
                new SettlementExpense("expense-1", "Dinner", "C", 20000m, ["A", "B"])
            ]));
    }

    [Fact]
    public void Calculate_WhenSharedParticipantDoesNotExist_ThrowsValidationError()
    {
        Assert.Throws<ArgumentException>(() => _calculator.Calculate(
            Participants("A", "B"),
            [
                new SettlementExpense("expense-1", "Dinner", "A", 20000m, ["A", "C"])
            ]));
    }

    [Fact]
    public void Calculate_WhenSharedByIsEmpty_SharesExpenseAcrossAllParticipants()
    {
        var result = _calculator.Calculate(
            Participants("A", "B", "C"),
            [
                new SettlementExpense("expense-1", "Default share", "A", 30000m, [])
            ]);

        Assert.Equal(10000m, result.Balances.Single(balance => balance.ParticipantId == "A").OwedAmount);
        Assert.Equal(10000m, result.Balances.Single(balance => balance.ParticipantId == "B").OwedAmount);
        Assert.Equal(10000m, result.Balances.Single(balance => balance.ParticipantId == "C").OwedAmount);
    }

    [Fact]
    public void Calculate_WhenAllSharedParticipantsAreExcluded_ThrowsValidationError()
    {
        Assert.Throws<ArgumentException>(() => _calculator.Calculate(
            Participants("A", "B"),
            [
                new SettlementExpense("expense-1", "No shares", "A", 20000m, ["A", "B"], ExcludedParticipantIds: ["A", "B"])
            ]));
    }

    [Fact]
    public void Calculate_WhenWeightIsNotPositive_ThrowsValidationError()
    {
        Assert.Throws<ArgumentException>(() => _calculator.Calculate(
            Participants("A", "B"),
            [
                new SettlementExpense(
                    "expense-1",
                    "Invalid weight",
                    "A",
                    20000m,
                    ["A", "B"],
                    new Dictionary<string, decimal> { ["A"] = 1m, ["B"] = 0m })
            ]));
    }

    [Fact]
    public void Calculate_WhenParticipantsAreEmpty_ThrowsValidationError()
    {
        Assert.Throws<ArgumentException>(() => _calculator.Calculate(
            [],
            [
                new SettlementExpense("expense-1", "Dinner", "A", 20000m, ["A"])
            ]));
    }

    private static IReadOnlyCollection<SettlementParticipant> Participants(params string[] ids)
    {
        return ids.Select(id => new SettlementParticipant(id, id)).ToList();
    }
}
