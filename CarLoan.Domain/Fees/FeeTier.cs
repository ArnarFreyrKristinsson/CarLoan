using CarLoan.Domain.Guards;

namespace CarLoan.Domain.Fees;

/// <summary>
/// One band of the origination fee schedule, keyed on contract length in months.
/// Bands are inclusive at the top: a 23-month band covers everything up to and including 23 months.
/// </summary>
public sealed record FeeTier(int MaximumContractMonths, decimal FeeRate)
{
    public int MaximumContractMonths { get; } = Guard.Positive(MaximumContractMonths, nameof(MaximumContractMonths));

    public decimal FeeRate { get; } = Guard.Positive(FeeRate, nameof(FeeRate));
}
