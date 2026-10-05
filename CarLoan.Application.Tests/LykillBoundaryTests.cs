using CarLoan.Application.Requests;
using CarLoan.Domain.Calculators;
using CarLoan.Domain.Models;
using FluentAssertions;

namespace CarLoan.Application.Tests;

/// <summary>
/// Pins every limit <c>docs/rules/lykill.md</c> names to the configured Lykill profile: each
/// one at the limit, then just past it. Validator tests prove the comparisons; only these prove
/// the numbers in <see cref="LenderProfiles"/>.
/// </summary>
public class LykillBoundaryTests
{
    private const RequestedCarCondition New = RequestedCarCondition.New;
    private const RequestedCarCondition Used = RequestedCarCondition.Used;

    private readonly MultiLenderLoanApplicationService _service =
        new(new LoanCalculator(), LenderProfiles.Build());

    private static LoanRequest Request(
        decimal purchasePrice, decimal downPayment, int months, RequestedCarCondition condition = New, int carAge = 0) =>
        new(purchasePrice, downPayment, months, condition, RequestedVehicleCategory.PetrolOrDiesel, carAge);

    public static TheoryData<string, LoanRequest, LoanRuleCode[]> Boundaries => new()
    {
        // A1: loan amount >= 750,000.
        { "A1 at limit", Request(1_000_000m, 250_000m, 60), [] },
        { "A1 past limit", Request(1_000_000m, 250_000.01m, 60), [LoanRuleCode.MinimumLoanAmount] },

        // A2: down payment >= 150,000. At Lykill's numbers, 150,000 down on the smallest price
        // that keeps LTV at 90% leaves no room, so just past A2 also crosses the 90% cap.
        { "A2 at limit", Request(1_500_000m, 150_000m, 60), [] },
        { "A2 past limit", Request(1_500_000m, 149_999.99m, 60), [LoanRuleCode.MinimumDownPayment, LoanRuleCode.MaximumLoanRatio] },

        // A3: loan amount <= 30,000,000.
        { "A3 at limit", Request(40_000_000m, 10_000_000m, 60), [] },
        { "A3 past limit", Request(40_000_000m, 9_999_999.99m, 60), [LoanRuleCode.MaximumLoanAmount] },

        // T1: new car, LTV <= 90% and term <= 84 months.
        { "T1 ratio at limit", Request(4_000_000m, 400_000m, 60), [] },
        { "T1 ratio past limit", Request(4_000_000m, 399_999.99m, 60), [LoanRuleCode.MaximumLoanRatio] },
        { "T1 term at limit", Request(2_000_000m, 1_000_000m, 84), [] },
        { "T1 term past limit", Request(2_000_000m, 1_000_000m, 85), [LoanRuleCode.MaximumLoanPeriod] },

        // T2: used car at LTV <= 80%, term <= 84 months.
        { "T2 term at limit", Request(2_000_000m, 400_000m, 84, Used), [] },
        { "T2 term past limit", Request(2_000_000m, 400_000m, 85, Used), [LoanRuleCode.MaximumLoanPeriod] },

        // T3: used car above 80% LTV, term <= 72 months; the 80% threshold moves 84 months into T3.
        { "T3 term at limit", Request(2_000_000m, 300_000m, 72, Used), [] },
        { "T3 term past limit", Request(2_000_000m, 300_000m, 73, Used), [LoanRuleCode.MaximumLoanPeriod] },
        { "T3 threshold past", Request(2_000_000m, 399_999.99m, 84, Used), [LoanRuleCode.MaximumLoanPeriod] },
        { "T3 ratio at limit", Request(4_000_000m, 400_000m, 60, Used), [] },
        { "T3 ratio past limit", Request(4_000_000m, 399_999.99m, 60, Used), [LoanRuleCode.MaximumLoanRatio] },

        // T4: term >= 6 months.
        { "T4 at limit", Request(1_000_000m, 250_000m, 6), [] },
        { "T4 past limit", Request(1_000_000m, 250_000m, 5), [LoanRuleCode.MinimumLoanPeriod] },

        // C1: used car above 80% LTV, car age + term <= 12 years.
        { "C1 at limit", Request(2_000_000m, 300_000m, 60, Used, 7), [] },
        { "C1 past limit", Request(2_000_000m, 300_000m, 61, Used, 7), [LoanRuleCode.CarAge] },

        // C2: used car at LTV <= 80%, car age + term <= 20 years; just above 80% the C1 cap applies.
        { "C2 at limit", Request(2_000_000m, 400_000m, 60, Used, 15), [] },
        { "C2 past limit", Request(2_000_000m, 400_000m, 61, Used, 15), [LoanRuleCode.CarAge] },
        { "C1/C2 threshold past", Request(2_000_000m, 399_999.99m, 60, Used, 15), [LoanRuleCode.CarAge] }
    };

    [Theory]
    [MemberData(nameof(Boundaries))]
    public void EvaluateLoanRequest_FailsExactlyTheExpectedRules_WhenLoanSitsOnLykillBoundary(
        string boundary, LoanRequest request, LoanRuleCode[] expectedFailures)
    {
        var lykill = _service.EvaluateLoanRequest(request)
            .Should().BeOfType<LoanEvaluationOutcome.Evaluated>().Subject
            .Lenders.Single(result => result.LenderName == "Lykill");

        lykill.ValidationResults.Where(rule => !rule.IsValid).Select(rule => rule.Code)
            .Should().BeEquivalentTo(expectedFailures, boundary);
    }
}
