using CarLoan.Domain.Guards;
using CarLoan.Domain.Models;
using Params = System.Collections.Generic.Dictionary<CarLoan.Domain.Models.LoanRuleParameter, decimal>;

namespace CarLoan.Domain.Validators;

/// <summary>
/// Caps the car age plus the term. Used cars only — new cars are exempt.
/// </summary>
public class CarAgeValidator(CarAgeLimits limits) : ILoanRule
{
    private const int MonthsPerYear = 12;

    private readonly CarAgeLimits _limits = Guard.NotNull(limits, nameof(limits));

    public LoanRuleResult Evaluate(Loan loan)
    {
        ArgumentNullException.ThrowIfNull(loan);

        if (loan.Car.Condition == CarCondition.New)
            return LoanRuleResult.Create(LoanRuleCode.CarAge, true);

        decimal termYears = (decimal)loan.Terms.LoanPeriodInMonths / MonthsPerYear;
        decimal combinedYears = loan.Car.AgeInYears + termYears;
        int maximumCombinedYears = _limits.MaximumCombinedYearsFor(loan.Terms.LoanRatio);

        if (combinedYears <= maximumCombinedYears)
            return LoanRuleResult.Create(LoanRuleCode.CarAge, true);

        return LoanRuleResult.Create(
            LoanRuleCode.CarAge,
            false,
            $"Car age plus loan term must not exceed {maximumCombinedYears} years at a loan ratio of {loan.Terms.LoanRatio:0.##}%.",
            new Params
            {
                [LoanRuleParameter.MaximumCombinedYears] = maximumCombinedYears,
                [LoanRuleParameter.LoanRatioThreshold] = _limits.LoanRatioThreshold,
                [LoanRuleParameter.CombinedYears] = combinedYears
            });
    }
}
