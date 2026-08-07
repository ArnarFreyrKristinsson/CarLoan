using CarLoan.Domain.Models;
using Params = System.Collections.Generic.Dictionary<string, object>;

namespace CarLoan.Domain.Validators;

public class MinimumDownPaymentValidator : ILoanRule
{
    private readonly decimal _minimumDownPayment;

    public MinimumDownPaymentValidator(decimal allowedMinimumDownPayment)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(allowedMinimumDownPayment);

        _minimumDownPayment = allowedMinimumDownPayment;
    }

    public LoanRuleResult Evaluate(Loan loan)
    {
        ArgumentNullException.ThrowIfNull(loan);

        bool isValid = loan.Terms.DownPayment >= _minimumDownPayment;
        return LoanRuleResult.Create(
            "MinimumDownPayment",
            isValid,
            $"Down payment must be at least {_minimumDownPayment:N0}.",
            new Params { ["min"] = _minimumDownPayment });
    }
}