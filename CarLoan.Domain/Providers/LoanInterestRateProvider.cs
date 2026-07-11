namespace CarLoan.Domain.Providers;

public sealed class LoanInterestRateProvider : ILoanInterestRateProvider
{
    private static readonly (decimal MinimumDownPayment, decimal InterestRate)[] _interestLookupTable =
    [
        (1000000m, 10.35m),
        (600000m, 11.20m),
        (400000m, 11.45m),
        (200000m, 12.20m)
    ];

    public decimal GetInterestRate(decimal downPayment)
    {
        foreach (var (minimumDownPayment, interestRate) in _interestLookupTable)
        {
            if (downPayment >= minimumDownPayment)
            {
                return interestRate;
            }
        }

        return _interestLookupTable[^1].InterestRate;
    }
}