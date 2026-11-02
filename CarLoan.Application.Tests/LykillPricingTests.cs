using CarLoan.Application.Requests;
using CarLoan.Domain.Calculators;
using CarLoan.Domain.Fees;
using CarLoan.Domain.Models;
using FluentAssertions;

namespace CarLoan.Application.Tests;

/// <summary>
/// The figures the calculator screen shows, priced by the configured Lykill profile. Expected
/// values come from <c>docs/rules/lykill.md</c> and the reference example in
/// <c>docs/spec/user-stories.md</c>, worked by hand.
/// </summary>
public class LykillPricingTests
{
    private const RequestedCarCondition New = RequestedCarCondition.New;
    private const RequestedCarCondition Used = RequestedCarCondition.Used;
    private const RequestedVehicleCategory Green = RequestedVehicleCategory.ElectricOrHydrogen;

    private readonly MultiLenderLoanApplicationService _service =
        new(new LoanCalculator(), LenderProfiles.Build());

    // The stories' reference example: a used electric car, 5,000,000 with 1,000,000 down, 84 months.
    private static readonly LoanRequest _reference = new(5_000_000m, 1_000_000m, 84, Used, Green, 5);

    private LoanEvaluationOutcome.Evaluated Evaluate(LoanRequest request) =>
        _service.EvaluateLoanRequest(request).Should().BeOfType<LoanEvaluationOutcome.Evaluated>().Subject;

    private LenderLoanEvaluationResult Lykill(LoanRequest request) =>
        Evaluate(request).Lenders.Single(result => result.LenderName == "Lykill");

    [Fact]
    public void EvaluateLoanRequest_ReportsLoanFigures_WhenReferenceExampleRequested()
    {
        // 4,000,000 of 5,000,000 is 80.0%, leaving 20.0% as down payment.
        Evaluate(_reference).Loan.Should().Be(new LoanFigures(4_000_000m, 80.0m, 20.0m));
    }

    [Fact]
    public void EvaluateLoanRequest_ReportsEveryFigureBehindThePayment_WhenReferenceExampleRequested()
    {
        // R-GRN-3 10.75%. FR6 3.20% halved by F1: 1.60%, 64,000, which F6 counts as saved in full.
        // F5: 4,064,000 financed. §7: 69,052.53 a month. P5: 69,052.53 × 84 − 4,064,000 = 1,736,412.52.
        // C2 allows 20 − 5 = 15 years and T2 84 months, so 84 is the longest term.
        var lykill = Lykill(_reference);

        lykill.IsAccepted.Should().BeTrue();
        lykill.InterestRate.Should().Be(10.75m);
        lykill.OriginationFee.Should().Be(new OriginationFee(64_000m, 64_000m, 1.60m, false));
        lykill.FinancedAmount.Should().Be(4_064_000m);
        lykill.MonthlyPayment.Should().Be(69_052.53m);
        lykill.TotalInterest.Should().Be(1_736_412.52m);
        lykill.PricingGaps.Should().BeEmpty();
        lykill.MaximumLoanPeriodInMonths.Should().Be(84);
    }

    [Theory]
    [InlineData(998_500, 80.1, 19.9)]
    [InlineData(999_990, 80.1, 19.9)]
    [InlineData(1_000_000, 80.0, 20.0)]
    [InlineData(1_002_000, 80.0, 20.0)]
    public void EvaluateLoanRequest_RoundsFinancingRatioUpToOneDecimal_WhenRatioHasMoreDecimals(
        decimal downPayment, decimal expectedRatio, decimal expectedDownPaymentPercentage)
    {
        // Stories, Language 4–5: 80.03% shows as 80.1%, 80.0002% as 80.1%, 79.96% as 80.0%;
        // the down payment percentage is 100 minus the ratio as shown.
        var figures = Evaluate(_reference with { DownPayment = downPayment }).Loan;

        figures.FinancingRatioRoundedUp.Should().Be(expectedRatio);
        figures.DownPaymentPercentage.Should().Be(expectedDownPaymentPercentage);
    }

    [Fact]
    public void EvaluateLoanRequest_ReportsNoRateAndNoPayment_WhenFinancingRatioAboveEveryRateBand()
    {
        // §5 has no band above 90%. 4,600,000 of 5,000,000 is 92%. The fee still exists:
        // FR5 3.00% halved by F1 is 1.50% of 4,600,000 = 69,000, financed 4,669,000.
        var lykill = Lykill(new LoanRequest(5_000_000m, 400_000m, 60, New, Green, 0));

        lykill.InterestRate.Should().BeNull();
        lykill.MonthlyPayment.Should().BeNull();
        lykill.TotalInterest.Should().BeNull();
        lykill.OriginationFee!.Amount.Should().Be(69_000m);
        lykill.FinancedAmount.Should().Be(4_669_000m);
        lykill.PricingGaps.Should().BeEquivalentTo([new PricingGap(PricingGapCode.InterestRate, 0m, 90m)]);
    }

