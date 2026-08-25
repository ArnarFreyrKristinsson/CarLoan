using CarLoan.Domain.Guards;

namespace CarLoan.Domain.Validators;

/// <summary>
/// Caps on the car age and the term taken together. The applicable cap depends on which
/// side of the loan ratio threshold the loan falls.
/// </summary>
public sealed record CarAgeLimits(
    decimal LoanRatioThreshold,
    int MaximumCombinedYearsAboveThreshold,
    int MaximumCombinedYearsWithinThreshold)
{
    public decimal LoanRatioThreshold { get; } = Guard.Positive(LoanRatioThreshold, nameof(LoanRatioThreshold));
    public int MaximumCombinedYearsAboveThreshold { get; } = Guard.Positive(MaximumCombinedYearsAboveThreshold, nameof(MaximumCombinedYearsAboveThreshold));
    public int MaximumCombinedYearsWithinThreshold { get; } = Guard.Positive(MaximumCombinedYearsWithinThreshold, nameof(MaximumCombinedYearsWithinThreshold));

    public int MaximumCombinedYearsFor(decimal loanRatio) =>
        loanRatio > LoanRatioThreshold
            ? MaximumCombinedYearsAboveThreshold
            : MaximumCombinedYearsWithinThreshold;
}
