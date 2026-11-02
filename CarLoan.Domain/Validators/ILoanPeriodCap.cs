using CarLoan.Domain.Models;

namespace CarLoan.Domain.Validators;

/// <summary>
/// A rule that limits how long a loan may run.
/// </summary>
public interface ILoanPeriodCap
{
    /// <summary>
    /// The longest term in months this rule allows for the loan's car and financing ratio,
    /// whatever term the loan has now, or null when the rule sets no limit for this loan.
    /// </summary>
    int? MaximumLoanPeriodInMonths(Loan loan);
}
