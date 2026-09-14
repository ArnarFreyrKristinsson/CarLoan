using CarLoan.Domain.Validators;
using Xunit;

namespace CarLoan.Domain.Tests;

public class CarAgeLimitsTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(-80)]
    public void Constructor_ThrowsArgumentOutOfRangeException_WhenLoanRatioThresholdIsZeroOrNegative(decimal loanRatioThreshold)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new CarAgeLimits(loanRatioThreshold, 12, 20));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-12)]
    public void Constructor_ThrowsArgumentOutOfRangeException_WhenMaximumCombinedYearsAboveThresholdIsZeroOrNegative(int maximumCombinedYears)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new CarAgeLimits(80m, maximumCombinedYears, 20));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-20)]
    public void Constructor_ThrowsArgumentOutOfRangeException_WhenMaximumCombinedYearsWithinThresholdIsZeroOrNegative(int maximumCombinedYears)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new CarAgeLimits(80m, 12, maximumCombinedYears));
    }

    [Fact]
    public void Constructor_CreatesLimits_WhenValidValuesProvided()
    {
        var limits = new CarAgeLimits(80m, 12, 20);

        Assert.Equal(80m, limits.LoanRatioThreshold);
        Assert.Equal(12, limits.MaximumCombinedYearsAboveThreshold);
        Assert.Equal(20, limits.MaximumCombinedYearsWithinThreshold);
    }
}
