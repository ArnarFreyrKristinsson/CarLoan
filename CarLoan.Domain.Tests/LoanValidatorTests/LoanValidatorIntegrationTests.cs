using CarLoan.Domain.Models;
using CarLoan.Domain.Validators;
using Xunit;

namespace CarLoan.Domain.Tests.LoanValidatorTests;

public class LoanValidatorIntegrationTests
{
    private static readonly ILoanRule[] _allRules =
    [
        new MinimumLoanAmountValidator(),
        new MinimumLoanPeriodValidator(),
        new MinimumDownPaymentValidator(),
        new MaximumLoanPeriodValidator()
    ];

    private readonly LoanValidator _validator = new(_allRules);

    private static Loan CreateLoan(
        decimal purchasePrice = 2_000_000m,
        decimal downPayment = 200_000m,
        int loanPeriodInMonths = 36,
        CarCondition condition = CarCondition.New)
    {
        var terms = new LoanTerms(purchasePrice, downPayment, loanPeriodInMonths, 12.20m);
        return new Loan(terms, new Car(condition));
    }

    [Fact]
    public void Validate_AllResultsAreValid_WhenAllRulesPass()
    {
        var loan = CreateLoan();
        var expectedResults = new[]
        {
            LoanRuleResult.Create("MinimumLoanAmount", true),
            LoanRuleResult.Create("MinimumLoanPeriod", true),
            LoanRuleResult.Create("MinimumDownPayment", true),
            LoanRuleResult.Create("MaximumLoanPeriod", true),
        };

        var results = _validator.Validate(loan);

        Assert.Equivalent(expectedResults, results);
    }

    [Fact]
    public void Validate_ReturnsResultForEveryRule_WhenLoanTermsProvided()
    {
        var loan = CreateLoan();

        var results = _validator.Validate(loan);

        Assert.Equal(_allRules.Length, results.Count);
    }

    [Fact]
    public void Validate_ContainsOnlyMinimumLoanAmountFailure_WhenLoanAmountTooLow()
    {
        var loan = CreateLoan(purchasePrice: 800_000m);

        var results = _validator.Validate(loan);

        var failure = Assert.Single(results, r => !r.IsValid);
        Assert.Equal("MinimumLoanAmount", failure.RuleName);
    }

    [Fact]
    public void Validate_ContainsOnlyMinimumDownPaymentFailure_WhenDownPaymentTooLow()
    {
        var loan = CreateLoan(purchasePrice: 900_000m, downPayment: 100_000m);

        var results = _validator.Validate(loan);

        var failure = Assert.Single(results, r => !r.IsValid);
        Assert.Equal("MinimumDownPayment", failure.RuleName);
    }

    [Fact]
    public void Validate_ContainsOnlyMinimumLoanPeriodFailure_WhenLoanPeriodTooShort()
    {
        var loan = CreateLoan(loanPeriodInMonths: 3);

        var results = _validator.Validate(loan);

        var failure = Assert.Single(results, r => !r.IsValid);
        Assert.Equal("MinimumLoanPeriod", failure.RuleName);
    }

    [Fact]
    public void Validate_ContainsOnlyMaximumLoanPeriodFailure_WhenLoanPeriodTooLong()
    {
        var loan = CreateLoan(loanPeriodInMonths: 85);

        var results = _validator.Validate(loan);

        var failure = Assert.Single(results, r => !r.IsValid);
        Assert.Equal("MaximumLoanPeriod", failure.RuleName);
    }

    [Fact]
    public void Validate_ContainsMultipleFailures_WhenMultipleRulesViolated()
    {
        var loan = CreateLoan(purchasePrice: 800_000m, downPayment: 100_000m, loanPeriodInMonths: 3);

        var results = _validator.Validate(loan);

        var failedRules = results.Where(r => !r.IsValid).Select(r => r.RuleName).ToList();
        Assert.Contains("MinimumLoanAmount", failedRules);
        Assert.Contains("MinimumDownPayment", failedRules);
        Assert.Contains("MinimumLoanPeriod", failedRules);
        Assert.DoesNotContain("MaximumLoanPeriod", failedRules);
    }
}
