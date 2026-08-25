using CarLoan.Domain.Guards;
using CarLoan.Domain.Models;
using Params = System.Collections.Generic.Dictionary<string, object>;

namespace CarLoan.Domain.Validators;

public class MaximumLoanAmountValidator(decimal maximumLoanAmount) : ILoanRule
{
    private readonly decimal _maximumLoanAmount = Guard.Positive(maximumLoanAmount, nameof(maximumLoanAmount));

    public LoanRuleResult Evaluate(Loan loan)
    {
        ArgumentNullException.ThrowIfNull(loan);

        bool isValid = loan.Terms.LoanAmount <= _maximumLoanAmount;
        return LoanRuleResult.Create(
            "MaximumLoanAmount",
            isValid,
            $"Loan amount must not exceed {_maximumLoanAmount:N0}.",
            new Params { ["max"] = _maximumLoanAmount });
    }
}
