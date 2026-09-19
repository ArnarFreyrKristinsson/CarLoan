using CarLoan.Domain.Fees;
using Xunit;

namespace CarLoan.Domain.Tests;

public class OriginationFeeSettingsTests
{
    private static readonly IReadOnlyList<FeeTier> _defaultTiers = [new(23, 1.80m), new(84, 3.20m)];

    [Fact]
    public void Constructor_ThrowsArgumentNullException_WhenTiersIsNull()
    {
        Assert.Throws<ArgumentNullException>(() => new OriginationFeeSettings(null!, 18_000m, 50m, 1.00m));
    }

    [Fact]
    public void Constructor_ThrowsArgumentException_WhenTiersIsEmpty()
    {
        Assert.Throws<ArgumentException>(() => new OriginationFeeSettings([], 18_000m, 50m, 1.00m));
    }

    [Fact]
    public void Constructor_ThrowsArgumentException_WhenTiersContainsNullEntry()
    {
        Assert.Throws<ArgumentException>(() => new OriginationFeeSettings([new FeeTier(23, 1.80m), null!], 18_000m, 50m, 1.00m));
    }

    [Fact]
    public void Constructor_ThrowsArgumentException_WhenTiersContainsDuplicateContractLengths()
    {
        Assert.Throws<ArgumentException>(() =>
            new OriginationFeeSettings([new FeeTier(23, 1.80m), new FeeTier(23, 2.00m)], 18_000m, 50m, 1.00m));
    }

    [Fact]
    public void Constructor_ThrowsArgumentException_WhenOnlySomeTiersShareAContractLength()
    {
        Assert.Throws<ArgumentException>(() =>
            new OriginationFeeSettings([new FeeTier(23, 1.80m), new FeeTier(23, 2.00m), new FeeTier(84, 3.20m)], 18_000m, 50m, 1.00m));
    }

    [Fact]
    public void FeeRateFor_ReturnsMatchingTierRate_WhenTiersProvidedUnordered()
    {
        var settings = new OriginationFeeSettings([new FeeTier(84, 3.20m), new FeeTier(23, 1.80m), new FeeTier(47, 2.25m)], 18_000m, 50m, 1.00m);

        Assert.Equal(2.25m, settings.FeeRateFor(30));
    }

    [Fact]
    public void Constructor_ThrowsArgumentOutOfRangeException_WhenMinimumFeeIsNegative()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new OriginationFeeSettings(_defaultTiers, -1m, 50m, 1.00m));
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(101)]
    public void Constructor_ThrowsArgumentOutOfRangeException_WhenGreenFeeDiscountPercentageIsOutsideZeroToHundred(decimal greenFeeDiscountPercentage)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new OriginationFeeSettings(_defaultTiers, 18_000m, greenFeeDiscountPercentage, 1.00m));
    }

    [Fact]
    public void Constructor_ThrowsArgumentOutOfRangeException_WhenPlugInHybridRateDiscountIsNegative()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new OriginationFeeSettings(_defaultTiers, 18_000m, 50m, -1m));
    }

    [Fact]
    public void Constructor_CreatesSettings_WhenValidValuesProvided()
    {
        var settings = new OriginationFeeSettings(_defaultTiers, 18_000m, 50m, 1.00m);

        Assert.Equal(18_000m, settings.MinimumFee);
        Assert.Equal(50m, settings.GreenFeeDiscountPercentage);
        Assert.Equal(1.00m, settings.PlugInHybridRateDiscount);
    }

    [Fact]
    public void Constructor_AcceptsZero_ForMinimumFeeAndPlugInHybridRateDiscount()
    {
        var settings = new OriginationFeeSettings(_defaultTiers, 0m, 50m, 0m);

        Assert.Equal(0m, settings.MinimumFee);
        Assert.Equal(0m, settings.PlugInHybridRateDiscount);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(100)]
    public void Constructor_AcceptsGreenFeeDiscountPercentage_AtZeroAndHundred(decimal greenFeeDiscountPercentage)
    {
        var settings = new OriginationFeeSettings(_defaultTiers, 18_000m, greenFeeDiscountPercentage, 1.00m);

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
}
