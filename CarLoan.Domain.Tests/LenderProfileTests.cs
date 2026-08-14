using CarLoan.Domain.Lenders;
using CarLoan.Domain.Providers;
using CarLoan.Domain.Validators;
using Xunit;

namespace CarLoan.Domain.Tests;

public class LenderProfileTests
{
    private static readonly LoanInterestRateProvider _defaultRateProvider = new([new RateTier(200000m, 12.20m)], 12.20m);

    [Fact]
    public void Constructor_ThrowsArgumentNullException_WhenNameIsNull()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new LenderProfile(null!, [new MinimumLoanPeriodValidator(6)], _defaultRateProvider));
    }

    [Fact]
    public void Constructor_ThrowsArgumentNullException_WhenRulesIsNull()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new LenderProfile("Lender", null!, _defaultRateProvider));
    }

    [Fact]
    public void Constructor_ThrowsArgumentNullException_WhenRateProviderIsNull()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new LenderProfile("Lender", [new MinimumLoanPeriodValidator(6)], null!));
    }

    [Fact]
    public void Constructor_ThrowsArgumentException_WhenRulesContainsNullEntry()
    {
        Assert.Throws<ArgumentException>(() =>
            new LenderProfile("Lender", [new MinimumLoanPeriodValidator(6), null!], _defaultRateProvider));
    }

    [Fact]
    public void Constructor_CreatesProfile_WhenValidValuesProvided()
    {
        var profile = new LenderProfile("Lender", [new MinimumLoanPeriodValidator(6)], _defaultRateProvider);

        Assert.Equal("Lender", profile.Name);
        Assert.Single(profile.Rules);
        Assert.Same(_defaultRateProvider, profile.RateProvider);
    }
}
