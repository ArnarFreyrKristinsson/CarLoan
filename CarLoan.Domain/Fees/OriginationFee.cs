namespace CarLoan.Domain.Fees;

/// <summary>
/// The origination fee charged on a loan. <see cref="MinimumFeeApplied"/> is true when the
/// minimum fee raised the fee above what the rate gave.
/// </summary>
public sealed record OriginationFee(decimal Amount, decimal AmountSaved, decimal EffectiveRate, bool MinimumFeeApplied);
