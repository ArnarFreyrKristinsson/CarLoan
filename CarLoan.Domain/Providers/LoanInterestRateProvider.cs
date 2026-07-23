namespace CarLoan.Domain.Providers;

public sealed class LoanInterestRateProvider : ILoanInterestRateProvider
{
    private const decimal LowestInterestRate = 12.20m;

    private static readonly (decimal MinimumDownPayment, decimal InterestRate)[] _interestLookupTable =
    [
        (1000000m, 10.35m),
        (600000m, 11.20m),
        (400000m, 11.45m),
        (200000m,  LowestInterestRate)
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

        return LowestInterestRate;
    }
}