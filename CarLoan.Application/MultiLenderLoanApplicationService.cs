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
    public IReadOnlyList<LenderLoanEvaluationResult> EvaluateLoanRequest(LoanRequest request) =>
        EvaluateLoan(LoanRequestMapper.ToLoan(request));

    private IReadOnlyList<LenderLoanEvaluationResult> EvaluateLoan(Loan loan) =>

        [.. lenderProfiles.Values.Select(profile => EvaluateForLender(profile, loan))];

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
            loanCalculator.CalculateMonthlyPayment(terms));
    }
}