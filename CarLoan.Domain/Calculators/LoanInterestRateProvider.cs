using CarLoan.Domain.Models;

namespace CarLoan.Domain.Calculators;

public sealed class LoanInterestRateProvider : ILoanInterestRateProvider
{
    private static readonly (decimal MinimumDownPayment, decimal InterestRate)[] _interestLookupTable =
    [
        (1000000m, 10.35m),
        (600000m, 11.20m),
        (400000m, 11.45m),
        (200000m, 12.20m)
    ];

    public decimal GetInterestRate(LoanTerms loanTerms)
    {
        foreach (var (minimumDownPayment, interestRate) in _interestLookupTable)
        {
            if (loanTerms.DownPayment >= minimumDownPayment)
            {
                return interestRate;
            }
        }

        return _interestLookupTable[^1].InterestRate;

    }
}
