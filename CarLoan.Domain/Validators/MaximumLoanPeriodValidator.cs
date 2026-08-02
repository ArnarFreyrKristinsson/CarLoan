using CarLoan.Domain.Models;
using Params = System.Collections.Generic.Dictionary<string, object>;

namespace CarLoan.Domain.Validators;

public class MaximumLoanPeriodValidator(LoanPeriodLimits limits) : ILoanRule
{
    private const string RuleName = "MaximumLoanPeriod";

    private readonly LoanPeriodLimits _limits = limits;

    public LoanRuleResult Evaluate(Loan loan)
    {
        ArgumentNullException.ThrowIfNull(loan);

        int period = loan.Terms.LoanPeriodInMonths;
        decimal ratio = loan.Terms.LoanRatio;
        var condition = loan.Car.Condition;

        if (_limits.ExceedsGeneralLimits(ratio, period))
            return LoanRuleResult.Create(RuleName, false,
                $"Loan ratio must not exceed {_limits.MaximumLoanRatio}% and period must not exceed {_limits.MaximumLoanPeriodMonths} months.",
                new Params { ["maxRatio"] = _limits.MaximumLoanRatio, ["maxMonths"] = _limits.MaximumLoanPeriodMonths });

        if (_limits.ExceedsUsedCarLimits(ratio, condition, period))
            return LoanRuleResult.Create(RuleName, false,
                $"Used cars with a loan ratio above {_limits.UsedCarLoanRatioThreshold}% must not exceed {_limits.UsedCarMaximumLoanPeriodMonths} months.",
                new Params { ["ratioThreshold"] = _limits.UsedCarLoanRatioThreshold, ["maxMonths"] = _limits.UsedCarMaximumLoanPeriodMonths });

        return LoanRuleResult.Create(RuleName, true);
    }
}