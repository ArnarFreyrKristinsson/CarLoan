using System.Globalization;

namespace CarLoan.Web.Presentation;

/// <summary>
/// Writes numbers the way the screen shows them: Icelandic separators and whole krónur.
/// Formatting only; every value comes from the engine or from what the user typed. Month
/// words use the plural, which every term Lykill's rules name (6, 72, 84 and whole years)
/// takes.
/// </summary>
internal static class IcelandicFormat
{
    private static readonly CultureInfo _icelandic = CultureInfo.GetCultureInfo("is-IS");

    /// <summary>Whole number with "." between thousands; a half rounds up.</summary>
    public static string Number(decimal value) => value.ToString("N0", _icelandic);

    /// <summary>An amount in whole krónur: 4.064.000 kr.</summary>
    public static string Krona(decimal value) => $"{Number(value)} kr.";

    /// <summary>A rate as it applies, two decimals: 10,75%.</summary>
    public static string Rate(decimal percent) => $"{percent.ToString("N2", _icelandic)}%";

    /// <summary>A ratio with one decimal, as the engine rounded it: 80,0%.</summary>
    public static string Ratio(decimal percent) => $"{percent.ToString("N1", _icelandic)}%";

    /// <summary>A limit from the lender's rules, without trailing zeros: 90%.</summary>
    public static string Limit(decimal percent) => $"{percent.ToString("0.##", _icelandic)}%";

    /// <summary>"6 mánuðir": a term after "vera" or "er".</summary>
    public static string Months(decimal months) => $"{Number(months)} mánuðir";

    /// <summary>"48 mánuði": a term after "í".</summary>
    public static string MonthsAfterPreposition(decimal months) => $"{Number(months)} mánuði";

    /// <summary>The digits in what the user typed, with the thousands dots ignored; null when there are none.</summary>
    public static decimal? ParseDigits(string? text)
    {
        string digits = new((text ?? string.Empty).Where(char.IsAsciiDigit).ToArray());

        return decimal.TryParse(digits, NumberStyles.None, CultureInfo.InvariantCulture, out decimal value) ? value : null;
    }
}
