namespace CarLoan.Domain.Models;

/// <summary>
/// A loan falls outside the range a lender's schedule prices, so that price does not exist.
/// <see cref="Minimum"/> and <see cref="Maximum"/> are the range the schedule does price: the
/// financing ratio in percent for <see cref="PricingGapCode.InterestRate"/>, the term in months
/// for <see cref="PricingGapCode.OriginationFee"/>.
/// </summary>
public sealed record PricingGap(PricingGapCode Code, decimal Minimum, decimal Maximum);
