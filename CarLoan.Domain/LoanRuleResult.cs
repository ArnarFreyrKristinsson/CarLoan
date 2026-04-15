namespace CarLoan.Domain;

public sealed record LoanRuleResult(bool IsValid, string RuleName, string? ErrorMessage);
