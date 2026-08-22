using CarLoan.Domain.Lenders;
using CarLoan.Domain.Providers;
using CarLoan.Domain.Validators;

namespace CarLoan.Application;

public static class LenderProfiles
{
    private static readonly LenderSettings _lykillLenderSettings = new(
            Name: "Lykill",
            MinAmount: 750000m,
            MinPeriod: 6,
            MinDownPayment: 150000m,
            PeriodLimits: new LoanPeriodLimits(90m, 80m, 84, 72),
            RateTiers:
            [
                new RateTier(1000000m, 10.35m),
                new RateTier(600000m, 11.20m),
                new RateTier(400000m, 11.45m),
                new RateTier(200000m, 12.20m)
            ],
            DefaultInterestRate: 12.20m);
    public static IReadOnlyDictionary<string, LenderProfile> Build()
    {
        var lykill = CreateStandardProfile(_lykillLenderSettings);

        return new Dictionary<string, LenderProfile>
        {
            [lykill.Name] = lykill
        };
    }

    private sealed record LenderSettings(
        string Name,
        decimal MinAmount,
        int MinPeriod,
        decimal MinDownPayment,
        LoanPeriodLimits PeriodLimits,
        IReadOnlyList<RateTier> RateTiers,
        decimal DefaultInterestRate);

    private static IEnumerable<ILoanRule> StandardRules(LenderSettings lenderSettings) =>
    [
        new MinimumLoanAmountValidator(lenderSettings.MinAmount),
        new MinimumLoanPeriodValidator(lenderSettings.MinPeriod),
        new MinimumDownPaymentValidator(lenderSettings.MinDownPayment),
        new MaximumLoanPeriodValidator(lenderSettings.PeriodLimits)
    ];

    private static LoanInterestRateProvider BuildRateProvider(LenderSettings lenderSettings) =>
     new(lenderSettings.RateTiers, lenderSettings.DefaultInterestRate);

    private static LenderProfile CreateStandardProfile(LenderSettings lenderSettings) =>
        new(lenderSettings.Name, [.. StandardRules(lenderSettings)], BuildRateProvider(lenderSettings));
}