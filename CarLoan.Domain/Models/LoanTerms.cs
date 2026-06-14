using CarLoan.Domain.Calculators;

namespace CarLoan.Domain.Models;

public sealed record LoanTerms(
    decimal MinimumLoanAmount,
    decimal PurchasePrice,
    decimal DownPayment,
    int LoanPeriodInMonths,
    decimal LoanRatio)
{
    private static readonly ILoanInterestRateProvider _loanInterestRateProvider = new LoanInterestRateProvider();

    public decimal LoanAmount => PurchasePrice - DownPayment;

    public decimal InterestRate => _loanInterestRateProvider.GetInterestRate(this);
}