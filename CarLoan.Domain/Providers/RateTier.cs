using CarLoan.Domain.Guards;

namespace CarLoan.Domain.Providers;

/// <summary>
/// One band of a rate table. Bands are inclusive at the top and exclusive at the bottom:
/// with bands of 50 and 70, a ratio of 70.0 falls in the 70 band and 70.01 in the next one up.
/// </summary>
public sealed record RateTier(decimal MaximumFinancingRatio, decimal InterestRate)
{
    public decimal MaximumFinancingRatio { get; } = Guard.Positive(MaximumFinancingRatio, nameof(MaximumFinancingRatio));

    public decimal InterestRate { get; } = Guard.Positive(InterestRate, nameof(InterestRate));
}
