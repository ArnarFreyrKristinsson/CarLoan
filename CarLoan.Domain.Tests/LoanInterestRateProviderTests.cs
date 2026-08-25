using CarLoan.Domain.Models;
using CarLoan.Domain.Providers;
using Xunit;

namespace CarLoan.Domain.Tests;

public class LoanInterestRateProviderTests
{
    private static readonly RateTable _general = new(
        [new(50m, 10.35m), new(70m, 11.20m), new(80m, 11.45m), new(90m, 12.20m)], 12.20m);

    private static readonly RateTable _green = new(
        [new(50m, 9.65m), new(70m, 10.50m), new(80m, 10.75m), new(90m, 11.50m)], 11.50m);

    private readonly LoanInterestRateProvider _provider = new(_general, _green);

    private static Loan CreateLoan(VehicleCategory category, decimal downPayment) =>
        new(new LoanTerms(2_000_000m, downPayment, 84, 0m), new Car(CarCondition.New, category, 0));

    [Theory]
    [InlineData(VehicleCategory.PetrolOrDiesel, 1_000_000, 10.35)]
    [InlineData(VehicleCategory.PetrolOrDiesel, 600_000, 11.20)]
    [InlineData(VehicleCategory.PetrolOrDiesel, 400_000, 11.45)]
    [InlineData(VehicleCategory.PetrolOrDiesel, 200_000, 12.20)]
    [InlineData(VehicleCategory.PlugInHybrid, 1_000_000, 10.35)]
    [InlineData(VehicleCategory.PlugInHybrid, 200_000, 12.20)]
    public void GetInterestRate_UsesGeneralTable_WhenVehicleIsNotGreen(
        VehicleCategory category, decimal downPayment, decimal expectedRate)
    {
        Assert.Equal(expectedRate, _provider.GetInterestRate(CreateLoan(category, downPayment)));
    }

    [Theory]
    [InlineData(1_000_000, 9.65)]
    [InlineData(600_000, 10.50)]
    [InlineData(400_000, 10.75)]
    [InlineData(200_000, 11.50)]
    public void GetInterestRate_UsesGreenTable_WhenVehicleIsElectricOrHydrogen(decimal downPayment, decimal expectedRate)
    {
        Assert.Equal(expectedRate, _provider.GetInterestRate(CreateLoan(VehicleCategory.ElectricOrHydrogen, downPayment)));
    }

    [Fact]
    public void GetInterestRate_ThrowsArgumentNullException_WhenLoanIsNull()
    {
        Assert.Throws<ArgumentNullException>(() => _provider.GetInterestRate(null!));
    }

    [Fact]
    public void Constructor_ThrowsArgumentNullException_WhenGeneralTableIsNull()
    {
        Assert.Throws<ArgumentNullException>(() => new LoanInterestRateProvider(null!, _green));
    }

    [Fact]
    public void Constructor_ThrowsArgumentNullException_WhenGreenTableIsNull()
    {
        Assert.Throws<ArgumentNullException>(() => new LoanInterestRateProvider(_general, null!));
    }
}
