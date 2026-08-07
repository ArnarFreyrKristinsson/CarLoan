using CarLoan.Application.Requests;
using CarLoan.Domain.Models;

namespace CarLoan.Application.Mapping;

public static class LoanRequestMapper
{
    public static Loan ToLoan(LoanRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        return new(
            new LoanTerms(
                request.PurchasePrice,
                request.DownPayment,
                request.LoanPeriodInMonths,
                InterestRate: 0m),
            new Car(ToCarCondition(request.CarCondition)));
    }

    private static CarCondition ToCarCondition(RequestedCarCondition condition) =>
        condition switch
        {
            RequestedCarCondition.New => CarCondition.New,
            RequestedCarCondition.Used => CarCondition.Used,
            _ => throw new ArgumentOutOfRangeException(nameof(condition), condition, null)
        };
}