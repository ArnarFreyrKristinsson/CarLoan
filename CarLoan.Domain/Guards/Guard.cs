namespace CarLoan.Domain.Guards;

internal static class Guard
{
    public static decimal Positive(decimal value, string parameterName) =>
        value > 0
            ? value
            : throw new ArgumentOutOfRangeException(parameterName, value, "Value must be positive.");

    public static int Positive(int value, string parameterName) =>
        value > 0
            ? value
            : throw new ArgumentOutOfRangeException(parameterName, value, "Value must be positive.");
}
