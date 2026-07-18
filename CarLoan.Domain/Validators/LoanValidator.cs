using CarLoan.Domain.Models;

namespace CarLoan.Domain.Validators;

public class LoanValidator(IEnumerable<ILoanRule> rules) : ILoanValidator
{
    private readonly IEnumerable<ILoanRule> _rules = rules;

    public IReadOnlyList<LoanRuleResult> Validate(Loan loan)
    {
        return [.. _rules.Select(rule => rule.Evaluate(loan))];
    }
}