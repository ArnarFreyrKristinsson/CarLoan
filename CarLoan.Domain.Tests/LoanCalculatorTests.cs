using CarLoan.Domain.Calculators;
using CarLoan.Domain.Models;
using Xunit;

namespace CarLoan.Domain.Tests;

public class LoanCalculatorTests
{
    private readonly LoanTerms _loanTerms;
    private readonly LoanCalculator _loanCalculator;

    public LoanCalculatorTests()
    {
        _loanTerms = new(750000m, 2000000m, 500000m, 6, 75m);
        _loanCalculator = new LoanCalculator(_loanTerms);
    }

    [Fact]
    public void CalculateMonthlyPayment_CorrectAmount_WhenLoanTermsProvided()
    {
        Assert.Equal(258415.03m, _loanCalculator.CalculateMonthlyPayment());
    }
}