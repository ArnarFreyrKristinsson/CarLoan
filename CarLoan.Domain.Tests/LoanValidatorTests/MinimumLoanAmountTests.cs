using CarLoan.Domain.Models;
using CarLoan.Domain.Validators;
using Xunit;

namespace CarLoan.Domain.Tests.LoanValidatorTests;

public class MinimumLoanAmountTests
{
    private readonly LoanTerms _defaultLoanTerms = new(2000000m, 1000000m, 84, 10.35m);
    private readonly Car _defaultCar = new(CarCondition.New);
    private readonly MinimumLoanAmountValidator _validator = new();

    [Fact]
    public void Evaluate_ThrowsArgumentNullException_WhenLoanIsNull()
    {
        Assert.Throws<ArgumentNullException>(() => _validator.Evaluate(null!));
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
}
