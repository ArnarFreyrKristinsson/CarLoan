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
        _loanTerms = new(2000000m, 500000m, 6, 11.45m);
        _loanCalculator = new LoanCalculator();
    }

    [Fact]
    public void LoanCalculator_ThrowsArgumentNullException_WhenLoanTermsIsNull()
    {
        Assert.Throws<ArgumentNullException>(() => new LoanCalculator().CalculateMonthlyPayment(null!));
    }

    [Fact]
    public void CalculateMonthlyPayment_ThrowsArgumentOutOfRangeException_WhenLoanPeriodIsZero()
    {
        var loanTerms = new LoanTerms(2000000m, 500000m, 0, 11.45m);

        Assert.Throws<ArgumentOutOfRangeException>(() => new LoanCalculator().CalculateMonthlyPayment(loanTerms));
    }

    [Theory]
    [InlineData(2, 0, 1)]
    [InlineData(2, 1, 2)]
    [InlineData(2, 3, 8)]
    [InlineData(10, 4, 10000)]
    [InlineData(1, 84, 1)]
    public void DecimalPow_ReturnsCorrectResult_WhenBaseAndExponentProvided(decimal baseValue, int exponent, decimal expected)
    {
        Assert.Equal(expected, LoanCalculator.DecimalPow(baseValue, exponent));
    }

    [Fact]
    public void CalculateMonthlyPayment_CorrectAmount_WhenLoanTermsProvided()
    {
        // Matches an independent recomputation of the standard formula.
        Assert.Equal(258415.03m, _loanCalculator.CalculateMonthlyPayment(_loanTerms));
    }
}