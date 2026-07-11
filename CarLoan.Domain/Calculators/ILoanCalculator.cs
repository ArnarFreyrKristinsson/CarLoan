using CarLoan.Domain.Models;

namespace CarLoan.Domain.Calculators;

public interface ILoanCalculator
{
    decimal CalculateMonthlyPayment(LoanTerms loanTerms);
}
