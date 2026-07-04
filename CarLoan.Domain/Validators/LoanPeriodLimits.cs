using CarLoan.Domain.Models;

namespace CarLoan.Domain.Validators;

internal static class LoanPeriodLimits
{
    internal const decimal MaximumLoanRatio = 90m;
    internal const decimal UsedCarLoanRatioThreshold = 80m;
    internal const int MaximumLoanPeriodMonths = 84;
    internal const int UsedCarMaximumLoanPeriodMonths = 72;

    internal static bool ExceedsGeneralLimits(decimal ratio, int period) =>
        ratio > MaximumLoanRatio || period > MaximumLoanPeriodMonths;

    internal static bool ExceedsUsedCarLimits(decimal ratio, CarCondition condition, int period) =>
        condition == CarCondition.Used && ratio > UsedCarLoanRatioThreshold && period > UsedCarMaximumLoanPeriodMonths;
}
