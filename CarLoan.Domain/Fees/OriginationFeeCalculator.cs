using CarLoan.Domain.Guards;
using CarLoan.Domain.Models;

namespace CarLoan.Domain.Fees;

/// <summary>
/// Computes the origination fee on the pre-fee loan amount, using the discounted rate from the
/// schedule, then raises it to the minimum fee. What the discounts are lives on
/// <see cref="OriginationFeeSettings"/>; this type only does the arithmetic.
/// </summary>
public sealed class OriginationFeeCalculator(OriginationFeeSettings settings) : IOriginationFeeCalculator
{
    private const int MoneyDecimals = 2;

    private readonly OriginationFeeSettings _settings = Guard.NotNull(settings, nameof(settings));

    public OriginationFee Calculate(Loan loan)
    {
        ArgumentNullException.ThrowIfNull(loan);

        decimal loanAmount = loan.Terms.LoanAmount;
        int contractMonths = loan.Terms.LoanPeriodInMonths;
        decimal effectiveRate = _settings.EffectiveRateFor(loan.Car.Category, contractMonths);

        decimal discountedFee = Money(loanAmount * effectiveRate / 100m);
        decimal undiscountedFee = Money(loanAmount * _settings.FeeRateFor(contractMonths) / 100m);

        decimal amount = Math.Max(discountedFee, _settings.MinimumFee);

        return new OriginationFee(amount, Math.Max(0m, undiscountedFee - amount), effectiveRate);
    }

    private static decimal Money(decimal value) => Math.Round(value, MoneyDecimals);
}
