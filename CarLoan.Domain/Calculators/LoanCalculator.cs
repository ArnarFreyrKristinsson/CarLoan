using CarLoan.Domain.Models;

namespace CarLoan.Domain.Calculators;

public class LoanCalculator() : ILoanCalculator
{
    public decimal CalculateMonthlyPayment(LoanTerms loanTerms)
    {
        ArgumentNullException.ThrowIfNull(loanTerms);

        if (loanTerms.LoanPeriodInMonths <= 0)
            throw new ArgumentOutOfRangeException(nameof(loanTerms), "Loan period must be greater than zero.");

        decimal monthlyRate = loanTerms.InterestRate / 100m / 12m;
        decimal compoundFactor = DecimalPow(1 + monthlyRate, loanTerms.LoanPeriodInMonths);

        return Math.Round(loanTerms.LoanAmount * monthlyRate * compoundFactor / (compoundFactor - 1), 2);
    }

    internal static decimal DecimalPow(decimal baseValue, int exponent) =>
        Enumerable.Repeat(baseValue, exponent).Aggregate(1m, (acc, x) => acc * x);
}