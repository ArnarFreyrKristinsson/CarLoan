using Xunit;

namespace CarLoan.Domain.Tests.LoanValidatorTests;

public class MaximumLoanPeriodTests
{
    private readonly LoanTerms _defaultLoanTerms = new(750000m, 2000000m, 1000000m, 11.10m, 84, 90m);

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
    public void IsMaximumLoanPeriodSatisfied_True_WhenLoanPeriodIsWithinMaximum(CarCondition carCondition, int loanPeriodInMonths, decimal loanRatio)
    {
        var loanTerms = _defaultLoanTerms with { LoanPeriodInMonths = loanPeriodInMonths, LoanRatio = loanRatio };
        var validator = new LoanValidator(loanTerms);

        var result = validator.IsMaximumLoanPeriodSatisfied(carCondition);

        Assert.True(result);
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
    public void IsMaximumLoanPeriodSatisfied_False_WhenLoanPeriodExceedsMaximum(CarCondition carCondition, int loanPeriodInMonths, decimal loanRatio)
    {
        var loanTerms = _defaultLoanTerms with { LoanPeriodInMonths = loanPeriodInMonths, LoanRatio = loanRatio };
        var validator = new LoanValidator(loanTerms);

        var result = validator.IsMaximumLoanPeriodSatisfied(carCondition);

        Assert.False(result);
    }
}