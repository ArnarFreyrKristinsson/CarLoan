using CarLoan.Domain.Guards;
using CarLoan.Domain.Models;
using Params = System.Collections.Generic.Dictionary<CarLoan.Domain.Models.LoanRuleParameter, decimal>;

namespace CarLoan.Domain.Validators;

public class MinimumDownPaymentValidator(decimal allowedMinimumDownPayment) : ILoanRule
{
    private readonly decimal _minimumDownPayment = Guard.Positive(allowedMinimumDownPayment, nameof(allowedMinimumDownPayment));

    public LoanRuleResult Evaluate(Loan loan)
    {
        ArgumentNullException.ThrowIfNull(loan);

        bool isValid = loan.Terms.DownPayment >= _minimumDownPayment;
        return LoanRuleResult.Create(
            LoanRuleCode.MinimumDownPayment,
            isValid,
            $"Down payment must be at least {_minimumDownPayment:N0}.",
            new Params { [LoanRuleParameter.Minimum] = _minimumDownPayment });
    }
}