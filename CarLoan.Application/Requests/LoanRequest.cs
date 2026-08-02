using CarLoan.Domain.Models;

namespace CarLoan.Application.Requests;

public sealed record LoanRequest(
    decimal PurchasePrice,
    decimal DownPayment,
    int LoanPeriodInMonths,
    CarCondition CarCondition);