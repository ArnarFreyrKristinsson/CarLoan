using CarLoan.Domain.Guards;

namespace CarLoan.Domain.Fees;

public sealed record FeeTier(int MaximumContractMonths, decimal FeeRate)
{
    public int MaximumContractMonths { get; } = Guard.Positive(MaximumContractMonths, nameof(MaximumContractMonths));

    public decimal FeeRate { get; } = Guard.Positive(FeeRate, nameof(FeeRate));
}
