using CarLoan.Domain.Models;
using Xunit;

namespace CarLoan.Domain.Tests;

public class LoanTermsTests
{
    [Fact]
    public void LoanAmount_CorrectValue_WhenLoanTermsProvided()
    {
        var loanTerms = new LoanTerms(7500m, 20000m, 5000m, 7, 90);

        Assert.Equal(15000, loanTerms.LoanAmount);
    }

    [Fact]
    public void InterestRate_CorrectValue_WhenLoanTermsProvided()
    {
        var loanTerms = new LoanTerms(7500m, 2000000m, 5000m, 7, 90);

        Assert.Equal(12.20m, loanTerms.InterestRate);
    }
}