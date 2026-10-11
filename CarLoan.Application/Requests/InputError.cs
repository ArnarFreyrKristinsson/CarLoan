namespace CarLoan.Application.Requests;

/// <summary>
/// A request field the engine cannot evaluate. Distinct from a lender declining the loan.
/// </summary>
public sealed record InputError(string Field, InputErrorKind Kind);
