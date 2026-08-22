using CarLoan.Domain.Guards;
using CarLoan.Domain.Models;

namespace CarLoan.Domain.Validators;

public sealed record LoanPeriodLimits(
    decimal MaximumLoanRatio,
    decimal UsedCarLoanRatioThreshold,
    int MaximumLoanPeriodMonths,
    int UsedCarMaximumLoanPeriodMonths)
{
    public decimal MaximumLoanRatio { get; } = Guard.Positive(MaximumLoanRatio, nameof(MaximumLoanRatio));
    public decimal UsedCarLoanRatioThreshold { get; } = Guard.Positive(UsedCarLoanRatioThreshold, nameof(UsedCarLoanRatioThreshold));
    public int MaximumLoanPeriodMonths { get; } = Guard.Positive(MaximumLoanPeriodMonths, nameof(MaximumLoanPeriodMonths));
    public int UsedCarMaximumLoanPeriodMonths { get; } = Guard.Positive(UsedCarMaximumLoanPeriodMonths, nameof(UsedCarMaximumLoanPeriodMonths));

    public bool ExceedsGeneralLimits(decimal ratio, int period) =>
        ratio > MaximumLoanRatio || period > MaximumLoanPeriodMonths;

    public bool ExceedsUsedCarLimits(decimal ratio, CarCondition condition, int period) =>
        condition == CarCondition.Used && ratio > UsedCarLoanRatioThreshold && period > UsedCarMaximumLoanPeriodMonths;
}
