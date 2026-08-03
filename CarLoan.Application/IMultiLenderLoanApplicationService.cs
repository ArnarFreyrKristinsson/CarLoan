using CarLoan.Application.Requests;
using CarLoan.Domain.Models;

namespace CarLoan.Application;

public interface IMultiLenderLoanApplicationService
{
    IReadOnlyList<LenderLoanEvaluationResult> EvaluateLoanRequest(LoanRequest request);
}
