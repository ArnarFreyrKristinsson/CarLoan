using CarLoan.Application.Requests;

namespace CarLoan.Application;

public interface IMultiLenderLoanApplicationService
{
    IReadOnlyList<LenderLoanEvaluationResult> EvaluateLoanRequest(LoanRequest request);
}
