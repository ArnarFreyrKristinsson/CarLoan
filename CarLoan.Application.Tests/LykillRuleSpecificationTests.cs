using CarLoan.Application.Requests;
using CarLoan.Domain.Calculators;
using CarLoan.Domain.Models;
using FluentAssertions;

namespace CarLoan.Application.Tests;

/// <summary>
/// Walks the published Lykill rules end to end, one scenario per branch of the schedule.
/// The expected payments come from an independent recomputation of the standard formula.
/// </summary>
public class LykillRuleSpecificationTests
{
    private readonly MultiLenderLoanApplicationService _service =
        new(new LoanCalculator(), LenderProfiles.Build());

    private LenderLoanEvaluationResult Evaluate(LoanRequest request) =>
        _service.EvaluateLoanRequest(request)
            .Should().BeOfType<LoanEvaluationOutcome.Evaluated>().Subject
            .Lenders.Single(result => result.LenderName == "Lykill");

    [Fact]
    public void EvaluateLoanRequest_PricesGreenLoanOnGreenSchedule_WhenUsedElectricCarWithinEveryRule()
    {
        // 4,000,000 loan at 80% LTV over 84 months, a 5-year-old car ages to 12, within the 20-year cap.
        var request = new LoanRequest(
            5_000_000m, 1_000_000m, 84, RequestedCarCondition.Used, RequestedVehicleCategory.ElectricOrHydrogen, 5);

        var result = Evaluate(request);

        result.ValidationResults.Should().OnlyContain(rule => rule.IsValid);
        result.InterestRate.Should().Be(10.75m);
        result.OriginationFee.EffectiveRate.Should().Be(1.60m);
        result.OriginationFee.Amount.Should().Be(64_000m);
        result.OriginationFee.AmountSaved.Should().Be(64_000m);
        result.MonthlyPayment.Should().Be(69_052.53m);
    }

    [Fact]
    public void EvaluateLoanRequest_PricesPlugInHybridOnGeneralSchedule_WhenNewPlugInHybridWithinEveryRule()
    {
        // 3,200,000 loan at 80% LTV over 60 months.
        var request = new LoanRequest(
            4_000_000m, 800_000m, 60, RequestedCarCondition.New, RequestedVehicleCategory.PlugInHybrid, 0);

        var result = Evaluate(request);

        result.ValidationResults.Should().OnlyContain(rule => rule.IsValid);
        result.InterestRate.Should().Be(11.45m);
        result.OriginationFee.EffectiveRate.Should().Be(2.00m);
        result.OriginationFee.Amount.Should().Be(64_000m);
        result.OriginationFee.AmountSaved.Should().Be(32_000m);
        result.MonthlyPayment.Should().Be(71_701.97m);
    }

    [Fact]
    public void EvaluateLoanRequest_FailsTermAndCarAgeRules_WhenUsedCarAboveEightyPercentRunsFullTerm()
    {
        // 85% LTV on a used car caps the term at 72 months, and 6 + 7 = 13 breaks the 12-year cap.
        var request = new LoanRequest(
            4_000_000m, 600_000m, 84, RequestedCarCondition.Used, RequestedVehicleCategory.PetrolOrDiesel, 6);

        var result = Evaluate(request);

        var failedRules = result.ValidationResults.Where(rule => !rule.IsValid).Select(rule => rule.Code);
        failedRules.Should().BeEquivalentTo([LoanRuleCode.MaximumLoanPeriod, LoanRuleCode.CarAge]);
    }

    [Fact]
    public void EvaluateLoanRequest_FailsOnlyLoanRatioRule_WhenNewCarAboveNinetyPercentWithinTerm()
    {
        // 95% LTV breaks T1's 90% cap; 60 months is within T1's 84.
        var request = new LoanRequest(
            4_000_000m, 200_000m, 60, RequestedCarCondition.New, RequestedVehicleCategory.PetrolOrDiesel, 0);

        var result = Evaluate(request);

        var failedRules = result.ValidationResults.Where(rule => !rule.IsValid).Select(rule => rule.Code);
        failedRules.Should().BeEquivalentTo([LoanRuleCode.MaximumLoanRatio]);
    }

