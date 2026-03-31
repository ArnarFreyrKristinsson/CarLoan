namespace CarLoan.Domain;

public class LoanCalculator(LoanTerms loanTerms)
{
    private readonly LoanTerms _loanTerms = loanTerms;

    public decimal CalculatePrincipal()
    {
        decimal loanAmount = _loanTerms.LoanAmount;
        decimal interestRateFraction = decimal.Round(_loanTerms.InterestRate / 100m, 2);
        decimal interests = interestRateFraction * loanAmount;
        decimal principal = loanAmount + interests;
        return principal;
    }
}