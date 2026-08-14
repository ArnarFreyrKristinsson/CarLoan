using CarLoan.Domain.Guards;

namespace CarLoan.Domain.Providers;

public sealed class LoanInterestRateProvider(IReadOnlyList<RateTier> interestLookupTable, decimal defaultInterestRate) : ILoanInterestRateProvider
{
    private readonly IReadOnlyList<RateTier> _interestLookupTable = ValidateAndSort(interestLookupTable);
    private readonly decimal _defaultInterestRate = Guard.Positive(defaultInterestRate, nameof(defaultInterestRate));

    public decimal GetInterestRate(decimal downPayment)
    {
        foreach (var (minimumDownPayment, interestRate) in _interestLookupTable)
        {
            if (downPayment >= minimumDownPayment)
            {
                return interestRate;
            }
        }

        return _defaultInterestRate;
    }

    private static IReadOnlyList<RateTier> ValidateAndSort(IReadOnlyList<RateTier> interestLookupTable)
    {
        Guard.NoNullElements(interestLookupTable, nameof(interestLookupTable));

        if (interestLookupTable.GroupBy(tier => tier.MinimumDownPayment).Any(group => group.Count() > 1))
        {
            throw new ArgumentException("Tier list must not contain duplicate minimum down payments.", nameof(interestLookupTable));
        }

        return [.. interestLookupTable.OrderByDescending(tier => tier.MinimumDownPayment)];
    }
}
