using CarLoan.Application.Requests;
using CarLoan.Domain.Calculators;
using CarLoan.Domain.Fees;
using CarLoan.Domain.Lenders;
using CarLoan.Domain.Providers;
using CarLoan.Domain.Validators;
using FluentAssertions;

namespace CarLoan.Application.Tests;

public class MultiLenderLoanApplicationServiceTests
{
    private static readonly LoanRequest _defaultRequest =
        new(2_000_000m, 1_000_000m, 84, RequestedCarCondition.New, RequestedVehicleCategory.PetrolOrDiesel, 0);

    private static LoanInterestRateProvider RateProvider(decimal generalRate) =>
        new(new RateTable([new RateTier(90m, generalRate)], generalRate),
            new RateTable([new RateTier(90m, generalRate - 0.70m)], generalRate - 0.70m));

    private static OriginationFeeCalculator FeeCalculator(decimal feeRate) =>
        new(new OriginationFeeSettings([new FeeTier(84, feeRate)], 18_000m, 50m, 1.00m));

    private static readonly IReadOnlyDictionary<string, LenderProfile> _profiles = new Dictionary<string, LenderProfile>
    {
        ["LenderA"] = new LenderProfile(
            "LenderA",
            [new MinimumDownPaymentValidator(150_000m)],
            RateProvider(10.00m),
            FeeCalculator(3.20m)),
        ["LenderB"] = new LenderProfile(
            "LenderB",
            [new MinimumDownPaymentValidator(2_000_000m)],
            RateProvider(8.00m),
            FeeCalculator(2.00m))
    };

    private readonly MultiLenderLoanApplicationService _service = new(new LoanCalculator(), _profiles);

    [Fact]
    public void Constructor_ThrowsArgumentNullException_WhenLoanCalculatorIsNull()
    {
        Assert.Throws<ArgumentNullException>(() => new MultiLenderLoanApplicationService(null!, _profiles));
    }

    [Fact]
    public void Constructor_ThrowsArgumentNullException_WhenLenderProfilesIsNull()
    {
        Assert.Throws<ArgumentNullException>(() => new MultiLenderLoanApplicationService(new LoanCalculator(), null!));
    }

    [Fact]
    public void Constructor_ThrowsArgumentException_WhenLenderProfilesContainsNullProfile()
    {
        var profiles = new Dictionary<string, LenderProfile> { ["LenderA"] = null! };

        Assert.Throws<ArgumentException>(() => new MultiLenderLoanApplicationService(new LoanCalculator(), profiles));
    }

    [Fact]
    public void EvaluateLoanRequest_UsesProfilesCapturedAtConstruction_WhenCallerMutatesProfilesAfterwards()
    {
        var mutableProfiles = new Dictionary<string, LenderProfile>(_profiles);
        var service = new MultiLenderLoanApplicationService(new LoanCalculator(), mutableProfiles);

        mutableProfiles["LenderC"] = null!;

        var results = service.EvaluateLoanRequest(_defaultRequest);

        results.Select(result => result.LenderName).Should().BeEquivalentTo(_profiles.Keys);
    }

    [Fact]
    public void EvaluateLoanRequest_ThrowsArgumentNullException_WhenRequestIsNull()
    {
        Assert.Throws<ArgumentNullException>(() => _service.EvaluateLoanRequest(null!));
    }

    [Fact]
    public void EvaluateLoanRequest_LabelsEachResultWithLenderName_WhenMultipleLendersProvided()
    {
        var results = _service.EvaluateLoanRequest(_defaultRequest);

        results.Select(result => result.LenderName).Should().BeEquivalentTo(_profiles.Keys);
    }

    [Fact]
    public void EvaluateLoanRequest_ValidatesAgainstEachLendersRules_WhenRulesDiffer()
    {
        var results = _service.EvaluateLoanRequest(_defaultRequest);

        var lenderA = results.Single(result => result.LenderName == "LenderA");
        var lenderB = results.Single(result => result.LenderName == "LenderB");

        Assert.All(lenderA.ValidationResults, ruleResult => Assert.True(ruleResult.IsValid));
        Assert.Contains(lenderB.ValidationResults, ruleResult => !ruleResult.IsValid);
    }

    [Fact]
    public void EvaluateLoanRequest_AppliesEachLendersInterestRate_WhenLoanRequestProvided()
    {
        var results = _service.EvaluateLoanRequest(_defaultRequest);

        results.Select(result => result.MonthlyPayment).Should().OnlyHaveUniqueItems();
        results.Single(result => result.LenderName == "LenderA").InterestRate.Should().Be(10.00m);
        results.Single(result => result.LenderName == "LenderB").InterestRate.Should().Be(8.00m);
    }

    [Fact]
    public void EvaluateLoanRequest_UsesGreenInterestRate_WhenVehicleIsElectricOrHydrogen()
    {
        var request = _defaultRequest with { VehicleCategory = RequestedVehicleCategory.ElectricOrHydrogen };

        var results = _service.EvaluateLoanRequest(request);

        results.Single(result => result.LenderName == "LenderA").InterestRate.Should().Be(9.30m);
    }

    [Fact]
    public void EvaluateLoanRequest_ReportsEachLendersOriginationFee_WhenLoanRequestProvided()
    {
        var results = _service.EvaluateLoanRequest(_defaultRequest);

        results.Single(result => result.LenderName == "LenderA").OriginationFee.Amount.Should().Be(32_000m);
        results.Single(result => result.LenderName == "LenderB").OriginationFee.Amount.Should().Be(20_000m);
    }

    [Fact]
    public void EvaluateLoanRequest_AddsOriginationFeeOnTopOfLoan_WhenCalculatingMonthlyPayment()
    {
        var withFee = _service.EvaluateLoanRequest(_defaultRequest)
            .Single(result => result.LenderName == "LenderA");

        // LenderA finances 1,000,000 plus a 32,000 fee at 10.00% over 84 months.
        var expected = new LoanCalculator().CalculateMonthlyPayment(
            new Domain.Models.LoanTerms(2_000_000m, 1_000_000m, 84, 10.00m, OriginationFee: 32_000m));

        withFee.MonthlyPayment.Should().Be(expected);
    }
}