    [Fact]
    public void EvaluateLoanRequest_FailsLoanRatioAndTermRules_WhenUsedCarAboveNinetyPercentRunsFullTerm()
    {
        // 95% LTV breaks T3's 90% cap, and 84 months breaks T3's 72-month term.
        var request = new LoanRequest(
            4_000_000m, 200_000m, 84, RequestedCarCondition.Used, RequestedVehicleCategory.PetrolOrDiesel, 0);

        var result = Evaluate(request);

        var failedRules = result.ValidationResults.Where(rule => !rule.IsValid).Select(rule => rule.Code);
        failedRules.Should().BeEquivalentTo([LoanRuleCode.MaximumLoanRatio, LoanRuleCode.MaximumLoanPeriod]);
    }

    [Fact]
    public void EvaluateLoanRequest_FailsMaximumLoanAmountRule_WhenLoanAmountAboveThirtyMillion()
    {
        var request = new LoanRequest(
            50_000_000m, 15_000_000m, 84, RequestedCarCondition.New, RequestedVehicleCategory.PetrolOrDiesel, 0);

        var result = Evaluate(request);

        var failedRules = result.ValidationResults.Where(rule => !rule.IsValid).Select(rule => rule.Code);
        failedRules.Should().Contain(LoanRuleCode.MaximumLoanAmount);
    }

    [Fact]
    public void EvaluateLoanRequest_FailsAmountAndDownPaymentRules_WhenLoanIsBelowBothMinimums()
    {
        var request = new LoanRequest(
            800_000m, 100_000m, 36, RequestedCarCondition.New, RequestedVehicleCategory.PetrolOrDiesel, 0);

        var result = Evaluate(request);

        var failedRules = result.ValidationResults.Where(rule => !rule.IsValid).Select(rule => rule.Code);
        failedRules.Should().BeEquivalentTo([LoanRuleCode.MinimumLoanAmount, LoanRuleCode.MinimumDownPayment]);
    }

    [Fact]
    public void EvaluateLoanRequest_FailsMinimumLoanPeriodRule_WhenTermIsShorterThanSixMonths()
    {
        var request = new LoanRequest(
            1_000_000m, 250_000m, 5, RequestedCarCondition.New, RequestedVehicleCategory.PetrolOrDiesel, 0);

        var result = Evaluate(request);

        var failedRules = result.ValidationResults.Where(rule => !rule.IsValid).Select(rule => rule.Code);
        failedRules.Should().BeEquivalentTo([LoanRuleCode.MinimumLoanPeriod]);
    }

    [Fact]
    public void EvaluateLoanRequest_PassesEveryRule_WhenTermIsExactlySixMonths()
    {
        var request = new LoanRequest(
            1_000_000m, 250_000m, 6, RequestedCarCondition.New, RequestedVehicleCategory.PetrolOrDiesel, 0);

        var result = Evaluate(request);

        result.ValidationResults.Should().OnlyContain(rule => rule.IsValid);
    }

    [Fact]
    public void EvaluateLoanRequest_RaisesFeeToMinimum_WhenComputedFeeIsBelowEighteenThousand()
    {
        // 750,000 loan over 12 months: 1.80% is 13,500, below the 18,000 minimum.
        var request = new LoanRequest(
            1_000_000m, 250_000m, 12, RequestedCarCondition.New, RequestedVehicleCategory.PetrolOrDiesel, 0);

        var result = Evaluate(request);

        result.ValidationResults.Should().OnlyContain(rule => rule.IsValid);
        result.OriginationFee.Amount.Should().Be(18_000m);
        result.OriginationFee.AmountSaved.Should().Be(0m);
    }
}
