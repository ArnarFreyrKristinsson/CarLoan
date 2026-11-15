using CarLoan.Domain.Guards;
using CarLoan.Domain.Models;

namespace CarLoan.Domain.Providers;

/// <summary>A rate schedule keyed on the financing ratio (LTV).</summary>
public sealed class RateTable(IReadOnlyList<RateTier> tiers)
{
    private readonly IReadOnlyList<RateTier> _tiers = ValidateAndSort(tiers);

    /// <summary>
    /// Returns the rate of the band that covers the financing ratio, or null when the ratio sits
    /// above every band: the schedule has no rate for it.
    /// </summary>
    public decimal? GetRate(decimal financingRatio)
    {
        foreach (var (maximumFinancingRatio, interestRate) in _tiers)
        {
            if (financingRatio <= maximumFinancingRatio)
            {
                return interestRate;
            }
        }

        return null;
    }

    /// <summary>
    /// Why <see cref="GetRate"/> has no rate for the financing ratio: the range the bands cover.
    /// Null when it has one.
    /// </summary>
    public PricingGap? FindGap(decimal financingRatio) =>
        GetRate(financingRatio) is null
            ? new PricingGap(PricingGapCode.InterestRate, 0m, _tiers[^1].MaximumFinancingRatio)
            : null;

    private static IReadOnlyList<RateTier> ValidateAndSort(IReadOnlyList<RateTier> tiers)
    {
        Guard.NoNullElements(tiers, nameof(tiers));

        if (tiers.Count == 0)
        {
            throw new ArgumentException("Tier list must not be empty.", nameof(tiers));
        }

        if (tiers.GroupBy(tier => tier.MaximumFinancingRatio).Any(group => group.Count() > 1))
        {
            throw new ArgumentException("Tier list must not contain duplicate maximum financing ratios.", nameof(tiers));
        }

        return [.. tiers.OrderBy(tier => tier.MaximumFinancingRatio)];
    }
}
