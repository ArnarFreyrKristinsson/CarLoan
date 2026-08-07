using CarLoan.Domain.Guards;

namespace CarLoan.Domain.Providers;

public sealed record RateTier(decimal MinimumDownPayment, decimal InterestRate)
{
    public decimal MinimumDownPayment { get; } = MinimumDownPayment >= 0
        ? MinimumDownPayment
        : throw new ArgumentOutOfRangeException(nameof(MinimumDownPayment), MinimumDownPayment, "Minimum down payment must not be negative.");

    public decimal InterestRate { get; } = Guard.Positive(InterestRate, nameof(InterestRate));
}
