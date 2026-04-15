namespace CarLoan.Domain;

public class LoanCalculator(LoanTerms loanTerms)
{
    public decimal CalculateMonthlyPayment()
    {
        decimal monthlyRate = loanTerms.InterestRate / 100m / 12m;
        decimal compoundFactor = DecimalPow(1 + monthlyRate, loanTerms.LoanPeriodInMonths);

        return Math.Round(loanTerms.LoanAmount * monthlyRate * compoundFactor / (compoundFactor - 1), 2);
    }

    private static decimal DecimalPow(decimal baseValue, int exponent) =>
        (decimal)Math.Pow((double)baseValue, exponent);
}