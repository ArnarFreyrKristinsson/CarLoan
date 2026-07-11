using CarLoan.Domain.Models;
using CarLoan.Domain.Validators;
using Xunit;

namespace CarLoan.Domain.Tests.LoanValidatorTests;

public class MinimumLoanPeriodTests
{
    private readonly LoanTerms _defaultLoanTerms = new(2000000m, 1000000m, 84, 10.35m);
    private readonly Car _defaultCar = new(CarCondition.New);
    private readonly MinimumLoanPeriodValidator _validator = new();

    [Fact]
    public void Evaluate_ThrowsArgumentNullException_WhenLoanIsNull()
    {
        Assert.Throws<ArgumentNullException>(() => _validator.Evaluate(null!));
    }

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

    [Fact]
    public void Evaluate_ReturnsParametersWithMinimum_WhenLoanPeriodLessThan6Months()
    {
        var loanTerms = _defaultLoanTerms with { LoanPeriodInMonths = 4 };
        var loan = new Loan(loanTerms, _defaultCar);

        var result = _validator.Evaluate(loan);

        Assert.NotNull(result.Parameters);
        Assert.Equal(6, result.Parameters["min"]);
    }

    [Fact]
    public void Evaluate_ReturnsNullParameters_WhenLoanPeriodIsValid()
    {
        var loan = new Loan(_defaultLoanTerms, _defaultCar);

        var result = _validator.Evaluate(loan);

        Assert.Null(result.Parameters);
    }
}
