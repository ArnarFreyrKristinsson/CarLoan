using CarLoan.Domain.Guards;
using CarLoan.Domain.Models;
using Params = System.Collections.Generic.Dictionary<CarLoan.Domain.Models.LoanRuleParameter, decimal>;

namespace CarLoan.Domain.Validators;

public class MaximumLoanPeriodValidator(LoanPeriodLimits limits) : ILoanRule
{
    private readonly LoanPeriodLimits _limits = Guard.NotNull(limits, nameof(limits));

    public LoanRuleResult Evaluate(Loan loan)
    {
        ArgumentNullException.ThrowIfNull(loan);

        int period = loan.Terms.LoanPeriodInMonths;
        decimal ratio = loan.Terms.LoanRatio;
        var condition = loan.Car.Condition;

        if (_limits.ExceedsGeneralLimit(period))
            return LoanRuleResult.Create(LoanRuleCode.MaximumLoanPeriod, false,
                $"Loan period must not exceed {_limits.MaximumLoanPeriodMonths} months.",
                new Params { [LoanRuleParameter.MaximumLoanPeriodMonths] = _limits.MaximumLoanPeriodMonths });

        if (_limits.ExceedsUsedCarLimits(ratio, condition, period))
            return LoanRuleResult.Create(LoanRuleCode.MaximumLoanPeriod, false,
                $"Used cars with a loan ratio above {_limits.UsedCarLoanRatioThreshold}% must not exceed {_limits.UsedCarMaximumLoanPeriodMonths} months.",
                new Params
                {
                    [LoanRuleParameter.LoanRatioThreshold] = _limits.UsedCarLoanRatioThreshold,
                    [LoanRuleParameter.MaximumLoanPeriodMonths] = _limits.UsedCarMaximumLoanPeriodMonths
                });

        return LoanRuleResult.Create(LoanRuleCode.MaximumLoanPeriod, true);
    }
}