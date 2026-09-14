namespace CarLoan.Domain.Fees;

public sealed record OriginationFee(decimal Amount, decimal AmountSaved, decimal EffectiveRate);