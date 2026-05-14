namespace CarLoan.Domain.Models;

public sealed record Loan(LoanTerms Terms, Car Car);
