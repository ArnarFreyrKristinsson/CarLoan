using CarLoan.Domain.Models;

namespace CarLoan.Application;

public interface IMultiLenderLoanApplicationService
{
    IReadOnlyList<LenderLoanEvaluationResult> EvaluateLoan(Loan loan);
}
