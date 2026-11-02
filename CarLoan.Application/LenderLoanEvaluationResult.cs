using CarLoan.Domain.Fees;
using CarLoan.Domain.Models;

namespace CarLoan.Application;

/// <summary>
/// One lender's answer. A price is null exactly when the lender's schedule has none for the
/// loan, and <see cref="PricingGaps"/> says why.
/// </summary>
public sealed record LenderLoanEvaluationResult(
    string LenderName,
    IReadOnlyList<LoanRuleResult> ValidationResults,
    decimal? MonthlyPayment,
    decimal? InterestRate,
    OriginationFee? OriginationFee,
    decimal? FinancedAmount,
    decimal? TotalInterest,
    IReadOnlyList<PricingGap> PricingGaps,
    int? MaximumLoanPeriodInMonths)
{
    /// <summary>Every rule passed.</summary>
    public bool IsAccepted => ValidationResults.All(rule => rule.IsValid);
}
