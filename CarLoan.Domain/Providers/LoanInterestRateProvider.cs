using CarLoan.Domain.Guards;
using CarLoan.Domain.Models;

namespace CarLoan.Domain.Providers;

/// <summary>
/// Picks the rate table by vehicle category, green or general and looks the rate up by financing ratio.
/// </summary>
public sealed class LoanInterestRateProvider(RateTable general, RateTable green) : ILoanInterestRateProvider
{
    private readonly RateTable _general = Guard.NotNull(general, nameof(general));
    private readonly RateTable _green = Guard.NotNull(green, nameof(green));

    public decimal? GetInterestRate(Loan loan)
    {
        ArgumentNullException.ThrowIfNull(loan);

        return TableFor(loan).GetRate(loan.Terms.LoanRatio);
    }

    public PricingGap? FindPricingGap(Loan loan)
    {
        ArgumentNullException.ThrowIfNull(loan);

        return TableFor(loan).FindGap(loan.Terms.LoanRatio);
    }

    private RateTable TableFor(Loan loan) => loan.Car.IsGreen ? _green : _general;
}
