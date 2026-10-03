using CarLoan.Application.Requests;

namespace CarLoan.Application;

/// <summary>
/// Either the request was malformed and nothing was evaluated, or every lender evaluated it.
/// A declined loan is an <see cref="Evaluated"/> outcome, never an <see cref="InvalidRequest"/>.
/// </summary>
public abstract record LoanEvaluationOutcome
{
    private LoanEvaluationOutcome()
    {
    }

    public sealed record InvalidRequest(IReadOnlyList<InputError> Errors) : LoanEvaluationOutcome;

    public sealed record Evaluated(IReadOnlyList<LenderLoanEvaluationResult> Lenders) : LoanEvaluationOutcome;
}
