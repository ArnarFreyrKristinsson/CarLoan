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

    [Theory]
    [MemberData(nameof(ExtremeWellFormedRequests))]
    public void EvaluateLoanRequest_EvaluatesWithoutNegativeAmounts_WhenRequestHasNoInputErrors(LoanRequest request)
    {
        LoanRequestValidator.Validate(request).Should().BeEmpty();

        var outcome = _service.EvaluateLoanRequest(request);

        var lenders = outcome.Should().BeOfType<LoanEvaluationOutcome.Evaluated>().Subject.Lenders;
        lenders.Should().AllSatisfy(lender =>
        {
            lender.MonthlyPayment.Should().BeGreaterThanOrEqualTo(0m);
            lender.OriginationFee.Amount.Should().BeGreaterThanOrEqualTo(0m);
        });
    }
}
