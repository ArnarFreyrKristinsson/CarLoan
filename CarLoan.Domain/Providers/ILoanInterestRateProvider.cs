using CarLoan.Domain.Models;

namespace CarLoan.Domain.Providers;

public interface ILoanInterestRateProvider
{
    /// <summary>The rate for the loan, or null when the schedule has no rate for it.</summary>
    decimal? GetInterestRate(Loan loan);

    /// <summary>
    /// Reports the loan's financing ratio falling outside the bands the rate schedule prices,
    /// or null when the schedule has a rate for it.
    /// </summary>
    PricingGap? FindPricingGap(Loan loan);
}
