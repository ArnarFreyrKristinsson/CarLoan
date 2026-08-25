using CarLoan.Domain.Models;
using CarLoan.Domain.Validators;
using Xunit;

namespace CarLoan.Domain.Tests.LoanValidatorTests;

public class CarAgeTests
{
    private static readonly CarAgeLimits _defaultLimits = new(80m, 12, 20);
    private readonly CarAgeValidator _validator = new(_defaultLimits);

    // Builds a loan at the requested ratio on a 2,000,000 purchase price.
    private static Loan CreateLoan(decimal loanRatio, int loanPeriodInMonths, int carAgeInYears, CarCondition condition = CarCondition.Used)
    {
        const decimal purchasePrice = 2_000_000m;
        var terms = new LoanTerms(purchasePrice, purchasePrice - (purchasePrice * loanRatio / 100m), loanPeriodInMonths, 10.35m);
        return new Loan(terms, new Car(condition, VehicleCategory.PetrolOrDiesel, carAgeInYears));
    }

    [Fact]
    public void Evaluate_ThrowsArgumentNullException_WhenLoanIsNull()
    {
        Assert.Throws<ArgumentNullException>(() => _validator.Evaluate(null!));
    }

    [Fact]
    public void Constructor_ThrowsArgumentNullException_WhenLimitsIsNull()
    {
        Assert.Throws<ArgumentNullException>(() => new CarAgeValidator(null!));
    }

    [Theory]
    [InlineData(25, 84, 20)]
    [InlineData(90, 84, 12)]
    public void Evaluate_IsValid_WhenCarIsNew(decimal loanRatio, int loanPeriodInMonths, int carAgeInYears)
    {
        var result = _validator.Evaluate(CreateLoan(loanRatio, loanPeriodInMonths, carAgeInYears, CarCondition.New));

        Assert.True(result.IsValid);
        Assert.Equal("CarAge", result.RuleName);
        Assert.Null(result.ErrorMessage);
    }

    [Theory]
    [InlineData(85, 84, 5)]
    [InlineData(85, 72, 6)]
    [InlineData(90, 84, 1)]
    [InlineData(80.01, 84, 5)]
    public void Evaluate_IsValid_WhenCombinedYearsWithinHighRatioLimit(decimal loanRatio, int loanPeriodInMonths, int carAgeInYears)
    {
        var result = _validator.Evaluate(CreateLoan(loanRatio, loanPeriodInMonths, carAgeInYears));

        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData(85, 84, 6)]
    [InlineData(90, 72, 7)]
    public void Evaluate_IsNotValid_WhenCombinedYearsExceedHighRatioLimit(decimal loanRatio, int loanPeriodInMonths, int carAgeInYears)
    {
        var result = _validator.Evaluate(CreateLoan(loanRatio, loanPeriodInMonths, carAgeInYears));

        Assert.False(result.IsValid);
        Assert.Equal("CarAge", result.RuleName);
        Assert.NotNull(result.ErrorMessage);
    }

    [Theory]
    [InlineData(80, 84, 13)]
    [InlineData(50, 84, 13)]
    [InlineData(80, 72, 14)]
    public void Evaluate_IsValid_WhenCombinedYearsWithinStandardLimit(decimal loanRatio, int loanPeriodInMonths, int carAgeInYears)
    {
        var result = _validator.Evaluate(CreateLoan(loanRatio, loanPeriodInMonths, carAgeInYears));

        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData(80, 84, 14)]
    [InlineData(50, 84, 20)]
    public void Evaluate_IsNotValid_WhenCombinedYearsExceedStandardLimit(decimal loanRatio, int loanPeriodInMonths, int carAgeInYears)
    {
        var result = _validator.Evaluate(CreateLoan(loanRatio, loanPeriodInMonths, carAgeInYears));

        Assert.False(result.IsValid);
    }

    [Fact]
    public void Evaluate_UsesFractionalTermYears_WhenLoanPeriodIsNotWholeYears()
    {
        // 30 months is 2.5 years; 9 + 2.5 = 11.5, within the high-ratio limit of 12.
        var result = _validator.Evaluate(CreateLoan(85m, 30, 9));

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Evaluate_ReturnsHighRatioLimitParameters_WhenHighRatioLimitExceeded()
    {
        var result = _validator.Evaluate(CreateLoan(85m, 84, 6));

        Assert.NotNull(result.Parameters);
        Assert.Equal(12, result.Parameters["maxCombinedYears"]);
        Assert.Equal(80m, result.Parameters["ratioThreshold"]);
    }

    [Fact]
    public void Evaluate_ReturnsStandardLimitParameters_WhenStandardLimitExceeded()
    {
        var result = _validator.Evaluate(CreateLoan(50m, 84, 20));

        Assert.NotNull(result.Parameters);
        Assert.Equal(20, result.Parameters["maxCombinedYears"]);
    }

    [Fact]
    public void Evaluate_ReturnsNullParameters_WhenCarAgeIsValid()
    {
        var result = _validator.Evaluate(CreateLoan(50m, 84, 5));

        Assert.Null(result.Parameters);
    }
}
