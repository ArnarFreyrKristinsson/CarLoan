namespace CarLoan.Domain.Models;

/// <summary>
/// The limits and values a failed rule reports, so the UI can state the reason in numbers.
/// </summary>
public enum LoanRuleParameter
{
    Minimum,
    Maximum,
    MaximumLoanPeriodMonths,
    LoanRatioThreshold,
    MaximumCombinedYears,
    CombinedYears
}
