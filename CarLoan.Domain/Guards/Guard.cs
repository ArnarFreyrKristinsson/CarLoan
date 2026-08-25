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

    public static decimal NonNegative(decimal value, string parameterName) =>
        value >= 0
            ? value
            : throw new ArgumentOutOfRangeException(parameterName, value, "Value must not be negative.");

    public static int NonNegative(int value, string parameterName) =>
        value >= 0
            ? value
            : throw new ArgumentOutOfRangeException(parameterName, value, "Value must not be negative.");

    public static decimal InRange(decimal value, decimal minimum, decimal maximum, string parameterName) =>
        value >= minimum && value <= maximum
            ? value
            : throw new ArgumentOutOfRangeException(parameterName, value, $"Value must be between {minimum} and {maximum}.");

    public static T NotNull<T>(T? value, string parameterName) where T : class =>
        value ?? throw new ArgumentNullException(parameterName);

    public static IReadOnlyList<T> NoNullElements<T>(IReadOnlyList<T> values, string parameterName) where T : class
    {
        ArgumentNullException.ThrowIfNull(values, parameterName);

        return values.Any(value => value is null)
            ? throw new ArgumentException("Collection must not contain null entries.", parameterName)
            : values;
    }
}
