using CarLoan.Domain.Models;

namespace CarLoan.Application;

public sealed record LoanEvaluationResult(
    IReadOnlyList<LoanRuleResult> ValidationResults,
    decimal MonthlyPayment);
