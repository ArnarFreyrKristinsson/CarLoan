using CarLoan.Domain.Guards;
using CarLoan.Domain.Models;
using Params = System.Collections.Generic.Dictionary<string, object>;

namespace CarLoan.Domain.Validators;

/// <summary>
/// Caps the car age plus the term. Used cars only — new cars are exempt.
/// </summary>
public class CarAgeValidator(CarAgeLimits limits) : ILoanRule
{
    private const string RuleName = "CarAge";
    private const int MonthsPerYear = 12;

    private readonly CarAgeLimits _limits = Guard.NotNull(limits, nameof(limits));

    public LoanRuleResult Evaluate(Loan loan)
    {
        ArgumentNullException.ThrowIfNull(loan);

        if (loan.Car.Condition == CarCondition.New)
            return LoanRuleResult.Create(RuleName, true);

        decimal termYears = (decimal)loan.Terms.LoanPeriodInMonths / MonthsPerYear;
        decimal combinedYears = loan.Car.AgeInYears + termYears;
        int maximumCombinedYears = _limits.MaximumCombinedYearsFor(loan.Terms.LoanRatio);

        if (combinedYears <= maximumCombinedYears)
            return LoanRuleResult.Create(RuleName, true);

        return LoanRuleResult.Create(
            RuleName,
            false,
            $"Car age plus loan term must not exceed {maximumCombinedYears} years at a loan ratio of {loan.Terms.LoanRatio:0.##}%.",
            new Params
            {
                ["maxCombinedYears"] = maximumCombinedYears,
                ["ratioThreshold"] = _limits.LoanRatioThreshold,
                ["combinedYears"] = combinedYears
            });
    }
}
