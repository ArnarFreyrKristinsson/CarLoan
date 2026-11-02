namespace CarLoan.Domain.Models;

/// <summary>
/// Which part of a lender's price schedule has no band for a loan.
/// </summary>
public enum PricingGapCode
{
    InterestRate,
    OriginationFee
}
