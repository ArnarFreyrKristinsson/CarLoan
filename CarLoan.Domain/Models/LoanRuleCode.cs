namespace CarLoan.Domain.Models;

/// <summary>
/// What a lender rule checks, the same for every lender. The UI words its reasons from this
/// code and the result's parameters; a lender's own spec numbering never reaches the UI.
/// </summary>
public enum LoanRuleCode
{
    MinimumLoanAmount,
    MaximumLoanAmount,
    MinimumDownPayment,
    MinimumLoanPeriod,
    MaximumLoanRatio,
    MaximumLoanPeriod,
    CarAge
}
