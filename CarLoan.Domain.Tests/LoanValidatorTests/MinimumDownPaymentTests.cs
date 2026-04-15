using Xunit;

namespace CarLoan.Domain.Tests.LoanValidatorTests;

public class MinimumDownPaymentTests
{
    private readonly LoanTerms _defaultLoanTerms = new(750000m, 2000000m, 1000000m, 11.10m, 84, 90m);
    private readonly Car _defaultCar = new(CarCondition.New);
    private readonly MinimumDownPaymentValidator _validator = new();

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
}
