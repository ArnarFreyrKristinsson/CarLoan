using CarLoan.Domain.Providers;
using Xunit;

namespace CarLoan.Domain.Tests;

public class RateTableTests
{
    private static readonly IReadOnlyList<RateTier> _generalTiers =
    [
        new(50m, 10.35m),
        new(70m, 11.20m),
        new(80m, 11.45m),
        new(90m, 12.20m)
    ];

    private readonly RateTable _table = new(_generalTiers, 12.20m);

    [Theory]
    [InlineData(0, 10.35)]
    [InlineData(50, 10.35)]
    [InlineData(50.01, 11.20)]
    [InlineData(70, 11.20)]
    [InlineData(70.01, 11.45)]
    [InlineData(80, 11.45)]
    [InlineData(80.01, 12.20)]
    [InlineData(90, 12.20)]
    public void GetRate_CorrectValue_WhenFinancingRatioProvided(decimal financingRatio, decimal expectedRate)
    {
        Assert.Equal(expectedRate, _table.GetRate(financingRatio));
    }

    [Fact]
    public void GetRate_ReturnsDefaultInterestRate_WhenFinancingRatioAboveHighestTier()
    {
        Assert.Equal(12.20m, _table.GetRate(95m));
    }

    [Fact]
    public void GetRate_ReturnsMatchingTierRate_WhenTiersProvidedUnordered()
    {
        var table = new RateTable([new RateTier(90m, 12.20m), new RateTier(50m, 10.35m), new RateTier(70m, 11.20m)], 12.20m);

        Assert.Equal(11.20m, table.GetRate(65m));
    }

    [Fact]
    public void Constructor_ThrowsArgumentNullException_WhenTierListIsNull()
    {
        Assert.Throws<ArgumentNullException>(() => new RateTable(null!, 12.20m));
    }

    [Fact]
    public void Constructor_ThrowsArgumentException_WhenTierListContainsNullEntry()
    {
        Assert.Throws<ArgumentException>(() => new RateTable([new RateTier(90m, 12.20m), null!], 12.20m));
    }

    [Fact]
    public void Constructor_ThrowsArgumentException_WhenTierListContainsDuplicateMaximumFinancingRatios()
    {
        Assert.Throws<ArgumentException>(() =>
            new RateTable([new RateTier(80m, 11.45m), new RateTier(80m, 9.99m)], 12.20m));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-12.20)]
    public void Constructor_ThrowsArgumentOutOfRangeException_WhenDefaultInterestRateIsZeroOrNegative(decimal defaultInterestRate)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new RateTable(_generalTiers, defaultInterestRate));
    }
}
