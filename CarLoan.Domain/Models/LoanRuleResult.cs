namespace CarLoan.Domain.Models;

public sealed record LoanRuleResult(
    bool IsValid,
    string RuleName,
    string? ErrorMessage = null,
    IReadOnlyDictionary<string, object>? Parameters = null)
{
    public static LoanRuleResult Create(
        string ruleName,
        bool isValid,
        string? failureMessage = null,
        IReadOnlyDictionary<string, object>? failureParams = null) =>
        new(
            isValid,
            ruleName,
            isValid ? null : failureMessage,
            isValid ? null : failureParams);
}