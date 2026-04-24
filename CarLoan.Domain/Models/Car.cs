namespace CarLoan.Domain;

public sealed record Car(CarCondition Condition);

public enum CarCondition
{
    New,
    Used
}
