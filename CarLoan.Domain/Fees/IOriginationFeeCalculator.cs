using CarLoan.Domain.Models;

namespace CarLoan.Domain.Fees;

public interface IOriginationFeeCalculator
{
    /// <summary>The fee for the loan, or null when the schedule has no fee for its term.</summary>
    OriginationFee? Calculate(Loan loan);

    /// <summary>
    /// Reports the loan's term falling outside the contract lengths the fee schedule prices,
    /// or null when the schedule has a fee for it.
    /// </summary>
    PricingGap? FindPricingGap(Loan loan);
}
