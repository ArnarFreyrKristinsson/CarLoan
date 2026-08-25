using CarLoan.Domain.Guards;
using CarLoan.Domain.Models;

namespace CarLoan.Domain.Validators;

public class LoanValidator(IEnumerable<ILoanRule> rules) : ILoanValidator
{
    private readonly IReadOnlyList<ILoanRule> _rules =
        Guard.NoNullElements([.. Guard.NotNull(rules, nameof(rules))], nameof(rules));

    public IReadOnlyList<LoanRuleResult> Validate(Loan loan)
    {
        return [.. _rules.Select(rule => rule.Evaluate(loan))];
    }
}