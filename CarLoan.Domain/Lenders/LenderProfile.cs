using CarLoan.Domain.Providers;
using CarLoan.Domain.Validators;

namespace CarLoan.Domain.Lenders;

public sealed record LenderProfile(
    string Name,
    IReadOnlyList<ILoanRule> Rules,
    ILoanInterestRateProvider RateProvider);
