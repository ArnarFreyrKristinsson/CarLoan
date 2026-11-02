using CarLoan.Domain.Guards;
using CarLoan.Domain.Models;

namespace CarLoan.Domain.Fees;

/// <summary>
/// A lender's origination fee schedule and its discounts.
/// </summary>
public sealed record OriginationFeeSettings(
    IReadOnlyList<FeeTier> Tiers,
    decimal MinimumFee,
    decimal GreenFeeDiscountPercentage,
    decimal PlugInHybridRateDiscount,
    int MinimumContractMonths)
{
    public IReadOnlyList<FeeTier> Tiers { get; } = ValidateAndSort(Tiers, nameof(Tiers));
    public decimal MinimumFee { get; } = Guard.NonNegative(MinimumFee, nameof(MinimumFee));
    public decimal GreenFeeDiscountPercentage { get; } = Guard.InRange(GreenFeeDiscountPercentage, 0m, 100m, nameof(GreenFeeDiscountPercentage));
    public decimal PlugInHybridRateDiscount { get; } = Guard.NonNegative(PlugInHybridRateDiscount, nameof(PlugInHybridRateDiscount));
    public int MinimumContractMonths { get; } = ValidateMinimumContractMonths(MinimumContractMonths, Tiers, nameof(MinimumContractMonths));

    /// <summary>
    /// Returns the fee rate of the band that covers the contract length, or null when the length is
    /// shorter than <see cref="MinimumContractMonths"/> or longer than the top band: the schedule has
    /// no fee for it.
    /// </summary>
    public decimal? FeeRateFor(int contractMonths)
    {
        if (contractMonths < MinimumContractMonths)
        {
            return null;
        }

        foreach (var tier in Tiers)
        {
            if (contractMonths <= tier.MaximumContractMonths)
            {
                return tier.FeeRate;
            }
        }

        return null;
    }

    /// <summary>
    /// Why <see cref="FeeRateFor"/> has no rate for the contract length: the range the schedule
    /// covers. Null when it has one.
    /// </summary>
    public PricingGap? FindGap(int contractMonths) =>
        FeeRateFor(contractMonths) is null
            ? new PricingGap(PricingGapCode.OriginationFee, MinimumContractMonths, Tiers[^1].MaximumContractMonths)
            : null;

    /// <summary>
    /// Applies the vehicle's discount to a fee rate. A plug-in hybrid has percentage points taken
    /// off the rate; a green vehicle has the fee amount discounted, which is the same as scaling
    /// the rate by the same percentage. The two discounts are never combined — a plug-in hybrid is
    /// not green.
    /// </summary>
    public decimal DiscountedRate(VehicleCategory category, decimal feeRate) =>
        category switch
        {
            VehicleCategory.PlugInHybrid => Math.Max(0m, feeRate - PlugInHybridRateDiscount),
            VehicleCategory.ElectricOrHydrogen => feeRate * (100m - GreenFeeDiscountPercentage) / 100m,
            _ => feeRate
        };

    // Runs after Tiers has been validated, so the list is known to be non-empty with no nulls. The
    // minimum must fall inside the first band, or that band could never apply.
    private static int ValidateMinimumContractMonths(int minimumContractMonths, IReadOnlyList<FeeTier> tiers, string parameterName)
    {
        Guard.Positive(minimumContractMonths, parameterName);

        int shortestTierMonths = tiers.Min(tier => tier.MaximumContractMonths);

        return minimumContractMonths <= shortestTierMonths
            ? minimumContractMonths
            : throw new ArgumentOutOfRangeException(parameterName, minimumContractMonths, "Value must fall inside the shortest fee tier.");
    }

    private static IReadOnlyList<FeeTier> ValidateAndSort(IReadOnlyList<FeeTier> tiers, string parameterName)
    {
        Guard.NoNullElements(tiers, parameterName);

        if (tiers.Count == 0)
        {
            throw new ArgumentException("Fee tier list must not be empty.", parameterName);
        }

        if (tiers.GroupBy(tier => tier.MaximumContractMonths).Any(group => group.Count() > 1))
        {
            throw new ArgumentException("Fee tier list must not contain duplicate contract lengths.", parameterName);
        }

        return [.. tiers.OrderBy(tier => tier.MaximumContractMonths)];
    }
}
