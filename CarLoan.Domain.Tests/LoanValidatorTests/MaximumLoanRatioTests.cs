using CarLoan.Domain.Models;
using CarLoan.Domain.Validators;
using FluentAssertions;
using Xunit;

namespace CarLoan.Domain.Tests.LoanValidatorTests;

public class MaximumLoanRatioTests
{
    private readonly MaximumLoanRatioValidator _validator = new(90m);

    private static Loan CreateLoan(decimal downPayment, CarCondition condition = CarCondition.New) =>
        new(new LoanTerms(2_000_000m, downPayment, 60, 10.35m), new Car(condition, VehicleCategory.PetrolOrDiesel, 0));

    [Fact]
    public void Evaluate_ThrowsArgumentNullException_WhenLoanIsNull()
    {
        Assert.Throws<ArgumentNullException>(() => _validator.Evaluate(null!));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-90)]
    public void Constructor_ThrowsArgumentOutOfRangeException_WhenMaximumLoanRatioIsZeroOrNegative(decimal maximumLoanRatio)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new MaximumLoanRatioValidator(maximumLoanRatio));
    }

    [Theory]
    [InlineData(200_000, CarCondition.New)]
    [InlineData(200_000, CarCondition.Used)]
    [InlineData(1_000_000, CarCondition.New)]
    public void Evaluate_IsValid_WhenLoanRatioIsAtOrBelowMaximum(decimal downPayment, CarCondition condition)
    {
        var result = _validator.Evaluate(CreateLoan(downPayment, condition));

        Assert.True(result.IsValid);
        Assert.Equal(LoanRuleCode.MaximumLoanRatio, result.Code);
        Assert.Null(result.ErrorMessage);
        Assert.Null(result.Parameters);
    }

    [Theory]
    [InlineData(199_999.99, CarCondition.New)]
    [InlineData(199_999.99, CarCondition.Used)]
    [InlineData(100_000, CarCondition.New)]
    public void Evaluate_IsNotValid_WhenLoanRatioIsAboveMaximum(decimal downPayment, CarCondition condition)
    {
        var result = _validator.Evaluate(CreateLoan(downPayment, condition));

        Assert.False(result.IsValid);
        Assert.Equal(LoanRuleCode.MaximumLoanRatio, result.Code);
        Assert.False(string.IsNullOrWhiteSpace(result.ErrorMessage));
    }

    [Fact]
    public void Evaluate_ReturnsMaximumParameter_WhenLoanRatioIsAboveMaximum()
    {
        var result = _validator.Evaluate(CreateLoan(100_000m));

        result.Parameters.Should().BeEquivalentTo(new Dictionary<LoanRuleParameter, decimal>
        {
            [LoanRuleParameter.Maximum] = 90m
        });
    }

    [Fact]
    public void Evaluate_IsNotValid_WhenLoanRatioExceedsConfiguredLimit()
    {
        var validator = new MaximumLoanRatioValidator(80m);

        var result = validator.Evaluate(CreateLoan(300_000m));

        Assert.False(result.IsValid);
        result.Parameters.Should().BeEquivalentTo(new Dictionary<LoanRuleParameter, decimal>
        {
            [LoanRuleParameter.Maximum] = 80m
        });
    }
}
