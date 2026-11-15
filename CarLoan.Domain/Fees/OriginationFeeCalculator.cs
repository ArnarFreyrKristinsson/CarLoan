using CarLoan.Domain.Guards;
using CarLoan.Domain.Models;

namespace CarLoan.Domain.Fees;

/// <summary>
/// Computes the origination fee on the pre-fee loan amount, using the discounted rate from the schedule,
/// then raises it to the minimum fee. A term the schedule does not cover has no fee.
/// </summary>
public sealed class OriginationFeeCalculator(OriginationFeeSettings settings) : IOriginationFeeCalculator
{
    private const int MoneyDecimals = 2;

    private readonly OriginationFeeSettings _settings = Guard.NotNull(settings, nameof(settings));

    public OriginationFee? Calculate(Loan loan)
    {
        ArgumentNullException.ThrowIfNull(loan);

        if (_settings.FeeRateFor(loan.Terms.LoanPeriodInMonths) is not decimal feeRate)
            return null;

        decimal loanAmount = loan.Terms.LoanAmount;
        decimal effectiveRate = _settings.DiscountedRate(loan.Car.Category, feeRate);

        decimal discountedFee = Money(loanAmount * effectiveRate / 100m);
        decimal undiscountedFee = Money(loanAmount * feeRate / 100m);

        decimal amount = Math.Max(discountedFee, _settings.MinimumFee);

        return new OriginationFee(amount, Math.Max(0m, undiscountedFee - amount), effectiveRate, discountedFee < _settings.MinimumFee);
    }

    public PricingGap? FindPricingGap(Loan loan)
    {
        ArgumentNullException.ThrowIfNull(loan);

        return _settings.FindGap(loan.Terms.LoanPeriodInMonths);
    }

    private static decimal Money(decimal value) => Math.Round(value, MoneyDecimals);
}
