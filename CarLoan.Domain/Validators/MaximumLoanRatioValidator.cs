using CarLoan.Domain.Guards;
using CarLoan.Domain.Models;
using Params = System.Collections.Generic.Dictionary<CarLoan.Domain.Models.LoanRuleParameter, decimal>;

namespace CarLoan.Domain.Validators;

public class MaximumLoanRatioValidator(decimal maximumLoanRatio) : ILoanRule
{
    private readonly decimal _maximumLoanRatio = Guard.Positive(maximumLoanRatio, nameof(maximumLoanRatio));

    public LoanRuleResult Evaluate(Loan loan)
    {
        ArgumentNullException.ThrowIfNull(loan);

        bool isValid = loan.Terms.LoanRatio <= _maximumLoanRatio;
        return LoanRuleResult.Create(
            LoanRuleCode.MaximumLoanRatio,
            isValid,
            $"Loan ratio must not exceed {_maximumLoanRatio}%.",
            new Params { [LoanRuleParameter.Maximum] = _maximumLoanRatio });
    }
}
