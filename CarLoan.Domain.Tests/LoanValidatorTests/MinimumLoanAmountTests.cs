using CarLoan.Domain.Models;
using CarLoan.Domain.Validators;
using Xunit;

namespace CarLoan.Domain.Tests.LoanValidatorTests;

public class MinimumLoanAmountTests
{
    private readonly LoanTerms _defaultLoanTerms = new(2000000m, 1000000m, 84, 10.35m);
    private readonly Car _defaultCar = new(CarCondition.New);
    private readonly MinimumLoanAmountValidator _validator = new(750000m);

    [Fact]
    public void Evaluate_ThrowsArgumentNullException_WhenLoanIsNull()
    {
        Assert.Throws<ArgumentNullException>(() => _validator.Evaluate(null!));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-750000)]
    public void Constructor_ThrowsArgumentOutOfRangeException_WhenMinimumLoanAmountIsZeroOrNegative(decimal minimumLoanAmount)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new MinimumLoanAmountValidator(minimumLoanAmount));
    }

    [Fact]
    public void Evaluate_IsNotValid_WhenLoanAmountLessThan750k()
    {
        var loanTerms = _defaultLoanTerms with { PurchasePrice = 800000m, DownPayment = 100000m };
        var loan = new Loan(loanTerms, _defaultCar);

        var result = _validator.Evaluate(loan);

        Assert.False(result.IsValid);
        Assert.Equal("MinimumLoanAmount", result.RuleName);
        Assert.NotNull(result.ErrorMessage);
    }

    [Fact]
    public void Evaluate_IsValid_WhenLoanAmountMoreThan750k()
    {
        var loan = new Loan(_defaultLoanTerms, _defaultCar);

        var result = _validator.Evaluate(loan);

        Assert.True(result.IsValid);
        Assert.Equal("MinimumLoanAmount", result.RuleName);
        Assert.Null(result.ErrorMessage);
    }

    [Fact]
    public void Evaluate_IsValid_WhenLoanAmountExactly750k()
    {
        var loanTerms = _defaultLoanTerms with { PurchasePrice = 900000m, DownPayment = 150000m };
        var loan = new Loan(loanTerms, _defaultCar);

        var result = _validator.Evaluate(loan);

        Assert.True(result.IsValid);
        Assert.Equal("MinimumLoanAmount", result.RuleName);
        Assert.Null(result.ErrorMessage);
    }

    [Fact]
    public void Evaluate_ReturnsParametersWithMinimum_WhenLoanAmountLessThan750k()
    {
        var loanTerms = _defaultLoanTerms with { PurchasePrice = 800000m, DownPayment = 100000m };
        var loan = new Loan(loanTerms, _defaultCar);

        var result = _validator.Evaluate(loan);

        Assert.NotNull(result.Parameters);
        Assert.Equal(750000m, result.Parameters["min"]);
    }

    [Fact]
    public void Evaluate_ReturnsNullParameters_WhenLoanAmountIsValid()
    {
        var loan = new Loan(_defaultLoanTerms, _defaultCar);

        var result = _validator.Evaluate(loan);

        Assert.Null(result.Parameters);
    }

    [Fact]
    public void Evaluate_IsNotValid_WhenConfiguredMinimumIsHigherThanAllowedMinimum()
    {
        var validator = new MinimumLoanAmountValidator(1500000m);
        var loan = new Loan(_defaultLoanTerms, _defaultCar);

        var result = validator.Evaluate(loan);

        Assert.False(result.IsValid);
        Assert.NotNull(result.Parameters);
        Assert.Equal(1500000m, result.Parameters["min"]);
    }
}
