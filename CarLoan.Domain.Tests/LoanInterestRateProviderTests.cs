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
    public void GetInterestRate_CorrectValue_WhenDownPaymentProvided(decimal downPayment, decimal expectedRate)
    {
        Assert.Equal(expectedRate, _provider.GetInterestRate(downPayment));
    }
}
