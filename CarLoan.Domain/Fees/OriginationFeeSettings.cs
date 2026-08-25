using CarLoan.Domain.Guards;

namespace CarLoan.Domain.Fees;

/// <summary>
/// A lender's origination fee schedule and its discounts.
/// </summary>
/// <param name="Tiers">The fee rate bands, keyed on contract length in months.</param>
/// <param name="MinimumFee">Floor applied after the discounts.</param>
/// <param name="GreenFeeDiscountPercentage">Percentage taken off the fee amount for green vehicles.</param>
/// <param name="PlugInHybridRateDiscount">Percentage points taken off the fee rate for plug-in hybrids.</param>
public sealed record OriginationFeeSettings(
    IReadOnlyList<FeeTier> Tiers,
    decimal MinimumFee,
    decimal GreenFeeDiscountPercentage,
    decimal PlugInHybridRateDiscount)
{
    public IReadOnlyList<FeeTier> Tiers { get; } = ValidateAndSort(Tiers);
    public decimal MinimumFee { get; } = Guard.NonNegative(MinimumFee, nameof(MinimumFee));
    public decimal GreenFeeDiscountPercentage { get; } = Guard.InRange(GreenFeeDiscountPercentage, 0m, 100m, nameof(GreenFeeDiscountPercentage));
    public decimal PlugInHybridRateDiscount { get; } = Guard.NonNegative(PlugInHybridRateDiscount, nameof(PlugInHybridRateDiscount));

    /// <summary>
    /// Returns the fee rate for the given contract length. Lengths above the top band clamp to
    /// it — such a term has already failed the term rules, so the fee only ever prices a
    /// loan that was rejected anyway.
    /// </summary>
    public decimal FeeRateFor(int contractMonths)
    {
        foreach (var tier in Tiers)
        {
            if (contractMonths <= tier.MaximumContractMonths)
            {
                return tier.FeeRate;
            }
        }

        return Tiers[^1].FeeRate;
    }

    private static IReadOnlyList<FeeTier> ValidateAndSort(IReadOnlyList<FeeTier> tiers)
    {
        Guard.NoNullElements(tiers, nameof(tiers));

        if (tiers.Count == 0)
        {
            throw new ArgumentException("Fee tier list must not be empty.", nameof(tiers));
        }

        if (tiers.GroupBy(tier => tier.MaximumContractMonths).Any(group => group.Count() > 1))
        {
            throw new ArgumentException("Fee tier list must not contain duplicate contract lengths.", nameof(tiers));
        }

        return [.. tiers.OrderBy(tier => tier.MaximumContractMonths)];
    }
}
