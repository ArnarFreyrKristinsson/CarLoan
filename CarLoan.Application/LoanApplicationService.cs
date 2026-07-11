using CarLoan.Domain.Calculators;
using CarLoan.Domain.Models;

namespace CarLoan.Application;

public class LoanApplicationService(ILoanCalculator loanCalculator, LoanTerms loanTerms) : ILoanApplicationService
{
    public decimal GetMonthlyPayment() => loanCalculator.CalculateMonthlyPayment(loanTerms);
}
