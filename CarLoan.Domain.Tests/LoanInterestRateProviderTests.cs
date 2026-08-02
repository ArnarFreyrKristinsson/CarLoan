using CarLoan.Domain.Providers;
using Xunit;

namespace CarLoan.Domain.Tests;

public class LoanInterestRateProviderTests
{
    private static readonly IReadOnlyList<RateTier> _defaultTiers =
    [
        new(1000000m, 10.35m),
        new(600000m, 11.20m),
        new(400000m, 11.45m),
        new(200000m, 12.20m)
    ];

    private readonly LoanInterestRateProvider _provider = new(_defaultTiers, 12.20m);

    [Theory]
    [InlineData(1000000, 10.35)]
    [InlineData(600000, 11.20)]
    [InlineData(400000, 11.45)]
    [InlineData(200000, 12.20)]
    public void GetInterestRate_CorrectValue_WhenDownPaymentProvided(decimal downPayment, decimal expectedRate)
    {
        Assert.Equal(expectedRate, _provider.GetInterestRate(downPayment));
    }

    [Fact]
    public void GetInterestRate_ReturnsLowestInterestRate_WhenDownPaymentBelowLowestTier()
    {
        Assert.Equal(12.20m, _provider.GetInterestRate(150_000m));
    }

    [Fact]
    public void GetInterestRate_UsesConfiguredTable_WhenDifferentTiersProvided()
    {
        var provider = new LoanInterestRateProvider([new RateTier(500000m, 8.50m)], 14.00m);

        Assert.Equal(8.50m, provider.GetInterestRate(500000m));
        Assert.Equal(14.00m, provider.GetInterestRate(100000m));
    }
}
