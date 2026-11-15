using CarLoan.Application.Requests;
using CarLoan.Domain.Calculators;
using FluentAssertions;

namespace CarLoan.Application.Tests;

/// <summary>
/// The contract promises that a request with no input errors is always evaluated: no exception,
/// no negative amounts. These are the extremes the input bounds still allow.
/// </summary>
public class WellFormedRequestGuaranteeTests
{
    private readonly MultiLenderLoanApplicationService _service =
        new(new LoanCalculator(), LenderProfiles.Build());

    public static TheoryData<LoanRequest> ExtremeWellFormedRequests
    {
        get
        {
            var requests = new TheoryData<LoanRequest>();

            foreach (var category in Enum.GetValues<RequestedVehicleCategory>())
            {
                foreach (var condition in Enum.GetValues<RequestedCarCondition>())
                {
                    requests.Add(new(1_000_000_000_000m, 0m, 1_200, condition, category, int.MaxValue));
                    requests.Add(new(1_000_000_000_000m, 999_999_999_999.99m, 1, condition, category, 0));
                    requests.Add(new(0.01m, 0m, 1, condition, category, 0));
                    requests.Add(new(0.01m, 0m, 1_200, condition, category, int.MaxValue));
                }
            }

            return requests;
        }
    }

    /// <summary>
    /// The far ends the Lykill schedules still price — a 90% loan on the largest price over the
    /// longest term, and a 50% loan on the smallest over the shortest — so every amount exists.
    /// </summary>
    public static TheoryData<LoanRequest> PricedExtremeWellFormedRequests
    {
        get
        {
            var requests = new TheoryData<LoanRequest>();

            foreach (var category in Enum.GetValues<RequestedVehicleCategory>())
            {
                foreach (var condition in Enum.GetValues<RequestedCarCondition>())
                {
                    requests.Add(new(1_000_000_000_000m, 100_000_000_000m, 84, condition, category, 0));
                    requests.Add(new(0.02m, 0.01m, 6, condition, category, 0));
                }
            }

            return requests;
        }
    }

    [Theory]
    [MemberData(nameof(ExtremeWellFormedRequests))]
    [MemberData(nameof(PricedExtremeWellFormedRequests))]
    public void EvaluateLoanRequest_EvaluatesWithoutNegativeAmounts_WhenRequestHasNoInputErrors(LoanRequest request)
    {
        LoanRequestValidator.Validate(request).Should().BeEmpty();

        var evaluated = _service.EvaluateLoanRequest(request).Should().BeOfType<LoanEvaluationOutcome.Evaluated>().Subject;

        Amounts(evaluated).Where(amount => amount is not null)
            .Should().AllSatisfy(amount => amount.Should().BeGreaterThanOrEqualTo(0m));
    }

    [Theory]
    [MemberData(nameof(PricedExtremeWellFormedRequests))]
    public void EvaluateLoanRequest_ReportsEveryAmount_WhenSchedulesPriceTheExtreme(LoanRequest request)
    {
        var evaluated = _service.EvaluateLoanRequest(request).Should().BeOfType<LoanEvaluationOutcome.Evaluated>().Subject;

        evaluated.Lenders.Should().AllSatisfy(lender => lender.PricingGaps.Should().BeEmpty());
        Amounts(evaluated).Should().NotContainNulls();
    }

    // A price the lender's schedule does not have is absent; every amount present is zero or more.
    private static IEnumerable<decimal?> Amounts(LoanEvaluationOutcome.Evaluated evaluated) =>
    [
        evaluated.Loan.LoanAmount,
        evaluated.Loan.FinancingRatioRoundedUp,
        evaluated.Loan.DownPaymentPercentage,
        .. evaluated.Lenders.SelectMany(lender => new[]
        {
            lender.MonthlyPayment,
            lender.InterestRate,
            lender.OriginationFee?.Amount,
            lender.OriginationFee?.AmountSaved,
            lender.FinancedAmount,
            lender.TotalInterest
        })
    ];
}
