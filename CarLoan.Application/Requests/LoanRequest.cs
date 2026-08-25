namespace CarLoan.Application.Requests;

public sealed record LoanRequest(
    decimal PurchasePrice,
    decimal DownPayment,
    int LoanPeriodInMonths,
    RequestedCarCondition CarCondition,
    RequestedVehicleCategory VehicleCategory,
    int CarAgeInYears);
