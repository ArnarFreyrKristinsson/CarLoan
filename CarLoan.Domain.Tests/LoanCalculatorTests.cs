using Xunit;

namespace CarLoan.Domain.Tests;

public class LoanCalculatorTests
{
    private readonly LoanTerms _loanTerms;
    private readonly LoanCalculator _loanCalculator;

    public LoanCalculatorTests()
    {
        _loanTerms = new(750000m, 2000000m, 500000m, 10.60m, 6, 75m);
        _loanCalculator = new(_loanTerms);
    }

    [Fact]
    public void CalculateMonthlyPayment_CorrectAmount_WhenLoanTermsProvided()
    {
        Assert.Equal(257785.81m, _loanCalculator.CalculateMonthlyPayment());
    }
}