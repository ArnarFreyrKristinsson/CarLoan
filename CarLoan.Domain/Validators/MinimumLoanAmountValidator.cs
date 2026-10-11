using CarLoan.Domain.Guards;
using CarLoan.Domain.Models;
using Params = System.Collections.Generic.Dictionary<CarLoan.Domain.Models.LoanRuleParameter, decimal>;

namespace CarLoan.Domain.Validators;

public class MinimumLoanAmountValidator(decimal minimumLoanAmount) : ILoanRule
{
    private readonly decimal _minimumLoanAmount = Guard.Positive(minimumLoanAmount, nameof(minimumLoanAmount));

    public LoanRuleResult Evaluate(Loan loan)
    {
        ArgumentNullException.ThrowIfNull(loan);

        bool isValid = loan.Terms.LoanAmount >= _minimumLoanAmount;
        return LoanRuleResult.Create(
            LoanRuleCode.MinimumLoanAmount,
            isValid,
            $"Loan amount must be at least {_minimumLoanAmount:N0}.",
            new Params { [LoanRuleParameter.Minimum] = _minimumLoanAmount });
    }
}
