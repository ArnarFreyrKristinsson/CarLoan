using CarLoan.Domain.Models;
using Params = System.Collections.Generic.Dictionary<string, object>;

namespace CarLoan.Domain.Validators;

public class MinimumLoanPeriodValidator : ILoanValidator
{
    private const int MinimumLoanPeriodMonths = 6;

    public LoanRuleResult Evaluate(Loan loan)
    {
        ArgumentNullException.ThrowIfNull(loan);

        bool isValid = loan.Terms.LoanPeriodInMonths >= MinimumLoanPeriodMonths;
        return LoanRuleResult.Create(
            "MinimumLoanPeriod",
            isValid,
            $"Loan period must be at least {MinimumLoanPeriodMonths} months.",
            new Params { ["min"] = MinimumLoanPeriodMonths });
    }
}