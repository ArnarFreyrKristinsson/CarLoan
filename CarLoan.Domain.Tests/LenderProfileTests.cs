using CarLoan.Domain.Fees;
using CarLoan.Domain.Lenders;
using CarLoan.Domain.Providers;
using CarLoan.Domain.Validators;
using Xunit;

namespace CarLoan.Domain.Tests;

public class LenderProfileTests
{
    private static readonly LoanInterestRateProvider _defaultRateProvider =
        new(new RateTable([new RateTier(90m, 12.20m)], 12.20m),
            new RateTable([new RateTier(90m, 11.50m)], 11.50m));

    private static readonly OriginationFeeCalculator _defaultFeeCalculator =
        new(new OriginationFeeSettings([new FeeTier(84, 3.20m)], 18_000m, 50m, 1.00m));

    [Fact]
    public void Constructor_ThrowsArgumentNullException_WhenNameIsNull()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new LenderProfile(null!, [new MinimumLoanPeriodValidator(6)], _defaultRateProvider, _defaultFeeCalculator));
    }

    [Fact]
    public void Constructor_ThrowsArgumentNullException_WhenRulesIsNull()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new LenderProfile("Lender", null!, _defaultRateProvider, _defaultFeeCalculator));
    }

    [Fact]
    public void Constructor_ThrowsArgumentNullException_WhenRateProviderIsNull()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new LenderProfile("Lender", [new MinimumLoanPeriodValidator(6)], null!, _defaultFeeCalculator));
    }

    [Fact]
    public void Constructor_ThrowsArgumentNullException_WhenFeeCalculatorIsNull()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new LenderProfile("Lender", [new MinimumLoanPeriodValidator(6)], _defaultRateProvider, null!));
    }

    [Fact]
    public void Constructor_ThrowsArgumentException_WhenRulesContainsNullEntry()
    {
        Assert.Throws<ArgumentException>(() =>
            new LenderProfile("Lender", [new MinimumLoanPeriodValidator(6), null!], _defaultRateProvider, _defaultFeeCalculator));
    }

    [Fact]
    public void Constructor_CreatesProfile_WhenValidValuesProvided()
    {
        var profile = new LenderProfile("Lender", [new MinimumLoanPeriodValidator(6)], _defaultRateProvider, _defaultFeeCalculator);

        Assert.Equal("Lender", profile.Name);
        Assert.Single(profile.Rules);
        Assert.Same(_defaultRateProvider, profile.RateProvider);
        Assert.Same(_defaultFeeCalculator, profile.FeeCalculator);
    }
}
