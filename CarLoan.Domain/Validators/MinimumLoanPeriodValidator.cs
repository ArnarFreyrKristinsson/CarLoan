using CarLoan.Domain.Models;
using Params = System.Collections.Generic.Dictionary<string, object>;

namespace CarLoan.Domain.Validators;

public class MinimumLoanPeriodValidator(int minimumLoanPeriodMonths) : ILoanRule
{
    private readonly int _minimumLoanPeriodMonths = minimumLoanPeriodMonths;

    public LoanRuleResult Evaluate(Loan loan)
    {
        ArgumentNullException.ThrowIfNull(loan);

        bool isValid = loan.Terms.LoanPeriodInMonths >= _minimumLoanPeriodMonths;
        return LoanRuleResult.Create(
            "MinimumLoanPeriod",
            isValid,
            $"Loan period must be at least {_minimumLoanPeriodMonths} months.",
            new Params { ["min"] = _minimumLoanPeriodMonths });
    }
}