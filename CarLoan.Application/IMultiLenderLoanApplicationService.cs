using CarLoan.Application.Requests;

namespace CarLoan.Application;

public interface IMultiLenderLoanApplicationService
{
    LoanEvaluationOutcome EvaluateLoanRequest(LoanRequest request);
}
