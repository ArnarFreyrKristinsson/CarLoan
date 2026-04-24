namespace CarLoan.Domain;

public class MinimumLoanPeriodValidator : ILoanValidator
{
    private const int MinimumLoanPeriodMonths = 6;

    public LoanRuleResult Evaluate(Loan loan)
    {
        bool isValid = loan.Terms.LoanPeriodInMonths >= MinimumLoanPeriodMonths;
        return new LoanRuleResult(
            isValid,
            "MinimumLoanPeriod",
            isValid ? null : $"Loan period must be at least {MinimumLoanPeriodMonths} months.");
    }
}
