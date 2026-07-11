namespace CarLoan.Domain.Models;

public sealed record LoanTerms(
    decimal PurchasePrice,
    decimal DownPayment,
    int LoanPeriodInMonths,
    decimal InterestRate)
{
    public decimal LoanAmount => PurchasePrice - DownPayment;
    public decimal LoanRatio => LoanAmount / PurchasePrice * 100m;
}