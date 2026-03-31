using Xunit;

namespace CarLoan.Domain.Tests.LoanValidatorTests;

public class MinimumLoanPeriodTests
{
    private readonly LoanTerms _defaultLoanTerms = new(750000m, 2000000m, 1000000m, 11.10m, 84, 90m);

    [Fact]
    public void IsMinimumLoanPeriodSatisfied_False_WhenLoanPeriodLessThan6Months()
    {
        var loanTerms = _defaultLoanTerms with { LoanPeriodInMonths = 4 };
        var loanValidator = new LoanValidator(loanTerms);

        Assert.False(loanValidator.IsMinimumLoanPeriodSatisfied());
    }

    [Fact]
    public void IsMinimumLoanPeriodSatisfied_True_WhenLoanPeriodIsExactly6Months()
    {
        var loanTerms = _defaultLoanTerms with { LoanPeriodInMonths = 6 };
        var loanValidator = new LoanValidator(loanTerms);

        Assert.True(loanValidator.IsMinimumLoanPeriodSatisfied());
    }

    [Fact]
    public void IsMinimumLoanPeriodSatisfied_True_WhenLoanPeriodMoreThan6Months()
    {
        var loanTerms = _defaultLoanTerms;
        var loanValidator = new LoanValidator(loanTerms);

        Assert.True(loanValidator.IsMinimumLoanPeriodSatisfied());
    }
}
