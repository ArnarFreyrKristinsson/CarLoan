using CarLoan.Domain.Models;
using CarLoan.Domain.Validators;
using Xunit;

namespace CarLoan.Domain.Tests.LoanValidatorTests;

public class MinimumDownPaymentTests
{
    private readonly LoanTerms _defaultLoanTerms = new(2000000m, 1000000m, 84, 10.35m);
    private readonly Car _defaultCar = new(CarCondition.New);
    private readonly MinimumDownPaymentValidator _validator = new();

    [Fact]
    public void Evaluate_ThrowsArgumentNullException_WhenLoanIsNull()
    {
        Assert.Throws<ArgumentNullException>(() => _validator.Evaluate(null!));
    }

    [Fact]
    public void Evaluate_IsNotValid_WhenDownPaymentLessThan150k()
    {
        var loanTerms = _defaultLoanTerms with { DownPayment = 100000m };
        var loan = new Loan(loanTerms, _defaultCar);

        var result = _validator.Evaluate(loan);

        Assert.False(result.IsValid);
        Assert.Equal("MinimumDownPayment", result.RuleName);
        Assert.NotNull(result.ErrorMessage);
    }

    [Fact]
    public void Evaluate_IsValid_WhenDownPaymentMoreThan150k()
    {
        var loan = new Loan(_defaultLoanTerms, _defaultCar);

        var result = _validator.Evaluate(loan);

        Assert.True(result.IsValid);
        Assert.Equal("MinimumDownPayment", result.RuleName);
        Assert.Null(result.ErrorMessage);
    }

    [Fact]
    public void Evaluate_ReturnsParametersWithMinimum_WhenDownPaymentLessThan150k()
    {
        var loanTerms = _defaultLoanTerms with { DownPayment = 100000m };
        var loan = new Loan(loanTerms, _defaultCar);

        var result = _validator.Evaluate(loan);

        Assert.NotNull(result.Parameters);
        Assert.Equal(150000m, result.Parameters["min"]);
    }

    [Fact]
    public void Evaluate_ReturnsNullParameters_WhenDownPaymentIsValid()
    {
        var loan = new Loan(_defaultLoanTerms, _defaultCar);

        var result = _validator.Evaluate(loan);

        Assert.Null(result.Parameters);
    }
}
