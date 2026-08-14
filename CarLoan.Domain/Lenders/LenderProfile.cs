using CarLoan.Domain.Guards;
using CarLoan.Domain.Providers;
using CarLoan.Domain.Validators;

namespace CarLoan.Domain.Lenders;

public sealed record LenderProfile(
    string Name,
    IReadOnlyList<ILoanRule> Rules,
    ILoanInterestRateProvider RateProvider)
{
    public string Name { get; } = Guard.NotNull(Name, nameof(Name));
    public IReadOnlyList<ILoanRule> Rules { get; } = Guard.NoNullElements(Rules, nameof(Rules));
    public ILoanInterestRateProvider RateProvider { get; } = Guard.NotNull(RateProvider, nameof(RateProvider));
}
