using CarLoan.Domain.Validators;
using Xunit;

namespace CarLoan.Domain.Tests;

public class LoanPeriodLimitsTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(-90)]
    public void Constructor_ThrowsArgumentOutOfRangeException_WhenMaximumLoanRatioIsZeroOrNegative(decimal maximumLoanRatio)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new LoanPeriodLimits(maximumLoanRatio, 80m, 84, 72));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-80)]
    public void Constructor_ThrowsArgumentOutOfRangeException_WhenUsedCarLoanRatioThresholdIsZeroOrNegative(decimal usedCarLoanRatioThreshold)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new LoanPeriodLimits(90m, usedCarLoanRatioThreshold, 84, 72));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-84)]
    public void Constructor_ThrowsArgumentOutOfRangeException_WhenMaximumLoanPeriodMonthsIsZeroOrNegative(int maximumLoanPeriodMonths)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new LoanPeriodLimits(90m, 80m, maximumLoanPeriodMonths, 72));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-72)]
    public void Constructor_ThrowsArgumentOutOfRangeException_WhenUsedCarMaximumLoanPeriodMonthsIsZeroOrNegative(int usedCarMaximumLoanPeriodMonths)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new LoanPeriodLimits(90m, 80m, 84, usedCarMaximumLoanPeriodMonths));
    }

    [Fact]
    public void Constructor_CreatesLimits_WhenValidValuesProvided()
    {
        var limits = new LoanPeriodLimits(90m, 80m, 84, 72);

        Assert.Equal(90m, limits.MaximumLoanRatio);
        Assert.Equal(80m, limits.UsedCarLoanRatioThreshold);
        Assert.Equal(84, limits.MaximumLoanPeriodMonths);
        Assert.Equal(72, limits.UsedCarMaximumLoanPeriodMonths);
    }
}
