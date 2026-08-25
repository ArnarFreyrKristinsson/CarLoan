using CarLoan.Domain.Models;
using CarLoan.Domain.Validators;
using Xunit;

namespace CarLoan.Domain.Tests.LoanValidatorTests;

public class MaximumLoanAmountTests
{
    private readonly LoanTerms _defaultLoanTerms = new(2_000_000m, 1_000_000m, 84, 10.35m);
    private readonly Car _defaultCar = new(CarCondition.New, VehicleCategory.PetrolOrDiesel, 0);
    private readonly MaximumLoanAmountValidator _validator = new(30_000_000m);

    [Fact]
    public void Evaluate_ThrowsArgumentNullException_WhenLoanIsNull()
    {
        Assert.Throws<ArgumentNullException>(() => _validator.Evaluate(null!));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-30_000_000)]
    public void Constructor_ThrowsArgumentOutOfRangeException_WhenMaximumLoanAmountIsZeroOrNegative(decimal maximumLoanAmount)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new MaximumLoanAmountValidator(maximumLoanAmount));
    }

    [Fact]
    public void Evaluate_IsNotValid_WhenLoanAmountAbove30M()
    {
        var loanTerms = _defaultLoanTerms with { PurchasePrice = 40_000_000m, DownPayment = 9_000_000m };
        var loan = new Loan(loanTerms, _defaultCar);

        var result = _validator.Evaluate(loan);

        Assert.False(result.IsValid);
        Assert.Equal("MaximumLoanAmount", result.RuleName);
        Assert.NotNull(result.ErrorMessage);
    }

    [Fact]
    public void Evaluate_IsValid_WhenLoanAmountExactly30M()
    {
        var loanTerms = _defaultLoanTerms with { PurchasePrice = 40_000_000m, DownPayment = 10_000_000m };
        var loan = new Loan(loanTerms, _defaultCar);

        var result = _validator.Evaluate(loan);

        Assert.True(result.IsValid);
        Assert.Equal("MaximumLoanAmount", result.RuleName);
        Assert.Null(result.ErrorMessage);
    }

    [Fact]
    public void Evaluate_IsValid_WhenLoanAmountBelow30M()
    {
        var loan = new Loan(_defaultLoanTerms, _defaultCar);

        var result = _validator.Evaluate(loan);

        Assert.True(result.IsValid);
        Assert.Null(result.Parameters);
    }

    [Fact]
    public void Evaluate_ReturnsParametersWithMaximum_WhenLoanAmountAbove30M()
    {
        var loanTerms = _defaultLoanTerms with { PurchasePrice = 40_000_000m, DownPayment = 9_000_000m };
        var loan = new Loan(loanTerms, _defaultCar);

        var result = _validator.Evaluate(loan);

        Assert.NotNull(result.Parameters);
        Assert.Equal(30_000_000m, result.Parameters["max"]);
    }

    [Fact]
    public void Evaluate_IsNotValid_WhenConfiguredMaximumIsLowerThanAllowedMaximum()
    {
        var validator = new MaximumLoanAmountValidator(500_000m);
        var loan = new Loan(_defaultLoanTerms, _defaultCar);

        var result = validator.Evaluate(loan);

        Assert.False(result.IsValid);
        Assert.Equal(500_000m, result.Parameters!["max"]);
    }
}
