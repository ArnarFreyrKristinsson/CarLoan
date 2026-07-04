using CarLoan.Domain.Models;
using Params = System.Collections.Generic.Dictionary<string, object>;

namespace CarLoan.Domain.Validators;

public class MinimumLoanAmountValidator : ILoanValidator
{
    private const decimal MinimumLoanAmount = 750000m;

    public LoanRuleResult Evaluate(Loan loan)
    {
        bool isValid = loan.Terms.LoanAmount >= MinimumLoanAmount;
        return LoanRuleResult.Create(
            "MinimumLoanAmount",
            isValid,
            $"Loan amount must be at least {MinimumLoanAmount:N0}.",
            new Params { ["min"] = MinimumLoanAmount });
    }
}
