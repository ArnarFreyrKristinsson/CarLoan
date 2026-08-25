using CarLoan.Domain.Guards;
using CarLoan.Domain.Models;

namespace CarLoan.Domain.Providers;

/// <summary>
/// Picks the rate table by vehicle category — green (V3) or general (V1, V2) — and looks the
/// rate up by financing ratio.
/// </summary>
public sealed class LoanInterestRateProvider(RateTable general, RateTable green) : ILoanInterestRateProvider
{
    private readonly RateTable _general = Guard.NotNull(general, nameof(general));
    private readonly RateTable _green = Guard.NotNull(green, nameof(green));

    public decimal GetInterestRate(Loan loan)
    {
        ArgumentNullException.ThrowIfNull(loan);

        return (loan.Car.IsGreen ? _green : _general).GetRate(loan.Terms.LoanRatio);
    }
}
