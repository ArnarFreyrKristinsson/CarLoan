using CarLoan.Domain.Models;
using CarLoan.Domain.Validators;
using FluentAssertions;
using Xunit;

namespace CarLoan.Domain.Tests.LoanValidatorTests;

public class LoanValidatorIntegrationTests
{
    private static readonly ILoanRule[] _allRules =
    [
        new MinimumLoanAmountValidator(750000m),
        new MaximumLoanAmountValidator(30_000_000m),
        new MinimumLoanPeriodValidator(6),
        new MinimumDownPaymentValidator(150000m),
        new MaximumLoanRatioValidator(90m),
        new MaximumLoanPeriodValidator(new LoanPeriodLimits(80m, 84, 72)),
        new CarAgeValidator(new CarAgeLimits(80m, 12, 20))
    ];

    private readonly LoanValidator _validator = new(_allRules);

    private static Loan CreateLoan(
        decimal purchasePrice = 2_000_000m,
        decimal downPayment = 200_000m,
        int loanPeriodInMonths = 36,
        CarCondition condition = CarCondition.New)
    {
        var terms = new LoanTerms(purchasePrice, downPayment, loanPeriodInMonths, 12.20m);
        return new Loan(terms, new Car(condition, VehicleCategory.PetrolOrDiesel, 0));
    }

    [Fact]
    public void Constructor_ThrowsArgumentNullException_WhenRulesIsNull()
    {
        Assert.Throws<ArgumentNullException>(() => new LoanValidator(null!));
    }

    [Fact]
    public void Constructor_ThrowsArgumentException_WhenRulesContainsNullEntry()
    {
        Assert.Throws<ArgumentException>(() => new LoanValidator([new MinimumLoanPeriodValidator(6), null!]));
    }

    [Fact]
    public void Validate_AllResultsAreValid_WhenAllRulesPass()
    {
        var loan = CreateLoan();
        var expectedResults = new[]
        {
            LoanRuleResult.Create(LoanRuleCode.MinimumLoanAmount, true),
            LoanRuleResult.Create(LoanRuleCode.MaximumLoanAmount, true),
            LoanRuleResult.Create(LoanRuleCode.MinimumLoanPeriod, true),
            LoanRuleResult.Create(LoanRuleCode.MinimumDownPayment, true),
            LoanRuleResult.Create(LoanRuleCode.MaximumLoanRatio, true),
            LoanRuleResult.Create(LoanRuleCode.MaximumLoanPeriod, true),
            LoanRuleResult.Create(LoanRuleCode.CarAge, true),
        };

        var results = _validator.Validate(loan);

        results.Should().BeEquivalentTo(expectedResults);
    }

    [Fact]
    public void Validate_ContainsOnlyMinimumLoanAmountFailure_WhenLoanAmountTooLow()
    {
        var loan = CreateLoan(purchasePrice: 800_000m);

        var results = _validator.Validate(loan);

        var failure = Assert.Single(results, r => !r.IsValid);
        Assert.Equal(LoanRuleCode.MinimumLoanAmount, failure.Code);
    }

    [Fact]
    public void Validate_ContainsOnlyMinimumDownPaymentFailure_WhenDownPaymentTooLow()
    {
        var loan = CreateLoan(purchasePrice: 900_000m, downPayment: 100_000m);

        var results = _validator.Validate(loan);

        var failure = Assert.Single(results, r => !r.IsValid);
        Assert.Equal(LoanRuleCode.MinimumDownPayment, failure.Code);
    }

    [Fact]
    public void Validate_ContainsOnlyMinimumLoanPeriodFailure_WhenLoanPeriodTooShort()
    {
        var loan = CreateLoan(loanPeriodInMonths: 3);

        var results = _validator.Validate(loan);

        var failure = Assert.Single(results, r => !r.IsValid);
        Assert.Equal(LoanRuleCode.MinimumLoanPeriod, failure.Code);
    }

    [Fact]
    public void Validate_ContainsOnlyMaximumLoanPeriodFailure_WhenLoanPeriodTooLong()
    {
        var loan = CreateLoan(loanPeriodInMonths: 85);

        var results = _validator.Validate(loan);

        var failure = Assert.Single(results, r => !r.IsValid);
        Assert.Equal(LoanRuleCode.MaximumLoanPeriod, failure.Code);
    }

    [Fact]
    public void Validate_ContainsOnlyMaximumLoanRatioFailure_WhenNewCarLoanRatioTooHighWithinTerm()
    {
        var loan = CreateLoan(downPayment: 150_000m, loanPeriodInMonths: 60);

        var results = _validator.Validate(loan);

        var failure = Assert.Single(results, r => !r.IsValid);
        Assert.Equal(LoanRuleCode.MaximumLoanRatio, failure.Code);
    }

    [Fact]
    public void Validate_ContainsMultipleFailures_WhenMultipleRulesViolated()
    {
        var loan = CreateLoan(purchasePrice: 800_000m, downPayment: 100_000m, loanPeriodInMonths: 3);

        var results = _validator.Validate(loan);

        var failedRules = results.Where(r => !r.IsValid).Select(r => r.Code);
        failedRules.Should().BeEquivalentTo([LoanRuleCode.MinimumLoanAmount, LoanRuleCode.MinimumDownPayment, LoanRuleCode.MinimumLoanPeriod]);
    }
}
