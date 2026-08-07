namespace CarLoan.Domain.Providers;

public sealed class LoanInterestRateProvider : ILoanInterestRateProvider
{
    private readonly IReadOnlyList<RateTier> _interestLookupTable;
    private readonly decimal _defaultInterestRate;

    public LoanInterestRateProvider(IReadOnlyList<RateTier> interestLookupTable, decimal defaultInterestRate)
    {
        ArgumentNullException.ThrowIfNull(interestLookupTable);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(defaultInterestRate);

        if (interestLookupTable.Any(tier => tier is null))
        {
            throw new ArgumentException("Tier list must not contain null entries.", nameof(interestLookupTable));
        }

        _interestLookupTable = [.. interestLookupTable.OrderByDescending(tier => tier.MinimumDownPayment)];
        _defaultInterestRate = defaultInterestRate;
    }

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
}