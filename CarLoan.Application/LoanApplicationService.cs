using CarLoan.Domain.Calculators;
using CarLoan.Domain.Models;
using CarLoan.Domain.Providers;
using CarLoan.Domain.Validators;

namespace CarLoan.Application;

public class LoanApplicationService(
    ILoanCalculator loanCalculator,
    ILoanInterestRateProvider interestRateProvider,
    ILoanValidator loanValidator,
    LoanTerms loanTerms) : ILoanApplicationService
{
    private decimal GetMonthlyPaymentWithInterestRate() =>
        loanCalculator.CalculateMonthlyPayment(
            loanTerms with { InterestRate = interestRateProvider.GetInterestRate(loanTerms.DownPayment) });

    public LoanEvaluationResult EvaluateLoan(Car car) =>
        new(
            loanValidator.Validate(new Loan(loanTerms, car)),
            GetMonthlyPaymentWithInterestRate());
}