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

    /// <summary>
    /// The longest term the two limits allow together: the general limit for every car, cut to
    /// the used-car limit when a used car is financed above the threshold.
    /// </summary>
    public int MaximumLoanPeriodFor(decimal ratio, CarCondition condition) =>
        condition == CarCondition.Used && ratio > UsedCarLoanRatioThreshold
            ? Math.Min(MaximumLoanPeriodMonths, UsedCarMaximumLoanPeriodMonths)
            : MaximumLoanPeriodMonths;

    public bool ExceedsUsedCarLimits(decimal ratio, CarCondition condition, int period) =>
        condition == CarCondition.Used && ratio > UsedCarLoanRatioThreshold && period > UsedCarMaximumLoanPeriodMonths;
}
