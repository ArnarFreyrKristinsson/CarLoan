using Xunit;

namespace CarLoan.Domain.Tests.LoanValidatorTests;

public class MinimumLoanPeriodTests
{
    private readonly LoanTerms _defaultLoanTerms = new(750000m, 2000000m, 1000000m, 11.10m, 84, 90m);
    private readonly Car _defaultCar = new(CarCondition.New);
    private readonly MinimumLoanPeriodValidator _validator = new();

    [Fact]
    public void Evaluate_IsNotValid_WhenLoanPeriodLessThan6Months()
    {
        var loanTerms = _defaultLoanTerms with { LoanPeriodInMonths = 4 };
        var loan = new Loan(loanTerms, _defaultCar);

        var result = _validator.Evaluate(loan);

        Assert.False(result.IsValid);
        Assert.Equal("MinimumLoanPeriod", result.RuleName);
        Assert.NotNull(result.ErrorMessage);
    }

    [Fact]
    public void Evaluate_IsValid_WhenLoanPeriodIsExactly6Months()
    {
        var loanTerms = _defaultLoanTerms with { LoanPeriodInMonths = 6 };
        var loan = new Loan(loanTerms, _defaultCar);

        var result = _validator.Evaluate(loan);

        Assert.True(result.IsValid);
        Assert.Equal("MinimumLoanPeriod", result.RuleName);
        Assert.Null(result.ErrorMessage);
    }

    [Fact]
    public void Evaluate_IsValid_WhenLoanPeriodMoreThan6Months()
    {
        var loan = new Loan(_defaultLoanTerms, _defaultCar);

        var result = _validator.Evaluate(loan);

        Assert.True(result.IsValid);
        Assert.Equal("MinimumLoanPeriod", result.RuleName);
        Assert.Null(result.ErrorMessage);
    }
}
