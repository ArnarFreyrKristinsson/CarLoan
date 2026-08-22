using CarLoan.Domain.Models;

namespace CarLoan.Application;

public sealed record LenderLoanEvaluationResult(
    string LenderName,
    IReadOnlyList<LoanRuleResult> ValidationResults,
    decimal MonthlyPayment);
