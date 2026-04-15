using Xunit;

namespace CarLoan.Domain.Tests.LoanValidatorTests;

public class MinimumLoanAmountTests
{
    private readonly LoanTerms _defaultLoanTerms = new(750000m, 2000000m, 1000000m, 11.10m, 84, 90m);
    private readonly Car _defaultCar = new(CarCondition.New);
    private readonly MinimumLoanAmountValidator _validator = new();

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
}
