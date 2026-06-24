using CarLoan.Domain.Calculators;

namespace CarLoan.Application;

public class LoanApplicationService(ILoanCalculator loanCalculator) : ILoanApplicationService
{
    public decimal GetMonthlyPayment() => loanCalculator.CalculateMonthlyPayment();
}
