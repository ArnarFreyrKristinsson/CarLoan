namespace CarLoan.Domain.Models;

public sealed record Car(CarCondition Condition);

public enum CarCondition
{
    New,
    Used
}
