using CarLoan.Domain.Providers;
using Xunit;

namespace CarLoan.Domain.Tests;

public class RateTierConstructorTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(-50)]
    public void Constructor_ThrowsArgumentOutOfRangeException_WhenMaximumFinancingRatioIsZeroOrNegative(decimal maximumFinancingRatio)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new RateTier(maximumFinancingRatio, 12.20m));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-12.20)]
    public void Constructor_ThrowsArgumentOutOfRangeException_WhenInterestRateIsZeroOrNegative(decimal interestRate)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new RateTier(90m, interestRate));
    }

    [Fact]
    public void Constructor_CreatesTier_WhenValidValuesProvided()
    {
        var tier = new RateTier(90m, 12.20m);

        Assert.Equal(90m, tier.MaximumFinancingRatio);
        Assert.Equal(12.20m, tier.InterestRate);
    }
}
