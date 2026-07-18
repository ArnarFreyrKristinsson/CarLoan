using CarLoan.Domain.Models;

namespace CarLoan.Domain.Validators;

public interface ILoanValidator
{
    IReadOnlyList<LoanRuleResult> Validate(Loan loan);
}
