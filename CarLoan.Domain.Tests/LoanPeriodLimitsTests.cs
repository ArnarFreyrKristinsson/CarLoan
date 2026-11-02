using CarLoan.Domain.Models;
using CarLoan.Domain.Validators;
using Xunit;

namespace CarLoan.Domain.Tests;

public class LoanPeriodLimitsTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(-80)]
    public void Constructor_ThrowsArgumentOutOfRangeException_WhenUsedCarLoanRatioThresholdIsZeroOrNegative(decimal usedCarLoanRatioThreshold)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new LoanPeriodLimits(usedCarLoanRatioThreshold, 84, 72));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-84)]
    public void Constructor_ThrowsArgumentOutOfRangeException_WhenMaximumLoanPeriodMonthsIsZeroOrNegative(int maximumLoanPeriodMonths)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new LoanPeriodLimits(80m, maximumLoanPeriodMonths, 72));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-72)]
    public void Constructor_ThrowsArgumentOutOfRangeException_WhenUsedCarMaximumLoanPeriodMonthsIsZeroOrNegative(int usedCarMaximumLoanPeriodMonths)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new LoanPeriodLimits(80m, 84, usedCarMaximumLoanPeriodMonths));
    }

    [Fact]
    public void Constructor_CreatesLimits_WhenValidValuesProvided()
    {
        var limits = new LoanPeriodLimits(80m, 84, 72);

        Assert.Equal(80m, limits.UsedCarLoanRatioThreshold);
        Assert.Equal(84, limits.MaximumLoanPeriodMonths);
        Assert.Equal(72, limits.UsedCarMaximumLoanPeriodMonths);
    }

    [Theory]
    [InlineData(CarCondition.New, 85, 84)]
    [InlineData(CarCondition.Used, 80, 84)]
    [InlineData(CarCondition.Used, 80.01, 72)]
    public void MaximumLoanPeriodFor_ReturnsLongestTermForCarAndRatio_WhenLoanTermsProvided(
        CarCondition condition, decimal ratio, int expectedMonths)
    {
        // T1: new, 84 months. T2: used at or below 80%, 84 months. T3: used above 80%, 72 months.
        var limits = new LoanPeriodLimits(80m, 84, 72);

        Assert.Equal(expectedMonths, limits.MaximumLoanPeriodFor(ratio, condition));
    }

    [Fact]
    public void MaximumLoanPeriodFor_KeepsGeneralLimit_WhenUsedCarLimitIsLonger()
    {
        // The general limit applies to every car; the used-car band only shortens it.
        var limits = new LoanPeriodLimits(80m, 84, 96);

        Assert.Equal(84, limits.MaximumLoanPeriodFor(85m, CarCondition.Used));
    }
}
