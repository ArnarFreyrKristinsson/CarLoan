using CarLoan.Domain.Providers;
using CarLoan.Domain.Validators;

namespace CarLoan.Domain.Lenders;

public sealed record LenderProfile(
    string Name,
    IReadOnlyList<ILoanRule> Rules,
    ILoanInterestRateProvider RateProvider)
{
    public string Name { get; } = Name ?? throw new ArgumentNullException(nameof(Name));
    public IReadOnlyList<ILoanRule> Rules { get; } = Rules ?? throw new ArgumentNullException(nameof(Rules));
    public ILoanInterestRateProvider RateProvider { get; } = RateProvider ?? throw new ArgumentNullException(nameof(RateProvider));
}
