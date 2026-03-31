using Xunit;

namespace CarLoan.Domain.Tests;

public class LoanTermsTests
{
    [Fact]
    public void LoanAmount_CorrectValue_WhenLoanTermsProvided()
    {
        var loanTerms = new LoanTerms(7500m, 20000m, 5000m, 11.10m, 7, 90);

        Assert.Equal(15000, loanTerms.LoanAmount);
    }
}
