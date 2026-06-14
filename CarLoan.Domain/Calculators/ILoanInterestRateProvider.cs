using CarLoan.Domain.Models;

namespace CarLoan.Domain.Calculators;

public interface ILoanInterestRateProvider
{
    decimal GetInterestRate(LoanTerms loanTerms);
}
