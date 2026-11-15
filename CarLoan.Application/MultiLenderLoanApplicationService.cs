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

    public LoanEvaluationOutcome EvaluateLoanRequest(LoanRequest request)
    {
        var inputErrors = LoanRequestValidator.Validate(request);

        if (inputErrors.Count > 0)
            return new LoanEvaluationOutcome.InvalidRequest(inputErrors);

        var loan = LoanRequestMapper.ToLoan(request);

        return new LoanEvaluationOutcome.Evaluated(EvaluateLoan(loan), LoanFigures.From(loan.Terms));
    }

    private static Dictionary<string, LenderProfile> SnapshotProfiles(
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

    /// <summary>
    /// Prices the loan wherever the lender's schedule has a price for it, and validates it. The
    /// rules run against the pre-fee loan amount, which no price changes. A payment needs both
    /// the rate and the fee, so a missing one leaves it out, and the gaps say why.
    /// </summary>
    private LenderLoanEvaluationResult EvaluateForLender(LenderProfile profile, Loan loan)
    {
        decimal? interestRate = profile.RateProvider.GetInterestRate(loan);
        var fee = profile.FeeCalculator.Calculate(loan);
        IReadOnlyList<PricingGap> pricingGaps =
            [.. new[] { profile.RateProvider.FindPricingGap(loan), profile.FeeCalculator.FindPricingGap(loan) }.OfType<PricingGap>()];

        var financedTerms = fee is null ? null : loan.Terms with { OriginationFee = fee.Amount };
        var pricedTerms = interestRate is decimal rate && financedTerms is not null
            ? financedTerms with { InterestRate = rate }
            : null;

        return new LenderLoanEvaluationResult(
            profile.Name,
            new LoanValidator(profile.Rules).Validate(loan),
            pricedTerms is null ? null : _loanCalculator.CalculateMonthlyPayment(pricedTerms),
            interestRate,
            fee,
            financedTerms?.FinancedAmount,
            pricedTerms is null ? null : _loanCalculator.CalculateTotalInterest(pricedTerms),
            pricingGaps,
            MaximumLoanPeriodInMonths(profile, loan));
    }

    /// <summary>
    /// The tightest limit among the lender's rules that cap the term; null when none does.
    /// </summary>
    private static int? MaximumLoanPeriodInMonths(LenderProfile profile, Loan loan) =>
        profile.Rules.OfType<ILoanPeriodCap>().Min(cap => cap.MaximumLoanPeriodInMonths(loan));
}