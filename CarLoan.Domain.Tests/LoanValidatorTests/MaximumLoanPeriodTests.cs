using CarLoan.Domain.Models;
using CarLoan.Domain.Validators;
using Xunit;

namespace CarLoan.Domain.Tests.LoanValidatorTests;

public class MaximumLoanPeriodTests
{
    private readonly LoanTerms _defaultLoanTerms = new(2000000m, 1000000m, 84, 10.35m);
    private readonly MaximumLoanPeriodValidator _validator = new();

    [Fact]
    public void Evaluate_ThrowsArgumentNullException_WhenLoanIsNull()
    {
        Assert.Throws<ArgumentNullException>(() => _validator.Evaluate(null!));
    }

    [Theory]
    [InlineData(CarCondition.New, 84, 200000)]
    [InlineData(CarCondition.New, 72, 200000)]
    [InlineData(CarCondition.New, 84, 400000)]
    [InlineData(CarCondition.Used, 72, 200000)]
    [InlineData(CarCondition.Used, 60, 200000)]
    [InlineData(CarCondition.Used, 84, 400000)]
    [InlineData(CarCondition.Used, 72, 400000)]
    [InlineData(CarCondition.Used, 73, 400000)]
    [InlineData(CarCondition.New, 84, 300000)]
    [InlineData(CarCondition.New, 84, 1000000)]
    [InlineData(CarCondition.Used, 84, 1000000)]
    public void Evaluate_IsValid_WhenLoanPeriodIsWithinMaximum(CarCondition carCondition,
                                                                int loanPeriodInMonths, decimal downPayment)
    {
        var loanTerms = _defaultLoanTerms with { LoanPeriodInMonths = loanPeriodInMonths, DownPayment = downPayment };
        var loan = new Loan(loanTerms, new Car(carCondition));

        var result = _validator.Evaluate(loan);

        Assert.True(result.IsValid);
        Assert.Equal("MaximumLoanPeriod", result.RuleName);
        Assert.Null(result.ErrorMessage);
    }

    [Theory]
    [InlineData(CarCondition.Used, 84, 200000)]
    [InlineData(CarCondition.Used, 85, 200000)]
    [InlineData(CarCondition.Used, 85, 600000)]
    [InlineData(CarCondition.New, 85, 200000)]
    [InlineData(CarCondition.Used, 73, 200000)]
    [InlineData(CarCondition.New, 85, 400000)]
    [InlineData(CarCondition.Used, 85, 400000)]
    [InlineData(CarCondition.Used, 73, 300000)]
    public void Evaluate_IsNotValid_WhenLoanPeriodExceedsMaximum(CarCondition carCondition,
                                                                 int loanPeriodInMonths,
                                                                 decimal downPayment)
    {
        var loanTerms = _defaultLoanTerms with { LoanPeriodInMonths = loanPeriodInMonths, DownPayment = downPayment };
        var loan = new Loan(loanTerms, new Car(carCondition));

        var result = _validator.Evaluate(loan);

        Assert.False(result.IsValid);
        Assert.Equal("MaximumLoanPeriod", result.RuleName);
        Assert.NotNull(result.ErrorMessage);
    }

    [Fact]
    public void Evaluate_ReturnsGeneralLimitParameters_WhenGeneralLimitsExceeded()
    {
        var loanTerms = _defaultLoanTerms with { LoanPeriodInMonths = 85, DownPayment = 200000m };
        var loan = new Loan(loanTerms, new Car(CarCondition.New));

        var result = _validator.Evaluate(loan);

        Assert.NotNull(result.Parameters);
        Assert.Equal(90m, result.Parameters["maxRatio"]);
        Assert.Equal(84, result.Parameters["maxMonths"]);
    }

    [Fact]
    public void Evaluate_ReturnsUsedCarLimitParameters_WhenUsedCarLimitsExceeded()
    {
        var loanTerms = _defaultLoanTerms with { LoanPeriodInMonths = 73, DownPayment = 300000m };
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