using CarLoan.Domain.Guards;

namespace CarLoan.Domain.Models;

public sealed record Car(CarCondition Condition, VehicleCategory Category, int AgeInYears)
{
    public int AgeInYears { get; } = Guard.NonNegative(AgeInYears, nameof(AgeInYears));

    public bool IsGreen => Category == VehicleCategory.ElectricOrHydrogen;
}

public enum CarCondition
{
    New,
    Used
}
