using CarLoan.Application.Requests;
using CarLoan.Domain.Calculators;
using CarLoan.Domain.Models;
using FluentAssertions;

namespace CarLoan.Application.Tests;

/// <summary>
/// Pins every price band <c>docs/rules/lykill.md</c> names to the configured Lykill profile,
/// through the service: each §5 rate band edge of both tables and each §6 fee band edge, at the
/// edge and just past it. Only these prove the numbers in <see cref="LenderProfiles"/>; the
/// calculator tests prove the comparisons.
/// </summary>
public class LykillPricingBoundaryTests
{
    private const RequestedVehicleCategory General = RequestedVehicleCategory.PetrolOrDiesel;
    private const RequestedVehicleCategory PlugInHybrid = RequestedVehicleCategory.PlugInHybrid;
    private const RequestedVehicleCategory Green = RequestedVehicleCategory.ElectricOrHydrogen;

    private readonly MultiLenderLoanApplicationService _service =
        new(new LoanCalculator(), LenderProfiles.Build());

    // A new car on a 2,000,000 price, so the down payment alone sets the financing ratio.
    private static LoanRequest Request(RequestedVehicleCategory category, decimal downPayment, int months = 60) =>
        new(2_000_000m, downPayment, months, RequestedCarCondition.New, category, 0);

    private LenderLoanEvaluationResult Lykill(LoanRequest request) =>
        _service.EvaluateLoanRequest(request)
            .Should().BeOfType<LoanEvaluationOutcome.Evaluated>().Subject
            .Lenders.Single(result => result.LenderName == "Lykill");

    public static TheoryData<string, LoanRequest, decimal?> RateBands => new()
    {
        // §5.1 R-GEN, V1 and V2. Bands are inclusive at the top: 50%, 70%, 80%, 90%.
        { "R-GEN-1 at 50%", Request(General, 1_000_000m), 10.35m },
        { "R-GEN-2 past 50%", Request(General, 999_999.99m), 11.20m },
        { "R-GEN-2 at 70%", Request(General, 600_000m), 11.20m },
        { "R-GEN-3 past 70%", Request(General, 599_999.99m), 11.45m },
        { "R-GEN-3 at 80%", Request(General, 400_000m), 11.45m },
        { "R-GEN-4 past 80%", Request(General, 399_999.99m), 12.20m },
        { "R-GEN-4 at 90%", Request(General, 200_000m), 12.20m },
        { "R-GEN past 90%", Request(General, 199_999.99m), null },
        { "R-GEN-4 at 90%, plug-in hybrid", Request(PlugInHybrid, 200_000m), 12.20m },
        { "R-GEN past 90%, plug-in hybrid", Request(PlugInHybrid, 199_999.99m), null },

        // §5.2 R-GRN, V3.
        { "R-GRN-1 at 50%", Request(Green, 1_000_000m), 9.65m },
        { "R-GRN-2 past 50%", Request(Green, 999_999.99m), 10.50m },
        { "R-GRN-2 at 70%", Request(Green, 600_000m), 10.50m },
        { "R-GRN-3 past 70%", Request(Green, 599_999.99m), 10.75m },
        { "R-GRN-3 at 80%", Request(Green, 400_000m), 10.75m },
        { "R-GRN-4 past 80%", Request(Green, 399_999.99m), 11.50m },
        { "R-GRN-4 at 90%", Request(Green, 200_000m), 11.50m },
        { "R-GRN past 90%", Request(Green, 199_999.99m), null }
    };

    [Theory]
    [MemberData(nameof(RateBands))]
    public void EvaluateLoanRequest_PricesRateBand_WhenFinancingRatioSitsOnBandEdge(
        string band, LoanRequest request, decimal? expectedRate)
    {
        var lykill = Lykill(request);

        lykill.InterestRate.Should().Be(expectedRate, band);

        if (expectedRate is null)
            lykill.PricingGaps.Should().BeEquivalentTo([new PricingGap(PricingGapCode.InterestRate, 0m, 90m)], band);
        else
            lykill.PricingGaps.Should().BeEmpty(band);
    }

    [Theory]
    [InlineData(5, null)]
    [InlineData(6, 1.80)]
    [InlineData(23, 1.80)]
    [InlineData(24, 2.00)]
    [InlineData(35, 2.00)]
    [InlineData(36, 2.25)]
    [InlineData(47, 2.25)]
    [InlineData(48, 2.50)]
    [InlineData(59, 2.50)]
    [InlineData(60, 3.00)]
    [InlineData(71, 3.00)]
    [InlineData(72, 3.20)]
    [InlineData(84, 3.20)]
    [InlineData(85, null)]
    public void EvaluateLoanRequest_PricesFeeBand_WhenTermSitsOnBandEdge(int months, double? expectedRate)
    {
        // §6 FR1–FR6 on a petrol car, no discount; 5 and 85 months fall outside the schedule.
        var lykill = Lykill(Request(General, 1_000_000m, months));

        decimal? effectiveRate = lykill.OriginationFee?.EffectiveRate;

        effectiveRate.Should().Be((decimal?)expectedRate);
    }
}
