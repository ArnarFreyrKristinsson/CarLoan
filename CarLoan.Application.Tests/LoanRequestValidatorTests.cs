using CarLoan.Application.Mapping;
using CarLoan.Application.Requests;
using FluentAssertions;

namespace CarLoan.Application.Tests;

public class LoanRequestValidatorTests
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
    public void Validate_ThrowsArgumentNullException_WhenRequestIsNull()
    {
        Assert.Throws<ArgumentNullException>(() => LoanRequestValidator.Validate(null!));
    }

    [Fact]
    public void Validate_ReturnsNoErrors_WhenEveryFieldIsWellFormed()
    {
        LoanRequestValidator.Validate(CreateRequest()).Should().BeEmpty();
    }

    [Fact]
    public void Validate_ReturnsNoErrors_WhenOptionalAmountsAreZero()
    {
        var request = CreateRequest(downPayment: 0m, carAgeInYears: 0);

        LoanRequestValidator.Validate(request).Should().BeEmpty();
    }

    [Fact]
    public void Validate_ReturnsNoErrors_WhenPositiveFieldsAreAtTheirSmallestValue()
    {
        var request = CreateRequest(purchasePrice: 0.01m, downPayment: 0m, loanPeriodInMonths: 1);

        LoanRequestValidator.Validate(request).Should().BeEmpty();
    }

    public static TheoryData<LoanRequest> RequestsTheMapperRejects => new()
    {
        CreateRequest(purchasePrice: 0m),
        CreateRequest(downPayment: -1m),
        CreateRequest(loanPeriodInMonths: 0),
        CreateRequest(carCondition: UnsupportedCarCondition),
        CreateRequest(vehicleCategory: UnsupportedVehicleCategory),
        CreateRequest(carAgeInYears: -1)
    };

    [Theory]
    [MemberData(nameof(RequestsTheMapperRejects))]
    public void Validate_ReportsAnError_WhenMapperWouldRejectRequest(LoanRequest request)
    {
        Assert.ThrowsAny<ArgumentException>(() => LoanRequestMapper.ToLoan(request));

        LoanRequestValidator.Validate(request).Should().NotBeEmpty();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Validate_ReportsPurchasePriceMustBePositive_WhenPurchasePriceIsZeroOrNegative(decimal purchasePrice)
    {
        var errors = LoanRequestValidator.Validate(CreateRequest(purchasePrice: purchasePrice));

        errors.Should().BeEquivalentTo([new InputError(nameof(LoanRequest.PurchasePrice), InputErrorKind.MustBePositive)]);
    }

    [Fact]
    public void Validate_ReportsDownPaymentMustNotBeNegative_WhenDownPaymentIsNegative()
    {
        var errors = LoanRequestValidator.Validate(CreateRequest(downPayment: -1m));

        errors.Should().BeEquivalentTo([new InputError(nameof(LoanRequest.DownPayment), InputErrorKind.MustNotBeNegative)]);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Validate_ReportsLoanPeriodMustBePositive_WhenLoanPeriodIsZeroOrNegative(int loanPeriodInMonths)
    {
        var errors = LoanRequestValidator.Validate(CreateRequest(loanPeriodInMonths: loanPeriodInMonths));

        errors.Should().BeEquivalentTo([new InputError(nameof(LoanRequest.LoanPeriodInMonths), InputErrorKind.MustBePositive)]);
    }

    [Fact]
    public void Validate_ReportsCarConditionUnsupported_WhenCarConditionIsUndefined()
    {
        var errors = LoanRequestValidator.Validate(CreateRequest(carCondition: UnsupportedCarCondition));

        errors.Should().BeEquivalentTo([new InputError(nameof(LoanRequest.CarCondition), InputErrorKind.Unsupported)]);
    }

    [Fact]
    public void Validate_ReportsVehicleCategoryUnsupported_WhenVehicleCategoryIsUndefined()
    {
        var errors = LoanRequestValidator.Validate(CreateRequest(vehicleCategory: UnsupportedVehicleCategory));

        errors.Should().BeEquivalentTo([new InputError(nameof(LoanRequest.VehicleCategory), InputErrorKind.Unsupported)]);
    }

    [Fact]
    public void Validate_ReportsCarAgeMustNotBeNegative_WhenCarAgeIsNegative()
    {
        var errors = LoanRequestValidator.Validate(CreateRequest(carAgeInYears: -1));

        errors.Should().BeEquivalentTo([new InputError(nameof(LoanRequest.CarAgeInYears), InputErrorKind.MustNotBeNegative)]);
    }

    [Fact]
    public void Validate_ReturnsNoErrors_WhenFieldsAreAtTheirLargestValue()
    {
        var request = CreateRequest(purchasePrice: 1_000_000_000_000m, loanPeriodInMonths: 1_200);

        LoanRequestValidator.Validate(request).Should().BeEmpty();
    }

    [Fact]
    public void Validate_ReportsPurchasePriceTooLarge_WhenPurchasePriceIsAboveMaximum()
    {
        var errors = LoanRequestValidator.Validate(CreateRequest(purchasePrice: 1_000_000_000_000.01m));

        errors.Should().BeEquivalentTo([new InputError(nameof(LoanRequest.PurchasePrice), InputErrorKind.TooLarge)]);
    }

    [Fact]
    public void Validate_ReportsLoanPeriodTooLarge_WhenLoanPeriodIsAboveMaximum()
    {
        var errors = LoanRequestValidator.Validate(CreateRequest(loanPeriodInMonths: 1_201));

        errors.Should().BeEquivalentTo([new InputError(nameof(LoanRequest.LoanPeriodInMonths), InputErrorKind.TooLarge)]);
    }

    [Theory]
    [InlineData(2_000_000)]
    [InlineData(3_000_000)]
    public void Validate_ReportsDownPaymentMustBeLessThanPurchasePrice_WhenNothingIsLeftToFinance(decimal downPayment)
    {
        var errors = LoanRequestValidator.Validate(CreateRequest(purchasePrice: 2_000_000m, downPayment: downPayment));

        errors.Should().BeEquivalentTo(
            [new InputError(nameof(LoanRequest.DownPayment), InputErrorKind.MustBeLessThanPurchasePrice)]);
    }

    [Fact]
    public void Validate_ReturnsNoErrors_WhenDownPaymentIsJustBelowPurchasePrice()
    {
        var request = CreateRequest(purchasePrice: 2_000_000m, downPayment: 1_999_999.99m);

        LoanRequestValidator.Validate(request).Should().BeEmpty();
    }

    [Fact]
    public void Validate_ReportsOnlyPurchasePrice_WhenPurchasePriceIsNotPositiveAndDownPaymentIsZero()
    {
        var errors = LoanRequestValidator.Validate(CreateRequest(purchasePrice: 0m, downPayment: 0m));

        errors.Should().BeEquivalentTo([new InputError(nameof(LoanRequest.PurchasePrice), InputErrorKind.MustBePositive)]);
    }

    [Fact]
    public void Validate_ReportsEveryMalformedField_WhenSeveralFieldsAreMalformed()
    {
        var request = CreateRequest(purchasePrice: 0m, downPayment: -1m, carAgeInYears: -1);

        var errors = LoanRequestValidator.Validate(request);

        errors.Should().BeEquivalentTo(
        [
            new InputError(nameof(LoanRequest.PurchasePrice), InputErrorKind.MustBePositive),
            new InputError(nameof(LoanRequest.DownPayment), InputErrorKind.MustNotBeNegative),
            new InputError(nameof(LoanRequest.CarAgeInYears), InputErrorKind.MustNotBeNegative)
        ]);
    }
}
