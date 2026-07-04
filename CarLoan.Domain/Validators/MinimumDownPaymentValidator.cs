using CarLoan.Domain.Models;
using Params = System.Collections.Generic.Dictionary<string, object>;

namespace CarLoan.Domain.Validators;

public class MinimumDownPaymentValidator : ILoanValidator
{
    private const decimal MinimumDownPayment = 150000m;

    public LoanRuleResult Evaluate(Loan loan)
    {
        bool isValid = loan.Terms.DownPayment >= MinimumDownPayment;
        return LoanRuleResult.Create(
            "MinimumDownPayment",
            isValid,
            $"Down payment must be at least {MinimumDownPayment:N0}.",
            new Params { ["min"] = MinimumDownPayment });
    }
}