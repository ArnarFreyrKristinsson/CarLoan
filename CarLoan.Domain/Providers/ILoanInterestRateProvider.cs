using CarLoan.Domain.Models;

namespace CarLoan.Domain.Providers;

public interface ILoanInterestRateProvider
{
    decimal GetInterestRate(ILoanTerms loanTerms);
}
