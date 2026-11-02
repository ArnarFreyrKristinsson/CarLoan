using CarLoan.Domain.Models;
using CarLoan.Domain.Fees;
using Xunit;

namespace CarLoan.Domain.Tests;

public class OriginationFeeSettingsTests
{
    private static readonly IReadOnlyList<FeeTier> _defaultTiers = [new(23, 1.80m), new(84, 3.20m)];

    [Fact]
    public void Constructor_ThrowsArgumentNullExceptionNamingTiers_WhenTiersIsNull()
    {
        Assert.Throws<ArgumentNullException>(nameof(OriginationFeeSettings.Tiers), () => new OriginationFeeSettings(null!, 18_000m, 50m, 1.00m, 6));
    }

    [Fact]
    public void Constructor_ThrowsArgumentExceptionNamingTiers_WhenTiersIsEmpty()
    {
        Assert.Throws<ArgumentException>(nameof(OriginationFeeSettings.Tiers), () => new OriginationFeeSettings([], 18_000m, 50m, 1.00m, 6));
    }

    [Fact]
    public void Constructor_ThrowsArgumentExceptionNamingTiers_WhenTiersContainsNullEntry()
    {
        Assert.Throws<ArgumentException>(nameof(OriginationFeeSettings.Tiers), () => new OriginationFeeSettings([new FeeTier(23, 1.80m), null!], 18_000m, 50m, 1.00m, 6));
    }

    [Fact]
    public void Constructor_ThrowsArgumentExceptionNamingTiers_WhenTiersContainsDuplicateContractLengths()
    {
        Assert.Throws<ArgumentException>(nameof(OriginationFeeSettings.Tiers), () =>
            new OriginationFeeSettings([new FeeTier(23, 1.80m), new FeeTier(23, 2.00m)], 18_000m, 50m, 1.00m, 6));
    }

    [Fact]
    public void Constructor_ThrowsArgumentException_WhenOnlySomeTiersShareAContractLength()
    {
        Assert.Throws<ArgumentException>(() =>
            new OriginationFeeSettings([new FeeTier(23, 1.80m), new FeeTier(23, 2.00m), new FeeTier(84, 3.20m)], 18_000m, 50m, 1.00m, 6));
    }

    [Fact]
    public void FeeRateFor_ReturnsMatchingTierRate_WhenTiersProvidedUnordered()
    {
        var settings = new OriginationFeeSettings([new FeeTier(84, 3.20m), new FeeTier(23, 1.80m), new FeeTier(47, 2.25m)], 18_000m, 50m, 1.00m, 6);

        Assert.Equal(2.25m, settings.FeeRateFor(30));
    }

