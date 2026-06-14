using CarLoan.Domain.Models;

namespace CarLoan.Domain.Validators;

public class LoanValidator(IEnumerable<ILoanValidator> rules)
{
    private readonly IEnumerable<ILoanValidator> _rules = rules;

    public IReadOnlyList<LoanRuleResult> Validate(Loan loan)
    {
        return [.. _rules.Select(rule => rule.Evaluate(loan))];
    }
}