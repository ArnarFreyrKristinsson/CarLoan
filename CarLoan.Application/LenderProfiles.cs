using CarLoan.Domain.Fees;
using CarLoan.Domain.Lenders;
using CarLoan.Domain.Providers;
using CarLoan.Domain.Validators;

namespace CarLoan.Application;

public static class LenderProfiles
{
    private static readonly LenderSettings _lykillLenderSettings = new(
            Name: "Lykill",
            MinAmount: 750_000m,
            MaxAmount: 30_000_000m,
            MinPeriod: 1,
            MinDownPayment: 150_000m,
            PeriodLimits: new LoanPeriodLimits(90m, 80m, 84, 72),
            CarAgeLimits: new CarAgeLimits(80m, 12, 20),
            GeneralRateTiers:
            [
                new RateTier(50m, 10.35m),
                new RateTier(70m, 11.20m),
                new RateTier(80m, 11.45m),
                new RateTier(90m, 12.20m)
            ],
            GreenRateTiers:
            [
                new RateTier(50m, 9.65m),
                new RateTier(70m, 10.50m),
                new RateTier(80m, 10.75m),
                new RateTier(90m, 11.50m)
            ],
            DefaultGeneralInterestRate: 12.20m,
            DefaultGreenInterestRate: 11.50m,
            FeeSettings: new OriginationFeeSettings(
                Tiers:
                [
                    new FeeTier(23, 1.80m),
                    new FeeTier(35, 2.00m),
                    new FeeTier(47, 2.25m),
                    new FeeTier(59, 2.50m),
                    new FeeTier(71, 3.00m),
                    new FeeTier(84, 3.20m)
                ],
                MinimumFee: 18_000m,
                GreenFeeDiscountPercentage: 50m,
                PlugInHybridRateDiscount: 1.00m));

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
        decimal MaxAmount,
        int MinPeriod,
        decimal MinDownPayment,
        LoanPeriodLimits PeriodLimits,
        CarAgeLimits CarAgeLimits,
        IReadOnlyList<RateTier> GeneralRateTiers,
        IReadOnlyList<RateTier> GreenRateTiers,
        decimal DefaultGeneralInterestRate,
        decimal DefaultGreenInterestRate,
        OriginationFeeSettings FeeSettings);

    private static IEnumerable<ILoanRule> StandardRules(LenderSettings lenderSettings) =>
    [
        new MinimumLoanAmountValidator(lenderSettings.MinAmount),
        new MaximumLoanAmountValidator(lenderSettings.MaxAmount),
        new MinimumLoanPeriodValidator(lenderSettings.MinPeriod),
        new MinimumDownPaymentValidator(lenderSettings.MinDownPayment),
        new MaximumLoanPeriodValidator(lenderSettings.PeriodLimits),
        new CarAgeValidator(lenderSettings.CarAgeLimits)
    ];

    private static LoanInterestRateProvider BuildRateProvider(LenderSettings lenderSettings) =>
        new(new RateTable(lenderSettings.GeneralRateTiers, lenderSettings.DefaultGeneralInterestRate),
            new RateTable(lenderSettings.GreenRateTiers, lenderSettings.DefaultGreenInterestRate));

    private static LenderProfile CreateStandardProfile(LenderSettings lenderSettings) =>
        new(lenderSettings.Name,
            [.. StandardRules(lenderSettings)],
            BuildRateProvider(lenderSettings),
            new OriginationFeeCalculator(lenderSettings.FeeSettings));
}
