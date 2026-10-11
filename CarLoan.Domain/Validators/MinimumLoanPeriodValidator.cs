using CarLoan.Domain.Guards;
using CarLoan.Domain.Models;
using Params = System.Collections.Generic.Dictionary<CarLoan.Domain.Models.LoanRuleParameter, decimal>;

namespace CarLoan.Domain.Validators;

public class MinimumLoanPeriodValidator(int minimumLoanPeriodMonths) : ILoanRule
{
    private readonly int _minimumLoanPeriodMonths = Guard.Positive(minimumLoanPeriodMonths, nameof(minimumLoanPeriodMonths));

    public LoanRuleResult Evaluate(Loan loan)
    {
        ArgumentNullException.ThrowIfNull(loan);

        bool isValid = loan.Terms.LoanPeriodInMonths >= _minimumLoanPeriodMonths;
        return LoanRuleResult.Create(
            LoanRuleCode.MinimumLoanPeriod,
            isValid,
            $"Loan period must be at least {_minimumLoanPeriodMonths} months.",
            new Params { [LoanRuleParameter.Minimum] = _minimumLoanPeriodMonths });
    }
}