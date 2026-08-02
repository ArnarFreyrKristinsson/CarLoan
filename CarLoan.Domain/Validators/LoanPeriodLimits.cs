using CarLoan.Domain.Models;

namespace CarLoan.Domain.Validators;

public sealed record LoanPeriodLimits(
    decimal MaximumLoanRatio,
    decimal UsedCarLoanRatioThreshold,
    int MaximumLoanPeriodMonths,
    int UsedCarMaximumLoanPeriodMonths)
{
    public bool ExceedsGeneralLimits(decimal ratio, int period) =>
        ratio > MaximumLoanRatio || period > MaximumLoanPeriodMonths;

    public bool ExceedsUsedCarLimits(decimal ratio, CarCondition condition, int period) =>
        condition == CarCondition.Used && ratio > UsedCarLoanRatioThreshold && period > UsedCarMaximumLoanPeriodMonths;
}
