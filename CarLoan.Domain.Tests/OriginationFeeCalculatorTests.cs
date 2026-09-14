using CarLoan.Domain.Fees;
using CarLoan.Domain.Models;
using Xunit;

namespace CarLoan.Domain.Tests;

public class OriginationFeeCalculatorTests
{
    private static readonly OriginationFeeSettings _defaultSettings = new(
        Tiers:
        [
            new(23, 1.80m),
            new(35, 2.00m),
            new(47, 2.25m),
            new(59, 2.50m),
            new(71, 3.00m),
            new(84, 3.20m)
        ],
        MinimumFee: 18_000m,
        GreenFeeDiscountPercentage: 50m,
        PlugInHybridRateDiscount: 1.00m);

    private readonly OriginationFeeCalculator _calculator = new(_defaultSettings);

    private static Loan CreateLoan(int loanPeriodInMonths, VehicleCategory category, decimal downPayment = 1_000_000m) =>
        new(new LoanTerms(2_000_000m, downPayment, loanPeriodInMonths, 10.35m),
            new Car(CarCondition.New, category, 0));

    [Fact]
    public void Calculate_ThrowsArgumentNullException_WhenLoanIsNull()
    {
        Assert.Throws<ArgumentNullException>(() => _calculator.Calculate(null!));
    }

    [Fact]
    public void Constructor_ThrowsArgumentNullException_WhenSettingsIsNull()
    {
        Assert.Throws<ArgumentNullException>(() => new OriginationFeeCalculator(null!));
    }

    [Theory]
    [InlineData(1, 1.80)]
    [InlineData(23, 1.80)]
    [InlineData(24, 2.00)]
    [InlineData(35, 2.00)]
    [InlineData(36, 2.25)]
    [InlineData(47, 2.25)]
    [InlineData(48, 2.50)]
    [InlineData(59, 2.50)]
    [InlineData(60, 3.00)]
    [InlineData(71, 3.00)]
    [InlineData(72, 3.20)]
    [InlineData(84, 3.20)]
    public void Calculate_AppliesFeeRateForContractLength_WhenVehicleIsPetrolOrDiesel(int loanPeriodInMonths, decimal expectedRate)
    {
        var fee = _calculator.Calculate(CreateLoan(loanPeriodInMonths, VehicleCategory.PetrolOrDiesel));

        Assert.Equal(expectedRate, fee.EffectiveRate);
        Assert.Equal(Math.Max(1_000_000m * expectedRate / 100m, 18_000m), fee.Amount);
    }

    [Fact]
    public void Calculate_SubtractsOnePercentagePointFromRate_WhenVehicleIsPlugInHybrid()
    {
        var fee = _calculator.Calculate(CreateLoan(84, VehicleCategory.PlugInHybrid));

        Assert.Equal(2.20m, fee.EffectiveRate);
        Assert.Equal(22_000m, fee.Amount);
    }

    [Fact]
    public void Calculate_HalvesFeeAmount_WhenVehicleIsElectricOrHydrogen()
    {
        var fee = _calculator.Calculate(CreateLoan(84, VehicleCategory.ElectricOrHydrogen));

        Assert.Equal(1.60m, fee.EffectiveRate);
        Assert.Equal(18_000m, fee.Amount);
    }

    [Fact]
    public void Calculate_DoesNotHalveFeeAmount_WhenVehicleIsPlugInHybrid()
    {
        var plugInHybrid = _calculator.Calculate(CreateLoan(84, VehicleCategory.PlugInHybrid));
        var green = _calculator.Calculate(CreateLoan(84, VehicleCategory.ElectricOrHydrogen));

        Assert.NotEqual(green.EffectiveRate, plugInHybrid.EffectiveRate);
        Assert.Equal(2.20m, plugInHybrid.EffectiveRate);
    }

    [Fact]
    public void Calculate_RaisesFeeToMinimum_WhenDiscountedFeeIsBelowMinimum()
    {
        var fee = _calculator.Calculate(CreateLoan(12, VehicleCategory.PetrolOrDiesel, downPayment: 1_250_000m));

        Assert.Equal(18_000m, fee.Amount);
    }

    [Fact]
    public void Calculate_ReportsAmountSaved_WhenDiscountApplied()
    {
        var fee = _calculator.Calculate(CreateLoan(84, VehicleCategory.PlugInHybrid));

        Assert.Equal(10_000m, fee.AmountSaved);
    }

    [Fact]
    public void Calculate_ReportsAmountSavedAfterMinimumApplied_WhenMinimumFeeLiftsDiscountedFee()
    {
        var fee = _calculator.Calculate(CreateLoan(84, VehicleCategory.ElectricOrHydrogen));

        Assert.Equal(14_000m, fee.AmountSaved);
    }

    [Fact]
    public void Calculate_ReportsNoAmountSaved_WhenNoDiscountApplies()
    {
        var fee = _calculator.Calculate(CreateLoan(84, VehicleCategory.PetrolOrDiesel));

        Assert.Equal(0m, fee.AmountSaved);
    }

    [Fact]
    public void Calculate_ReportsNoAmountSaved_WhenMinimumFeeExceedsUndiscountedFee()
    {
        var fee = _calculator.Calculate(CreateLoan(12, VehicleCategory.ElectricOrHydrogen, downPayment: 1_250_000m));

        Assert.Equal(0m, fee.AmountSaved);
    }

    [Fact]
    public void Calculate_UsesHighestTierRate_WhenContractLengthExceedsEveryTier()
    {
        var fee = _calculator.Calculate(CreateLoan(120, VehicleCategory.PetrolOrDiesel));

        Assert.Equal(3.20m, fee.EffectiveRate);
    }
}
