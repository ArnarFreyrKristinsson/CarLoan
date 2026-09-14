using CarLoan.Domain.Guards;

namespace CarLoan.Domain.Providers;

public sealed record RateTier(decimal MaximumFinancingRatio, decimal InterestRate)
{
    public decimal MaximumFinancingRatio { get; } = Guard.Positive(MaximumFinancingRatio, nameof(MaximumFinancingRatio));

    public decimal InterestRate { get; } = Guard.Positive(InterestRate, nameof(InterestRate));
}
