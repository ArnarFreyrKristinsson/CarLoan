using Xunit;

namespace CarLoan.Domain.Tests.LoanValidatorTests;

public class MinimumLoanAmountTests
{
    private readonly LoanTerms _defaultLoanTerms = new(750000m, 2000000m, 1000000m, 11.10m, 84, 90m);

    [Fact]
    public void IsMinimumLoanAmountSatisfied_False_WhenLoanAmountLessThan750k()
    {
        var loanTerms = _defaultLoanTerms with { PurchasePrice = 800000m, DownPayment = 100000m };
        var loanValidator = new LoanValidator(loanTerms);

        Assert.False(loanValidator.IsMinimumLoanAmountSatisfied());
    }

    [Fact]
    public void IsMinimumLoanAmountSatisfied_True_WhenLoanAmountMoreThan750k()
    {
        var loanTerms = _defaultLoanTerms;
        var loanValidator = new LoanValidator(loanTerms);

        Assert.True(loanValidator.IsMinimumLoanAmountSatisfied());
    }
}
