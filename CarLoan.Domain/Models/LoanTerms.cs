namespace CarLoan.Domain.Models;

public sealed record LoanTerms(
    decimal PurchasePrice,
    decimal DownPayment,
    int LoanPeriodInMonths,
    decimal InterestRate,
    decimal OriginationFee = 0m)
{
    public decimal LoanAmount => PurchasePrice - DownPayment;

    public decimal LoanRatio => LoanAmount / PurchasePrice * 100m;

    public decimal FinancedAmount => LoanAmount + OriginationFee;
}
