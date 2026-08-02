using CarLoan.Domain.Lenders;
using CarLoan.Domain.Providers;
using CarLoan.Domain.Validators;

namespace CarLoan.Application;

public static class LenderProfiles
{
    private readonly static LenderSettings LykillLenderSettings = new(
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
            LowestInterestRate: 12.20m);
    public static IReadOnlyDictionary<string, LenderProfile> Build()
    {
        var lykill = CreateStandardProfile(LykillLenderSettings);

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
        decimal LowestInterestRate);

    private static IEnumerable<ILoanRule> StandardRules(LenderSettings lendersettings) =>
    [
        new MinimumLoanAmountValidator(lendersettings.MinAmount),
        new MinimumLoanPeriodValidator(lendersettings.MinPeriod),
        new MinimumDownPaymentValidator(lendersettings.MinDownPayment),
        new MaximumLoanPeriodValidator(lendersettings.PeriodLimits)
    ];

    private static LoanInterestRateProvider BuildRateProvider(LenderSettings lendersettings) =>
     new(lendersettings.RateTiers, lendersettings.LowestInterestRate);

    private static LenderProfile CreateStandardProfile(LenderSettings lendersettings) =>
        new(lendersettings.Name, [.. StandardRules(lendersettings)], BuildRateProvider(lendersettings));
}