using CarLoan.Domain.Models;
using Xunit;

namespace CarLoan.Domain.Tests;

public class LoanTermsTests
{
    [Fact]
    public void LoanAmount_CorrectValue_WhenLoanTermsProvided()
    {
        var loanTerms = new LoanTerms(20000m, 5000m, 7, 10.35m);

        Assert.Equal(15000m, loanTerms.LoanAmount);
    }

    [Fact]
    public void LoanRatio_CorrectValue_WhenLoanTermsProvided()
    {
        var loanTerms = new LoanTerms(20000m, 5000m, 7, 10.35m);

        Assert.Equal(75m, loanTerms.LoanRatio);
    }

    [Fact]
    public void FinancedAmount_EqualsLoanAmount_WhenNoOriginationFeeApplied()
    {
        var loanTerms = new LoanTerms(20000m, 5000m, 7, 10.35m);

        Assert.Equal(15000m, loanTerms.FinancedAmount);
    }

    [Fact]
    public void FinancedAmount_AddsOriginationFeeOnTop_WhenOriginationFeeApplied()
    {
        var loanTerms = new LoanTerms(20000m, 5000m, 7, 10.35m, OriginationFee: 450m);

        Assert.Equal(15450m, loanTerms.FinancedAmount);
    }

    [Fact]
    public void LoanRatio_IgnoresOriginationFee_WhenOriginationFeeApplied()
    {
        var loanTerms = new LoanTerms(20000m, 5000m, 7, 10.35m, OriginationFee: 450m);

        Assert.Equal(75m, loanTerms.LoanRatio);
    }
}
