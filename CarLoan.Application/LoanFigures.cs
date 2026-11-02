using CarLoan.Domain.Models;

namespace CarLoan.Application;

/// <summary>
/// The figures of a loan that are the same for every lender, as a screen shows them.
/// </summary>
/// <param name="LoanAmount">Purchase price minus down payment, in kr, before any fee.</param>
/// <param name="FinancingRatioRoundedUp">
/// The financing ratio in percent, rounded up to one decimal so it never shows on the wrong side
/// of a limit: 80.03 is 80.1. The rules and rates use the exact ratio, never this one.
/// </param>
/// <param name="DownPaymentPercentage">100 minus <paramref name="FinancingRatioRoundedUp"/>.</param>
public sealed record LoanFigures(decimal LoanAmount, decimal FinancingRatioRoundedUp, decimal DownPaymentPercentage)
{
    private const decimal TenthsPerPercent = 10m;

    internal static LoanFigures From(LoanTerms terms)
    {
        decimal financingRatio = Math.Ceiling(terms.LoanRatio * TenthsPerPercent) / TenthsPerPercent;

        return new(terms.LoanAmount, financingRatio, 100m - financingRatio);
    }
}
