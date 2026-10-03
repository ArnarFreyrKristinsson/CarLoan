namespace CarLoan.Application.Requests;

public enum InputErrorKind
{
    MustBePositive,
    MustNotBeNegative,
    Unsupported,
    TooLarge,
    MustBeLessThanPurchasePrice
}
