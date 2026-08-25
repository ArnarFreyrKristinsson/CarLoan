using CarLoan.Domain.Guards;
using CarLoan.Domain.Models;

namespace CarLoan.Domain.Fees;

/// <summary>
/// Computes the origination fee on the pre-fee loan amount. The plug-in hybrid discount comes
/// off the rate, the green discount comes off the amount, and they are never combined — a
/// plug-in hybrid is not green.
/// </summary>
public sealed class OriginationFeeCalculator(OriginationFeeSettings settings) : IOriginationFeeCalculator
{
    private const int MoneyDecimals = 2;

    private readonly OriginationFeeSettings _settings = Guard.NotNull(settings, nameof(settings));

    public OriginationFee Calculate(Loan loan)
    {
        ArgumentNullException.ThrowIfNull(loan);

        decimal loanAmount = loan.Terms.LoanAmount;
        decimal baseRate = _settings.FeeRateFor(loan.Terms.LoanPeriodInMonths);
        decimal effectiveRate = EffectiveRateFor(loan.Car, baseRate);

        decimal discountedFee = Money(loanAmount * effectiveRate / 100m);
        decimal undiscountedFee = Money(loanAmount * baseRate / 100m);

        decimal amount = Math.Max(discountedFee, _settings.MinimumFee);

        return new OriginationFee(amount, Math.Max(0m, undiscountedFee - amount), effectiveRate);
    }

    private decimal EffectiveRateFor(Car car, decimal baseRate) => car.Category switch
    {
        VehicleCategory.PlugInHybrid => Math.Max(0m, baseRate - _settings.PlugInHybridRateDiscount),
        VehicleCategory.ElectricOrHydrogen => baseRate * (100m - _settings.GreenFeeDiscountPercentage) / 100m,
        _ => baseRate
    };

    private static decimal Money(decimal value) => Math.Round(value, MoneyDecimals);
}
