using CarLoan.Application.Requests;
using CarLoan.Domain.Models;

namespace CarLoan.Application.Mapping;

public static class LoanRequestMapper
{
    public static Loan ToLoan(this LoanRequest request) =>
        new(
            new LoanTerms(
                request.PurchasePrice,
                request.DownPayment,
                request.LoanPeriodInMonths,
                InterestRate: 0m),
            new Car(request.CarCondition));
}
