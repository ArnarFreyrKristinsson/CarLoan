using CarLoan.Domain.Guards;

namespace CarLoan.Domain.Providers;

public sealed record RateTier(decimal MinimumDownPayment, decimal InterestRate)
{
    public decimal MinimumDownPayment { get; } = Guard.NonNegative(MinimumDownPayment, nameof(MinimumDownPayment));

    public decimal InterestRate { get; } = Guard.Positive(InterestRate, nameof(InterestRate));
}