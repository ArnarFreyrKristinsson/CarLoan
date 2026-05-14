namespace CarLoan.Domain.Models;

public sealed record LoanRuleResult(bool IsValid, string RuleName, string? ErrorMessage);
