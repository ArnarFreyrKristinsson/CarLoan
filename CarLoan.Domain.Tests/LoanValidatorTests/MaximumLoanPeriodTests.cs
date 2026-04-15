using Xunit;

namespace CarLoan.Domain.Tests.LoanValidatorTests;

public class MaximumLoanPeriodTests
{
    private readonly LoanTerms _defaultLoanTerms = new(750000m, 2000000m, 1000000m, 11.10m, 84, 90m);
    private readonly MaximumLoanPeriodValidator _validator = new();

    [Theory]
    [InlineData(CarCondition.New, 84, 90)]
    [InlineData(CarCondition.New, 72, 90)]
    [InlineData(CarCondition.New, 84, 80)]
    [InlineData(CarCondition.Used, 72, 90)]
    [InlineData(CarCondition.Used, 60, 90)]
    [InlineData(CarCondition.Used, 84, 80)]
    [InlineData(CarCondition.Used, 72, 80)]
    [InlineData(CarCondition.Used, 73, 80)]
    [InlineData(CarCondition.New, 84, 85)]
    [InlineData(CarCondition.New, 84, 50)]
    [InlineData(CarCondition.Used, 84, 50)]
    public void Evaluate_IsValid_WhenLoanPeriodIsWithinMaximum(CarCondition carCondition, int loanPeriodInMonths, decimal loanRatio)
    {
        var loanTerms = _defaultLoanTerms with { LoanPeriodInMonths = loanPeriodInMonths, LoanRatio = loanRatio };
        var loan = new Loan(loanTerms, new Car(carCondition));

        var result = _validator.Evaluate(loan);

        Assert.True(result.IsValid);
        Assert.Equal("MaximumLoanPeriod", result.RuleName);
        Assert.Null(result.ErrorMessage);
    }

    [Theory]
    [InlineData(CarCondition.Used, 84, 90)]
    [InlineData(CarCondition.Used, 85, 90)]
    [InlineData(CarCondition.Used, 85, 70)]
    [InlineData(CarCondition.New, 85, 90)]
    [InlineData(CarCondition.Used, 73, 90)]
    [InlineData(CarCondition.New, 85, 80)]
    [InlineData(CarCondition.Used, 85, 80)]
    [InlineData(CarCondition.Used, 73, 85)]
    public void Evaluate_IsNotValid_WhenLoanPeriodExceedsMaximum(CarCondition carCondition, int loanPeriodInMonths, decimal loanRatio)
    {
        var loanTerms = _defaultLoanTerms with { LoanPeriodInMonths = loanPeriodInMonths, LoanRatio = loanRatio };
        var loan = new Loan(loanTerms, new Car(carCondition));

        var result = _validator.Evaluate(loan);

        Assert.False(result.IsValid);
        Assert.Equal("MaximumLoanPeriod", result.RuleName);
        Assert.NotNull(result.ErrorMessage);
    }
}