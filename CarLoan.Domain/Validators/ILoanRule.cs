using CarLoan.Domain.Models;

namespace CarLoan.Domain.Validators;

public interface ILoanRule
{
    LoanRuleResult Evaluate(Loan loan);
}
