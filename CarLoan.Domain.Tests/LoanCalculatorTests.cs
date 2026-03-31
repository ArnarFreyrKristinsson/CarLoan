using Xunit;

namespace CarLoan.Domain.Tests;

public class LoanCalculatorTests
{
    private readonly LoanTerms _loanTerms;
    private readonly LoanCalculator _loanCalculator;

    public LoanCalculatorTests()
    {
        _loanTerms = new(7500m, 20000m, 5000m, 11.10m, 7, 90);
        _loanCalculator = new(_loanTerms);
    }

    [Fact]
    public void CalculatePrincipal_CorrectAmount_WhenInterestRateIs11_10()
    {
        Assert.Equal(16650, _loanCalculator.CalculatePrincipal());
    }
}