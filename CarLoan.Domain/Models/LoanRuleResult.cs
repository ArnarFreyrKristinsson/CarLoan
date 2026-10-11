namespace CarLoan.Domain.Models;

public sealed record LoanRuleResult(
    bool IsValid,
    LoanRuleCode Code,
    string? ErrorMessage = null,
    IReadOnlyDictionary<LoanRuleParameter, decimal>? Parameters = null)
{
    public static LoanRuleResult Create(
        LoanRuleCode code,
        bool isValid,
        string? failureMessage = null,
        IReadOnlyDictionary<LoanRuleParameter, decimal>? failureParams = null) =>
        new(
            isValid,
            code,
            isValid ? null : failureMessage,
            isValid ? null : failureParams);
}