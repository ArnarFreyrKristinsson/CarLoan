namespace CarLoan.Domain;

public class MaximumLoanPeriodValidator : ILoanValidator
{
    private const string RuleName = "MaximumLoanPeriod";
    private const decimal MaximumLoanRatio = 90m;
    private const decimal UsedCarLoanRatioThreshold = 80m;
    private const int MaximumLoanPeriodMonths = 84;
    private const int UsedCarMaximumLoanPeriodMonths = 72;

    private static bool ExceedsGeneralLimits(decimal ratio, int period) =>
        ratio > MaximumLoanRatio || period > MaximumLoanPeriodMonths;

    private static bool ExceedsUsedCarRatio(decimal ratio, CarCondition carCondition) =>
        carCondition == CarCondition.Used && ratio > UsedCarLoanRatioThreshold;

    private static LoanRuleResult Fail(string message) => new(false, RuleName, message);
    private static LoanRuleResult Pass() => new(true, RuleName, null);

    public LoanRuleResult Evaluate(Loan loan)
    {
        int period = loan.Terms.LoanPeriodInMonths;
        decimal ratio = loan.Terms.LoanRatio;
        var carCondition = loan.Car.Condition;

        if (ExceedsGeneralLimits(ratio, period))
            return Fail($"Loan ratio must not exceed {MaximumLoanRatio}% and period must not exceed {MaximumLoanPeriodMonths} months.");

        if (ExceedsUsedCarRatio(ratio, carCondition) && period > UsedCarMaximumLoanPeriodMonths)
            return Fail($"Used cars with a loan ratio above {UsedCarLoanRatioThreshold}% must not exceed {UsedCarMaximumLoanPeriodMonths} months.");

        return Pass();
    }
}
