using CarLoan.Application.Mapping;
using CarLoan.Application.Requests;
using CarLoan.Domain.Calculators;
using CarLoan.Domain.Lenders;
using CarLoan.Domain.Models;
using CarLoan.Domain.Validators;

namespace CarLoan.Application;

public class MultiLenderLoanApplicationService(
    ILoanCalculator loanCalculator,
    IReadOnlyDictionary<string, LenderProfile> lenderProfiles) : IMultiLenderLoanApplicationService
{
    private readonly ILoanCalculator _loanCalculator = loanCalculator ?? throw new ArgumentNullException(nameof(loanCalculator));
    private readonly IReadOnlyDictionary<string, LenderProfile> _lenderProfiles = SnapshotProfiles(lenderProfiles);

    public IReadOnlyList<LenderLoanEvaluationResult> EvaluateLoanRequest(LoanRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        return EvaluateLoan(LoanRequestMapper.ToLoan(request));
    }

    private static IReadOnlyDictionary<string, LenderProfile> SnapshotProfiles(
        IReadOnlyDictionary<string, LenderProfile> lenderProfiles)
    {
        ArgumentNullException.ThrowIfNull(lenderProfiles);

        if (lenderProfiles.Values.Any(profile => profile is null))
        {
            throw new ArgumentException("Profile in lender profiles must not be null.", nameof(lenderProfiles));
        }

        return new Dictionary<string, LenderProfile>(lenderProfiles);
    }

    private IReadOnlyList<LenderLoanEvaluationResult> EvaluateLoan(Loan loan) =>

        [.. _lenderProfiles.Values.Select(profile => EvaluateForLender(profile, loan))];

    private LenderLoanEvaluationResult EvaluateForLender(LenderProfile profile, Loan loan)
    {
        var terms = loan.Terms with
        {
            InterestRate = profile.RateProvider
            .GetInterestRate(loan.Terms.DownPayment)
        };

        return new LenderLoanEvaluationResult(
            profile.Name,
            new LoanValidator(profile.Rules).Validate(loan with { Terms = terms }),
            _loanCalculator.CalculateMonthlyPayment(terms));
    }
}
