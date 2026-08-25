using CarLoan.Domain.Models;

namespace CarLoan.Application.Tests;

public class LenderProfilesTests
{
    private static Loan CreateLoan(decimal downPayment, VehicleCategory category) =>
        new(new LoanTerms(2_000_000m, downPayment, 84, 0m), new Car(CarCondition.New, category, 0));

    [Fact]
    public void Build_ReturnsConfiguredLykillProfile_WhenCalled()
    {
        var profiles = LenderProfiles.Build();

        var lykill = profiles["Lykill"];

        Assert.Equal("Lykill", lykill.Name);
        Assert.Equal(6, lykill.Rules.Count);
    }

    [Theory]
    [InlineData(1_000_000, 10.35)]
    [InlineData(600_000, 11.20)]
    [InlineData(400_000, 11.45)]
    [InlineData(200_000, 12.20)]
    public void Build_ConfiguresGeneralRateTable_WhenVehicleIsPetrolOrDiesel(decimal downPayment, decimal expectedRate)
    {
        var lykill = LenderProfiles.Build()["Lykill"];

        Assert.Equal(expectedRate, lykill.RateProvider.GetInterestRate(CreateLoan(downPayment, VehicleCategory.PetrolOrDiesel)));
    }

    [Theory]
    [InlineData(1_000_000, 9.65)]
    [InlineData(600_000, 10.50)]
    [InlineData(400_000, 10.75)]
    [InlineData(200_000, 11.50)]
    public void Build_ConfiguresGreenRateTable_WhenVehicleIsElectricOrHydrogen(decimal downPayment, decimal expectedRate)
    {
        var lykill = LenderProfiles.Build()["Lykill"];

        Assert.Equal(expectedRate, lykill.RateProvider.GetInterestRate(CreateLoan(downPayment, VehicleCategory.ElectricOrHydrogen)));
    }

    [Theory]
    [InlineData(VehicleCategory.PetrolOrDiesel, 3.20)]
    [InlineData(VehicleCategory.PlugInHybrid, 2.20)]
    [InlineData(VehicleCategory.ElectricOrHydrogen, 1.60)]
    public void Build_ConfiguresOriginationFeeSchedule_WhenLongestContractLengthUsed(VehicleCategory category, decimal expectedRate)
    {
        var lykill = LenderProfiles.Build()["Lykill"];

        Assert.Equal(expectedRate, lykill.FeeCalculator.Calculate(CreateLoan(1_000_000m, category)).EffectiveRate);
    }
}
