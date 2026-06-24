using CarLoan.Domain.Calculators;

namespace CarLoan.Domain.Models;

public sealed record LoanTerms(
    decimal MinimumLoanAmount,
    decimal PurchasePrice,
    decimal DownPayment,
    int LoanPeriodInMonths,
    decimal LoanRatio) : ILoanTerms
{
    private static readonly LoanInterestRateProvider _loanInterestRateProvider = new();

    public decimal LoanAmount => PurchasePrice - DownPayment;

    public decimal InterestRate => _loanInterestRateProvider.GetInterestRate(this);
}