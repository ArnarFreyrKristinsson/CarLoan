using CarLoan.Domain.Models;
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

    private readonly RateTable _table = new(_generalTiers);

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

    [Theory]
    [InlineData(90.01)]
    [InlineData(95)]
    public void GetRate_ReturnsNoRate_WhenFinancingRatioAboveHighestTier(decimal financingRatio)
    {
        // lykill.md §5: the bands end at 90%, so there is no rate above it.
        Assert.Null(_table.GetRate(financingRatio));
    }

    [Fact]
    public void Constructor_ThrowsArgumentExceptionNamingTiers_WhenTierListIsEmpty()
    {
        Assert.Throws<ArgumentException>("tiers", () => new RateTable([]));
    }

    [Fact]
    public void GetRate_ReturnsMatchingTierRate_WhenTiersProvidedUnordered()
    {
        var table = new RateTable([new RateTier(90m, 12.20m), new RateTier(50m, 10.35m), new RateTier(70m, 11.20m)]);

        Assert.Equal(11.20m, table.GetRate(65m));
    }

    [Fact]
    public void Constructor_ThrowsArgumentNullException_WhenTierListIsNull()
    {
        Assert.Throws<ArgumentNullException>(() => new RateTable(null!));
    }

    [Fact]
    public void Constructor_ThrowsArgumentException_WhenTierListContainsNullEntry()
    {
        Assert.Throws<ArgumentException>(() => new RateTable([new RateTier(90m, 12.20m), null!]));
    }

    [Fact]
    public void Constructor_ThrowsArgumentException_WhenTierListContainsDuplicateMaximumFinancingRatios()
    {
        Assert.Throws<ArgumentException>(() =>
            new RateTable([new RateTier(80m, 11.45m), new RateTier(80m, 9.99m)]));
    }

    [Fact]
    public void Constructor_ThrowsArgumentException_WhenOnlySomeTiersShareAMaximumFinancingRatio()
    {
        Assert.Throws<ArgumentException>(() =>
            new RateTable([new RateTier(80m, 11.45m), new RateTier(80m, 9.99m), new RateTier(90m, 12.20m)]));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(90)]
    public void FindGap_ReturnsNull_WhenFinancingRatioWithinHighestTier(decimal financingRatio)
    {
        Assert.Null(_table.FindGap(financingRatio));
    }

    [Fact]
    public void FindGap_ReportsRatesUpToHighestTier_WhenFinancingRatioAboveHighestTier()
    {
        // lykill.md §5: the bands end at 90%, so there is no rate above it.
        Assert.Equal(new PricingGap(PricingGapCode.InterestRate, 0m, 90m), _table.FindGap(90.01m));
    }
}
