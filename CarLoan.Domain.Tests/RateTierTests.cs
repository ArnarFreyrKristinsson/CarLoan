using CarLoan.Domain.Providers;
using Xunit;

namespace CarLoan.Domain.Tests;

public class RateTierConstructorTests
{
    [Fact]
    public void Constructor_ThrowsArgumentOutOfRangeException_WhenMinimumDownPaymentIsNegative()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new RateTier(-200000m, 12.20m));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-12.20)]
    public void Constructor_ThrowsArgumentOutOfRangeException_WhenInterestRateIsZeroOrNegative(decimal interestRate)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new RateTier(200000m, interestRate));
    }

    [Fact]
    public void Constructor_CreatesTier_WhenValidValuesProvided()
    {
        var tier = new RateTier(200000m, 12.20m);

        Assert.Equal(200000m, tier.MinimumDownPayment);
        Assert.Equal(12.20m, tier.InterestRate);
    }
}
