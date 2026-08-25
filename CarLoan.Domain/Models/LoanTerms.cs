namespace CarLoan.Domain.Models;

public sealed record LoanTerms(
    decimal PurchasePrice,
    decimal DownPayment,
    int LoanPeriodInMonths,
    decimal InterestRate,
    decimal OriginationFee = 0m)
{
    /// <summary>Pre-fee loan amount. All amount and ratio rules run against this.</summary>
    public decimal LoanAmount => PurchasePrice - DownPayment;

    public decimal LoanRatio => LoanAmount / PurchasePrice * 100m;

    /// <summary>The loan amount with the origination fee added on top (F5).</summary>
    public decimal FinancedAmount => LoanAmount + OriginationFee;
}
