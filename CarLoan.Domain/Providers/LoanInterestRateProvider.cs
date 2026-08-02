namespace CarLoan.Domain.Providers;

public sealed class LoanInterestRateProvider(IReadOnlyList<RateTier> interestLookupTable, decimal lowestInterestRate) : ILoanInterestRateProvider
{
    private readonly IReadOnlyList<RateTier> _interestLookupTable = interestLookupTable;
    private readonly decimal _lowestInterestRate = lowestInterestRate;

    public decimal GetInterestRate(decimal downPayment)
    {
        foreach (var (minimumDownPayment, interestRate) in _interestLookupTable)
        {
            if (downPayment >= minimumDownPayment)
            {
                return interestRate;
            }
        }

        return _lowestInterestRate;
    }
}