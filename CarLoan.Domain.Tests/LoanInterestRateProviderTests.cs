using CarLoan.Domain.Models;
using CarLoan.Domain.Providers;
using Xunit;

namespace CarLoan.Domain.Tests;

public class LoanInterestRateProviderTests
{
    private readonly LoanInterestRateProvider _provider = new();

    [Theory]
    [InlineData(1000000, 10.35)]
    [InlineData(600000, 11.20)]
    [InlineData(400000, 11.45)]
    [InlineData(200000, 12.20)]
    public void GetInterestRate_CorrectValue_WhenLoanTermsProvided(decimal downPayment, decimal expectedRate)
    {
        var loanTerms = new LoanTerms(750000m, 2000000m, downPayment, 36, 75m);

        Assert.Equal(expectedRate, _provider.GetInterestRate(loanTerms));
    }
}
