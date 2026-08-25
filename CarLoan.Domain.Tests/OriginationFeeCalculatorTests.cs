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

    // A 2,000,000 car with a 1,000,000 down payment leaves a 1,000,000 loan amount.
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
        // 3.20% halved is 1.60%; 1.60% of 1,000,000 is 16,000, which the minimum fee lifts to 18,000.
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
        // 2,000,000 less a 1,250,000 down payment leaves 750,000; 1.80% of that is 13,500.
        var fee = _calculator.Calculate(CreateLoan(12, VehicleCategory.PetrolOrDiesel, downPayment: 1_250_000m));

        Assert.Equal(18_000m, fee.Amount);
    }

    [Fact]
    public void Calculate_ReportsAmountSaved_WhenDiscountApplied()
    {
        // Undiscounted 3.20% of 1,000,000 is 32,000; the plug-in hybrid pays 22,000.
        var fee = _calculator.Calculate(CreateLoan(84, VehicleCategory.PlugInHybrid));

        Assert.Equal(10_000m, fee.AmountSaved);
    }

    [Fact]
    public void Calculate_ReportsAmountSavedAfterMinimumApplied_WhenMinimumFeeLiftsDiscountedFee()
    {
        // Undiscounted 3.20% of 1,000,000 is 32,000; green pays the 18,000 minimum, not 16,000.
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
        // 1.80% of 750,000 is 13,500 undiscounted, below the 18,000 minimum both discounted and not.
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
