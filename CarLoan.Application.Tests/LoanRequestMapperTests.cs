using CarLoan.Application.Mapping;
using CarLoan.Application.Requests;
using CarLoan.Domain.Models;
using FluentAssertions;

namespace CarLoan.Application.Tests;

public class LoanRequestMapperTests
{
    private const RequestedCarCondition UnsupportedCarCondition = (RequestedCarCondition)999;
    private const RequestedVehicleCategory UnsupportedVehicleCategory = (RequestedVehicleCategory)999;

    private static LoanRequest CreateRequest(
        decimal purchasePrice = 2_000_000m,
        decimal downPayment = 500_000m,
        int loanPeriodInMonths = 60,
        RequestedCarCondition carCondition = RequestedCarCondition.New,
        RequestedVehicleCategory vehicleCategory = RequestedVehicleCategory.PetrolOrDiesel,
        int carAgeInYears = 0) =>
        new(purchasePrice, downPayment, loanPeriodInMonths, carCondition, vehicleCategory, carAgeInYears);

    [Fact]
    public void ToLoan_ThrowsArgumentNullException_WhenRequestIsNull()
    {
        Assert.Throws<ArgumentNullException>(() => LoanRequestMapper.ToLoan(null!));
    }

    [Fact]
    public void ToLoan_ThrowsArgumentOutOfRangeExceptionWithMessage_WhenCarConditionIsUnsupported()
    {
        var request = CreateRequest(carCondition: UnsupportedCarCondition);

        var exception = Assert.Throws<ArgumentOutOfRangeException>(() => LoanRequestMapper.ToLoan(request));

        Assert.Contains("Unsupported car condition.", exception.Message);
    }

    [Fact]
    public void ToLoan_ThrowsArgumentOutOfRangeExceptionWithMessage_WhenVehicleCategoryIsUnsupported()
    {
        var request = CreateRequest(vehicleCategory: UnsupportedVehicleCategory);

        var exception = Assert.Throws<ArgumentOutOfRangeException>(() => LoanRequestMapper.ToLoan(request));

        Assert.Contains("Unsupported vehicle category.", exception.Message);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-2_000_000)]
    public void ToLoan_ThrowsArgumentOutOfRangeException_WhenPurchasePriceIsZeroOrNegative(decimal purchasePrice)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => LoanRequestMapper.ToLoan(CreateRequest(purchasePrice: purchasePrice)));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-60)]
    public void ToLoan_ThrowsArgumentOutOfRangeException_WhenLoanPeriodIsZeroOrNegative(int loanPeriodInMonths)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => LoanRequestMapper.ToLoan(CreateRequest(loanPeriodInMonths: loanPeriodInMonths)));
    }

    [Fact]
    public void ToLoan_ThrowsArgumentOutOfRangeException_WhenDownPaymentIsNegative()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => LoanRequestMapper.ToLoan(CreateRequest(downPayment: -500_000m)));
    }

    [Fact]
    public void ToLoan_ThrowsArgumentOutOfRangeException_WhenCarAgeIsNegative()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => LoanRequestMapper.ToLoan(CreateRequest(carAgeInYears: -1)));
    }

    [Fact]
    public void ToLoan_MapsRequestToLoan_WhenLoanRequestProvided()
    {
        var request = CreateRequest(carCondition: RequestedCarCondition.Used, carAgeInYears: 4);

        var loan = LoanRequestMapper.ToLoan(request);

        loan.Should().BeEquivalentTo(
            new Loan(
                new LoanTerms(2_000_000m, 500_000m, 60, 0m),
                new Car(CarCondition.Used, VehicleCategory.PetrolOrDiesel, 4)));
    }

    [Fact]
    public void ToLoan_MapsNewCarCondition_WhenNewConditionRequested()
    {
        var loan = LoanRequestMapper.ToLoan(CreateRequest(carCondition: RequestedCarCondition.New));

        loan.Car.Condition.Should().Be(CarCondition.New);
    }

    [Theory]
    [InlineData(RequestedVehicleCategory.PetrolOrDiesel, VehicleCategory.PetrolOrDiesel)]
    [InlineData(RequestedVehicleCategory.PlugInHybrid, VehicleCategory.PlugInHybrid)]
    [InlineData(RequestedVehicleCategory.ElectricOrHydrogen, VehicleCategory.ElectricOrHydrogen)]
    public void ToLoan_MapsVehicleCategory_WhenVehicleCategoryRequested(
        RequestedVehicleCategory requested, VehicleCategory expected)
    {
        var loan = LoanRequestMapper.ToLoan(CreateRequest(vehicleCategory: requested));

        loan.Car.Category.Should().Be(expected);
    }
}
