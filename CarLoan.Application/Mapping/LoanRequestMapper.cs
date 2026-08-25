using CarLoan.Application.Requests;
using CarLoan.Domain.Models;

namespace CarLoan.Application.Mapping;

public static class LoanRequestMapper
{
    public static Loan ToLoan(LoanRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(request.PurchasePrice);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(request.LoanPeriodInMonths);
        ArgumentOutOfRangeException.ThrowIfNegative(request.DownPayment);
        ArgumentOutOfRangeException.ThrowIfNegative(request.CarAgeInYears);

        return new(
            new LoanTerms(
                request.PurchasePrice,
                request.DownPayment,
                request.LoanPeriodInMonths,
                InterestRate: 0m),
            new Car(
                ToCarCondition(request.CarCondition),
                ToVehicleCategory(request.VehicleCategory),
                request.CarAgeInYears));
    }

    private static CarCondition ToCarCondition(RequestedCarCondition condition) =>
        condition switch
        {
            RequestedCarCondition.New => CarCondition.New,
            RequestedCarCondition.Used => CarCondition.Used,
            _ => throw new ArgumentOutOfRangeException(nameof(condition), condition, "Unsupported car condition.")
        };

    private static VehicleCategory ToVehicleCategory(RequestedVehicleCategory category) =>
        category switch
        {
            RequestedVehicleCategory.PetrolOrDiesel => VehicleCategory.PetrolOrDiesel,
            RequestedVehicleCategory.PlugInHybrid => VehicleCategory.PlugInHybrid,
            RequestedVehicleCategory.ElectricOrHydrogen => VehicleCategory.ElectricOrHydrogen,
            _ => throw new ArgumentOutOfRangeException(nameof(category), category, "Unsupported vehicle category.")
        };
}
