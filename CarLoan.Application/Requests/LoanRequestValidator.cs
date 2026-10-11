namespace CarLoan.Application.Requests;

/// <summary>
/// Checks that a request is well formed enough to evaluate. Whether a lender accepts the loan
/// is a separate question, answered by the lender rules.
/// </summary>
public static class LoanRequestValidator
{
    /// <summary>Far above any car loan, so lender rules still decide every real case.</summary>
    public const decimal MaximumPurchasePrice = 1_000_000_000_000m;

    /// <summary>Far above any car loan, so lender rules still decide every real case.</summary>
    public const int MaximumLoanPeriodInMonths = 1_200;

    public static IReadOnlyList<InputError> Validate(LoanRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        List<InputError> errors = [];

        if (request.PurchasePrice <= 0)
            errors.Add(new(nameof(LoanRequest.PurchasePrice), InputErrorKind.MustBePositive));
        else if (request.PurchasePrice > MaximumPurchasePrice)
            errors.Add(new(nameof(LoanRequest.PurchasePrice), InputErrorKind.TooLarge));

        if (request.DownPayment < 0)
            errors.Add(new(nameof(LoanRequest.DownPayment), InputErrorKind.MustNotBeNegative));
        else if (request.PurchasePrice > 0 && request.DownPayment >= request.PurchasePrice)
            errors.Add(new(nameof(LoanRequest.DownPayment), InputErrorKind.MustBeLessThanPurchasePrice));

        if (request.LoanPeriodInMonths <= 0)
            errors.Add(new(nameof(LoanRequest.LoanPeriodInMonths), InputErrorKind.MustBePositive));
        else if (request.LoanPeriodInMonths > MaximumLoanPeriodInMonths)
            errors.Add(new(nameof(LoanRequest.LoanPeriodInMonths), InputErrorKind.TooLarge));

        if (!Enum.IsDefined(request.CarCondition))
            errors.Add(new(nameof(LoanRequest.CarCondition), InputErrorKind.Unsupported));

        if (!Enum.IsDefined(request.VehicleCategory))
            errors.Add(new(nameof(LoanRequest.VehicleCategory), InputErrorKind.Unsupported));

        if (request.CarAgeInYears < 0)
            errors.Add(new(nameof(LoanRequest.CarAgeInYears), InputErrorKind.MustNotBeNegative));

        return errors;
    }
}
