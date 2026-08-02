using CarLoan.Domain.Calculators;
using CarLoan.Domain.Lenders;
using CarLoan.Domain.Models;
using CarLoan.Domain.Providers;
using CarLoan.Domain.Validators;
using FluentAssertions;

namespace CarLoan.Application.Tests;

public class MultiLenderLoanApplicationServiceTests
{
    private static readonly LoanTerms _defaultLoanTerms = new(2000000m, 1000000m, 84, 0m);
    private static readonly Car _defaultCar = new(CarCondition.New);
    private static readonly Loan _defaultLoan = new(_defaultLoanTerms, _defaultCar);

    private static readonly IReadOnlyDictionary<string, LenderProfile> _profiles = new Dictionary<string, LenderProfile>
    {
        ["LenderA"] = new LenderProfile(
            "LenderA",
            [new MinimumDownPaymentValidator(150000m)],
            new LoanInterestRateProvider([new RateTier(200000m, 10.00m)], 12.00m)),
        ["LenderB"] = new LenderProfile(
            "LenderB",
            [new MinimumDownPaymentValidator(2000000m)],
            new LoanInterestRateProvider([new RateTier(200000m, 8.00m)], 11.00m))
    };

    private readonly MultiLenderLoanApplicationService _service = new(new LoanCalculator(), _profiles);

    [Fact]
    public void EvaluateLoan_LabelsEachResultWithLenderName_WhenMultipleLendersProvided()
    {
        var results = _service.EvaluateLoan(_defaultLoan);

        results.Select(result => result.LenderName).Should().BeEquivalentTo(_profiles.Keys);
    }

    [Fact]
    public void EvaluateLoan_ValidatesAgainstEachLendersRules_WhenRulesDiffer()
    {
        var results = _service.EvaluateLoan(_defaultLoan);

        var lenderA = results.Single(result => result.LenderName == "LenderA");
        var lenderB = results.Single(result => result.LenderName == "LenderB");

        Assert.All(lenderA.ValidationResults, ruleResult => Assert.True(ruleResult.IsValid));
        Assert.Contains(lenderB.ValidationResults, ruleResult => !ruleResult.IsValid);
    }
}
