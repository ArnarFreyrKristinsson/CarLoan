using CarLoan.Domain.Guards;
using CarLoan.Domain.Models;

namespace CarLoan.Domain.Validators;

public sealed record LoanPeriodLimits(
    decimal UsedCarLoanRatioThreshold,
    int MaximumLoanPeriodMonths,
    int UsedCarMaximumLoanPeriodMonths)
{
    public decimal UsedCarLoanRatioThreshold { get; } = Guard.Positive(UsedCarLoanRatioThreshold, nameof(UsedCarLoanRatioThreshold));
    public int MaximumLoanPeriodMonths { get; } = Guard.Positive(MaximumLoanPeriodMonths, nameof(MaximumLoanPeriodMonths));
    public int UsedCarMaximumLoanPeriodMonths { get; } = Guard.Positive(UsedCarMaximumLoanPeriodMonths, nameof(UsedCarMaximumLoanPeriodMonths));

    public bool ExceedsGeneralLimit(int period) =>
        period > MaximumLoanPeriodMonths;

    public bool ExceedsUsedCarLimits(decimal ratio, CarCondition condition, int period) =>
        condition == CarCondition.Used && ratio > UsedCarLoanRatioThreshold && period > UsedCarMaximumLoanPeriodMonths;
}
