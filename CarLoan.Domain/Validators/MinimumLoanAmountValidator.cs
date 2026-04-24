namespace CarLoan.Domain;

public class MinimumLoanAmountValidator : ILoanValidator
{
    private const decimal MinimumLoanAmount = 750000m;

    public LoanRuleResult Evaluate(Loan loan)
    {
        bool isValid = loan.Terms.LoanAmount >= MinimumLoanAmount;
        return new LoanRuleResult(
            isValid,
            "MinimumLoanAmount",
            isValid ? null : $"Loan amount must be at least {MinimumLoanAmount:N0}.");
    }
}
