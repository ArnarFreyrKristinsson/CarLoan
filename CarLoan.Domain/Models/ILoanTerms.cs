namespace CarLoan.Domain.Models;

public interface ILoanTerms
{
    decimal InterestRate { get; }
    decimal LoanAmount { get; }
    int LoanPeriodInMonths { get; }
}
