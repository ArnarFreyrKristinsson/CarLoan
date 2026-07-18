using CarLoan.Domain.Models;

namespace CarLoan.Application;

public interface ILoanApplicationService
{
    LoanEvaluationResult EvaluateLoan(Car car);
}
