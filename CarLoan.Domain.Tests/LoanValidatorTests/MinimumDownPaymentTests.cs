using Xunit;

namespace CarLoan.Domain.Tests.LoanValidatorTests;

public class MinimumDownPaymentTests
{
    private readonly LoanTerms _defaultLoanTerms = new(750000m, 2000000m, 1000000m, 11.10m, 84, 90m);

    [Fact]
    public void IsMinimumDownPaymentSatisfied_False_WhenDownPaymentLessThan150k()
    {
        var loanTerms = _defaultLoanTerms with { DownPayment = 100000m };
        var loanValidator = new LoanValidator(loanTerms);

        Assert.False(loanValidator.IsMinimumDownPaymentSatisfied());
    }

    [Fact]
    public void IsMinimumDownPaymentSatisfied_True_WhenDownPaymentMoreThan150k()
    {
        var loanTerms = _defaultLoanTerms;
        var loanValidator = new LoanValidator(loanTerms);

        Assert.True(loanValidator.IsMinimumDownPaymentSatisfied());
    }
}
