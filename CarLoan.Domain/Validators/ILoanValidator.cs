namespace CarLoan.Domain;

public interface ILoanValidator
{
    LoanRuleResult Evaluate(Loan loan);
}
