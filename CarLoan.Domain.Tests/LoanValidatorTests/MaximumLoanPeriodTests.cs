using CarLoan.Domain.Models;
using CarLoan.Domain.Validators;
using Xunit;

namespace CarLoan.Domain.Tests.LoanValidatorTests;

public class MaximumLoanPeriodTests
{
    private readonly LoanTerms _defaultLoanTerms = new(750000m, 2000000m, 1000000m, 84, 90m);
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
    public void Evaluate_IsValid_WhenLoanPeriodIsWithinMaximum(CarCondition carCondition, 
                                                                int loanPeriodInMonths, decimal loanRatio)
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
    public void Evaluate_IsNotValid_WhenLoanPeriodExceedsMaximum(CarCondition carCondition, 
                                                                 int loanPeriodInMonths, 
                                                                 decimal loanRatio)
    {
        var loanTerms = _defaultLoanTerms with { LoanPeriodInMonths = loanPeriodInMonths, LoanRatio = loanRatio };
        var loan = new Loan(loanTerms, new Car(carCondition));

        var result = _validator.Evaluate(loan);

        Assert.False(result.IsValid);
        Assert.Equal("MaximumLoanPeriod", result.RuleName);
        Assert.NotNull(result.ErrorMessage);
    }

    [Fact]
    public void Evaluate_ReturnsGeneralLimitParameters_WhenGeneralLimitsExceeded()
    {
        var loanTerms = _defaultLoanTerms with { LoanPeriodInMonths = 85, LoanRatio = 90m };
        var loan = new Loan(loanTerms, new Car(CarCondition.New));

        var result = _validator.Evaluate(loan);

        Assert.NotNull(result.Parameters);
        Assert.Equal(90m, result.Parameters["maxRatio"]);
        Assert.Equal(84, result.Parameters["maxMonths"]);
    }

    [Fact]
    public void Evaluate_ReturnsUsedCarLimitParameters_WhenUsedCarLimitsExceeded()
    {
        var loanTerms = _defaultLoanTerms with { LoanPeriodInMonths = 73, LoanRatio = 85m };
        var loan = new Loan(loanTerms, new Car(CarCondition.Used));

        var result = _validator.Evaluate(loan);

        Assert.NotNull(result.Parameters);
        Assert.Equal(80m, result.Parameters["ratioThreshold"]);
        Assert.Equal(72, result.Parameters["maxMonths"]);
    }

    [Fact]
    public void Evaluate_ReturnsNullParameters_WhenLoanPeriodIsValid()
    {
        var loan = new Loan(_defaultLoanTerms, new Car(CarCondition.New));

        var result = _validator.Evaluate(loan);

        Assert.Null(result.Parameters);
    }
}