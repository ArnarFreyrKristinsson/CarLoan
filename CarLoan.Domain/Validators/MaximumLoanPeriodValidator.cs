using CarLoan.Domain.Models;
using Params = System.Collections.Generic.Dictionary<string, object>;

namespace CarLoan.Domain.Validators;

public class MaximumLoanPeriodValidator : ILoanValidator
{
    private const string RuleName = "MaximumLoanPeriod";

    public LoanRuleResult Evaluate(Loan loan)
    {
        int period = loan.Terms.LoanPeriodInMonths;
        decimal ratio = loan.Terms.LoanRatio;
        var condition = loan.Car.Condition;

        if (LoanPeriodLimits.ExceedsGeneralLimits(ratio, period))
            return LoanRuleResult.Create(RuleName, false,
                $"Loan ratio must not exceed {LoanPeriodLimits.MaximumLoanRatio}% and period must not exceed {LoanPeriodLimits.MaximumLoanPeriodMonths} months.",
                new Params { ["maxRatio"] = LoanPeriodLimits.MaximumLoanRatio, ["maxMonths"] = LoanPeriodLimits.MaximumLoanPeriodMonths });

        if (LoanPeriodLimits.ExceedsUsedCarLimits(ratio, condition, period))
            return LoanRuleResult.Create(RuleName, false,
                $"Used cars with a loan ratio above {LoanPeriodLimits.UsedCarLoanRatioThreshold}% must not exceed {LoanPeriodLimits.UsedCarMaximumLoanPeriodMonths} months.",
                new Params { ["ratioThreshold"] = LoanPeriodLimits.UsedCarLoanRatioThreshold, ["maxMonths"] = LoanPeriodLimits.UsedCarMaximumLoanPeriodMonths });

        return LoanRuleResult.Create(RuleName, true);
    }
}