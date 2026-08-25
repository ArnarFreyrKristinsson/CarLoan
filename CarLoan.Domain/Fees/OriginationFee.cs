namespace CarLoan.Domain.Fees;

/// <summary>
/// The origination fee charged on a loan.
/// </summary>
/// <param name="Amount">The fee payable, after discounts and the minimum fee.</param>
/// <param name="AmountSaved">How much the discounts took off the undiscounted fee.</param>
/// <param name="EffectiveRate">The rate the fee works out to, before the minimum fee is applied.</param>
public sealed record OriginationFee(decimal Amount, decimal AmountSaved, decimal EffectiveRate)
{
    public static readonly OriginationFee None = new(0m, 0m, 0m);
}