    [Fact]
    public void EvaluateLoanRequest_PricesTopRateBand_WhenFinancingRatioExactlyNinety()
    {
        // R-GRN-4: above 80% up to and including 90%, 11.50%.
        var lykill = Lykill(new LoanRequest(5_000_000m, 500_000m, 60, New, Green, 0));

        lykill.InterestRate.Should().Be(11.50m);
        lykill.MonthlyPayment.Should().NotBeNull();
        lykill.PricingGaps.Should().BeEmpty();
    }

    [Fact]
    public void EvaluateLoanRequest_ReportsNoRate_WhenFinancingRatioJustAboveNinety()
    {
        // 4,500,001 of 5,000,000 is 90.00002%, past the last band.
        var lykill = Lykill(new LoanRequest(5_000_000m, 499_999m, 60, New, Green, 0));

        lykill.InterestRate.Should().BeNull();
        lykill.PricingGaps.Should().BeEquivalentTo([new PricingGap(PricingGapCode.InterestRate, 0m, 90m)]);
    }

    [Theory]
    [InlineData(5)]
    [InlineData(85)]
    public void EvaluateLoanRequest_ReportsNoFeeAndNoPayment_WhenTermOutsideFeeSchedule(int months)
    {
        // §6: FR1 starts at 6 months and FR6 ends at 84. The rate still exists: R-GRN-3 at 80%.
        var lykill = Lykill(_reference with { LoanPeriodInMonths = months });

        lykill.OriginationFee.Should().BeNull();
        lykill.FinancedAmount.Should().BeNull();
        lykill.MonthlyPayment.Should().BeNull();
        lykill.TotalInterest.Should().BeNull();
        lykill.InterestRate.Should().Be(10.75m);
        lykill.PricingGaps.Should().BeEquivalentTo([new PricingGap(PricingGapCode.OriginationFee, 6m, 84m)]);
    }

    [Theory]
    [InlineData(6)]
    [InlineData(84)]
    public void EvaluateLoanRequest_PricesFee_WhenTermAtEitherEndOfFeeSchedule(int months)
    {
        var lykill = Lykill(_reference with { LoanPeriodInMonths = months });

        lykill.OriginationFee.Should().NotBeNull();
        lykill.MonthlyPayment.Should().NotBeNull();
        lykill.PricingGaps.Should().BeEmpty();
    }

    [Fact]
    public void EvaluateLoanRequest_ReportsBothGaps_WhenNeitherRateNorFeeExists()
    {
        var lykill = Lykill(new LoanRequest(5_000_000m, 400_000m, 85, New, Green, 0));

        lykill.PricingGaps.Should().BeEquivalentTo(
        [
            new PricingGap(PricingGapCode.InterestRate, 0m, 90m),
            new PricingGap(PricingGapCode.OriginationFee, 6m, 84m)
        ]);
    }

    [Fact]
    public void EvaluateLoanRequest_PricesDeclinedLoan_WhenRuleFailsButScheduleCoversIt()
    {
        // 4,250,000 of 5,000,000 is 85%: T3 caps a used car at 72 months, so 84 is declined, but
        // R-GRN-4 and FR6 both price it.
        var lykill = Lykill(_reference with { DownPayment = 750_000m });

        lykill.IsAccepted.Should().BeFalse();
        lykill.InterestRate.Should().Be(11.50m);
        lykill.MonthlyPayment.Should().NotBeNull();
        lykill.TotalInterest.Should().NotBeNull();
    }

    [Theory]
    [InlineData(New, 0, 1_000_000, 84)]
    [InlineData(Used, 8, 750_000, 48)]
    [InlineData(Used, 2, 750_000, 72)]
    [InlineData(Used, 8, 1_000_000, 84)]
    [InlineData(Used, 13, 1_000_000, 84)]
    [InlineData(Used, 14, 1_000_000, 72)]
    [InlineData(Used, 12, 750_000, 0)]
    public void EvaluateLoanRequest_ReportsLongestAllowedTerm_WhenTermAndAgeRulesLimitIt(
        RequestedCarCondition condition, int carAge, decimal downPayment, int expectedMonths)
    {
        // T1 new: 84. At 85% (750,000 down): T3 72, C1 (12 − age) × 12, so 48 for 8 years, T3's 72
        // for 2 years, 0 for 12. At 80% (1,000,000 down): T2 84, C2 (20 − age) × 12, so 84 for
        // 8 and 13 years, 72 for 14. The loan's own 84-month term plays no part.
        var lykill = Lykill(_reference with { CarCondition = condition, CarAgeInYears = carAge, DownPayment = downPayment });

        lykill.MaximumLoanPeriodInMonths.Should().Be(expectedMonths);
    }
}
