using CarLoan.Domain.Guards;

namespace CarLoan.Domain.Providers;

/// <summary>
/// A rate schedule keyed on the financing ratio (LTV). Nothing else — not the down payment,
/// not the term — affects the rate.
/// </summary>
public sealed class RateTable(IReadOnlyList<RateTier> tiers, decimal defaultInterestRate)
{
    private readonly IReadOnlyList<RateTier> _tiers = ValidateAndSort(tiers);
    private readonly decimal _defaultInterestRate = Guard.Positive(defaultInterestRate, nameof(defaultInterestRate));

    /// <summary>
    /// Returns the rate for the given financing ratio, or the default rate when the ratio
    /// sits above every band. Ratios above the top band belong to loans the rules reject,
    /// so the default only ever prices a loan that has already failed validation.
    /// </summary>
    public decimal GetRate(decimal financingRatio)
    {
        foreach (var (maximumFinancingRatio, interestRate) in _tiers)
        {
            if (financingRatio <= maximumFinancingRatio)
            {
                return interestRate;
            }
        }

        return _defaultInterestRate;
    }

    private static IReadOnlyList<RateTier> ValidateAndSort(IReadOnlyList<RateTier> tiers)
    {
        Guard.NoNullElements(tiers, nameof(tiers));

        if (tiers.GroupBy(tier => tier.MaximumFinancingRatio).Any(group => group.Count() > 1))
        {
            throw new ArgumentException("Tier list must not contain duplicate maximum financing ratios.", nameof(tiers));
        }

        return [.. tiers.OrderBy(tier => tier.MaximumFinancingRatio)];
    }
}
