namespace CarLoan.Domain.Providers;

public sealed record RateTier(decimal MinimumDownPayment, decimal InterestRate);
