using CarLoan.Domain.Models;
using Params = System.Collections.Generic.Dictionary<string, object>;

namespace CarLoan.Domain.Validators;

public class MinimumLoanAmountValidator(decimal minimumLoanAmount) : ILoanRule
{
    private readonly decimal _minimumLoanAmount = minimumLoanAmount;

    public LoanRuleResult Evaluate(Loan loan)
    {
        ArgumentNullException.ThrowIfNull(loan);

        bool isValid = loan.Terms.LoanAmount >= _minimumLoanAmount;
        return LoanRuleResult.Create(
            "MinimumLoanAmount",
            isValid,
            $"Loan amount must be at least {_minimumLoanAmount:N0}.",
            new Params { ["min"] = _minimumLoanAmount });
    }
}
