using CarLoan.Application.Requests;
using CarLoan.Domain.Calculators;
using CarLoan.Domain.Models;
using FluentAssertions;

namespace CarLoan.Application.Tests;

/// <summary>
/// The longest term the result reports must be exactly where the lender's term and car-age rules
/// start failing (T1–T3, C1–C2): the screen puts its marker there and the rule message appears
/// past it (US-6 AC8–9). Checked over new and used cars, both sides of the 80% threshold, above
/// the 90% cap, and car ages either side of the 12- and 20-year caps.
/// </summary>
public class LykillLongestTermTests
{
    private const int MinimumTerm = 6;

    private readonly MultiLenderLoanApplicationService _service =
        new(new LoanCalculator(), LenderProfiles.Build());

    public static TheoryData<RequestedCarCondition, decimal, int> Cars
    {
        get
        {
            var cars = new TheoryData<RequestedCarCondition, decimal, int>();

            // 80%, 85% and 95% of a 5,000,000 price.
            foreach (decimal downPayment in new[] { 1_000_000m, 750_000m, 250_000m })
            {
                cars.Add(RequestedCarCondition.New, downPayment, 0);

                foreach (int age in new[] { 0, 2, 5, 6, 8, 11, 12, 13, 14, 19, 20, 25 })
                    cars.Add(RequestedCarCondition.Used, downPayment, age);
            }

            return cars;
        }
    }

    private IEnumerable<LoanRuleCode> TermRuleFailures(LoanRequest request) =>
        _service.EvaluateLoanRequest(request)
            .Should().BeOfType<LoanEvaluationOutcome.Evaluated>().Subject
            .Lenders.Single(result => result.LenderName == "Lykill")
            .ValidationResults
            .Where(rule => !rule.IsValid && rule.Code is LoanRuleCode.MaximumLoanPeriod or LoanRuleCode.CarAge)
            .Select(rule => rule.Code);

    private int? LongestTerm(LoanRequest request) =>
        _service.EvaluateLoanRequest(request)
            .Should().BeOfType<LoanEvaluationOutcome.Evaluated>().Subject
            .Lenders.Single(result => result.LenderName == "Lykill")
            .MaximumLoanPeriodInMonths;

    [Theory]
    [MemberData(nameof(Cars))]
    public void EvaluateLoanRequest_ReportsLongestTermWhereTermRulesStartFailing_WhenTheTermIsAllowed(
        RequestedCarCondition condition, decimal downPayment, int carAge)
    {
        var request = new LoanRequest(5_000_000m, downPayment, 60, condition, RequestedVehicleCategory.ElectricOrHydrogen, carAge);
        int longest = LongestTerm(request) ?? throw new InvalidOperationException("Lykill's term rules always limit the term.");

        if (longest < MinimumTerm)
        {
            // No term is allowed: the car alone reaches its age cap.
            TermRuleFailures(request with { LoanPeriodInMonths = MinimumTerm }).Should().Contain(LoanRuleCode.CarAge);
            return;
        }

        TermRuleFailures(request with { LoanPeriodInMonths = longest }).Should().BeEmpty();
        TermRuleFailures(request with { LoanPeriodInMonths = longest + 1 }).Should().NotBeEmpty();
    }
}