    [Fact]
    public void Constructor_ThrowsArgumentOutOfRangeException_WhenMinimumFeeIsNegative()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new OriginationFeeSettings(_defaultTiers, -1m, 50m, 1.00m, 6));
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(101)]
    public void Constructor_ThrowsArgumentOutOfRangeException_WhenGreenFeeDiscountPercentageIsOutsideZeroToHundred(decimal greenFeeDiscountPercentage)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new OriginationFeeSettings(_defaultTiers, 18_000m, greenFeeDiscountPercentage, 1.00m, 6));
    }

    [Fact]
    public void Constructor_ThrowsArgumentOutOfRangeException_WhenPlugInHybridRateDiscountIsNegative()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new OriginationFeeSettings(_defaultTiers, 18_000m, 50m, -1m, 6));
    }

    [Fact]
    public void Constructor_CreatesSettings_WhenValidValuesProvided()
    {
        var settings = new OriginationFeeSettings(_defaultTiers, 18_000m, 50m, 1.00m, 6);

        Assert.Equal(18_000m, settings.MinimumFee);
        Assert.Equal(50m, settings.GreenFeeDiscountPercentage);
        Assert.Equal(1.00m, settings.PlugInHybridRateDiscount);
        Assert.Equal(6, settings.MinimumContractMonths);
    }

    [Fact]
    public void Constructor_AcceptsZero_ForMinimumFeeAndPlugInHybridRateDiscount()
    {
        var settings = new OriginationFeeSettings(_defaultTiers, 0m, 50m, 0m, 6);

        Assert.Equal(0m, settings.MinimumFee);
        Assert.Equal(0m, settings.PlugInHybridRateDiscount);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(100)]
    public void Constructor_AcceptsGreenFeeDiscountPercentage_AtZeroAndHundred(decimal greenFeeDiscountPercentage)
    {
        var settings = new OriginationFeeSettings(_defaultTiers, 18_000m, greenFeeDiscountPercentage, 1.00m, 6);

        Assert.Equal(greenFeeDiscountPercentage, settings.GreenFeeDiscountPercentage);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-23)]
    public void FeeTierConstructor_ThrowsArgumentOutOfRangeException_WhenMaximumContractMonthsIsZeroOrNegative(int maximumContractMonths)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new FeeTier(maximumContractMonths, 1.80m));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1.80)]
    public void FeeTierConstructor_ThrowsArgumentOutOfRangeException_WhenFeeRateIsZeroOrNegative(decimal feeRate)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new FeeTier(23, feeRate));
    }

    [Theory]
    [InlineData(5)]
    [InlineData(85)]
    public void FeeRateFor_ReturnsNoRate_WhenContractLengthOutsideSchedule(int contractMonths)
    {
        // lykill.md §6: FR1 starts at 6 months and FR6 ends at 84; there is no fee outside them.
        var settings = new OriginationFeeSettings(_defaultTiers, 18_000m, 50m, 1.00m, 6);

        Assert.Null(settings.FeeRateFor(contractMonths));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-6)]
    public void Constructor_ThrowsArgumentOutOfRangeExceptionNamingMinimumContractMonths_WhenZeroOrNegative(int minimumContractMonths)
    {
        Assert.Throws<ArgumentOutOfRangeException>(nameof(OriginationFeeSettings.MinimumContractMonths), () =>
            new OriginationFeeSettings(_defaultTiers, 18_000m, 50m, 1.00m, minimumContractMonths));
    }

    [Fact]
    public void Constructor_ThrowsArgumentOutOfRangeExceptionNamingMinimumContractMonths_WhenLongerThanLongestTier()
    {
        Assert.Throws<ArgumentOutOfRangeException>(nameof(OriginationFeeSettings.MinimumContractMonths), () =>
            new OriginationFeeSettings(_defaultTiers, 18_000m, 50m, 1.00m, 85));
    }

    [Fact]
    public void Constructor_ThrowsArgumentOutOfRangeExceptionNamingMinimumContractMonths_WhenLongerThanShortestTier()
    {
        // The minimum must fall inside the first band, or that band could never apply.
        Assert.Throws<ArgumentOutOfRangeException>(nameof(OriginationFeeSettings.MinimumContractMonths), () =>
            new OriginationFeeSettings(_defaultTiers, 18_000m, 50m, 1.00m, 24));
    }

    [Fact]
    public void Constructor_AcceptsMinimumContractMonths_WhenEqualToShortestTier()
    {
        var settings = new OriginationFeeSettings(_defaultTiers, 18_000m, 50m, 1.00m, 23);

        Assert.Equal(23, settings.MinimumContractMonths);
    }

    [Theory]
    [InlineData(5)]
    [InlineData(85)]
    public void FindGap_ReportsScheduledContractLengths_WhenContractLengthOutsideSchedule(int contractMonths)
    {
        // lykill.md §6: the fee schedule runs from 6 (FR1) to 84 months (FR6).
        var settings = new OriginationFeeSettings(_defaultTiers, 18_000m, 50m, 1.00m, 6);

        Assert.Equal(new PricingGap(PricingGapCode.OriginationFee, 6m, 84m), settings.FindGap(contractMonths));
    }

    [Theory]
    [InlineData(6)]
    [InlineData(84)]
    public void FindGap_ReturnsNull_WhenContractLengthWithinSchedule(int contractMonths)
    {
        var settings = new OriginationFeeSettings(_defaultTiers, 18_000m, 50m, 1.00m, 6);

        Assert.Null(settings.FindGap(contractMonths));
    }
}
