namespace CarLoan.Domain;

public class MinimumDownPaymentValidator : ILoanValidator
{
    private const decimal MinimumDownPayment = 150000m;

    public LoanRuleResult Evaluate(Loan loan)
    {
        bool isValid = loan.Terms.DownPayment >= MinimumDownPayment;
        return new LoanRuleResult(
            isValid,
            "MinimumDownPayment",
            isValid ? null : $"Down payment must be at least {MinimumDownPayment:N0}.");
    }
}
