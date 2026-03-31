namespace CarLoan.Domain;

public sealed record LoanTerms(
    decimal MinimumLoanAmount,
    decimal PurchasePrice,
    decimal DownPayment,
    decimal InterestRate,
    int LoanPeriodInMonths,
    decimal LoanRatio)
{
    public decimal LoanAmount => PurchasePrice - DownPayment;
}