using CarLoan.Domain.Models;
using CarLoan.Domain.Validators;
using FluentAssertions;
using Xunit;

namespace CarLoan.Domain.Tests.LoanValidatorTests;

public class MaximumLoanPeriodTests
{
    private readonly LoanTerms _defaultLoanTerms = new(2000000m, 1000000m, 84, 10.35m);
    private static readonly LoanPeriodLimits _defaultLimits = new(80m, 84, 72);
    private readonly MaximumLoanPeriodValidator _validator = new(_defaultLimits);

    [Fact]
    public void Evaluate_ThrowsArgumentNullException_WhenLoanIsNull()
    {
        Assert.Throws<ArgumentNullException>(() => _validator.Evaluate(null!));
    }

    [Fact]
    public void Constructor_ThrowsArgumentNullException_WhenLimitsIsNull()
    {
        Assert.Throws<ArgumentNullException>(() => new MaximumLoanPeriodValidator(null!));
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
        var loan = new Loan(loanTerms, new Car(carCondition, VehicleCategory.PetrolOrDiesel, 0));

        var result = _validator.Evaluate(loan);

        Assert.True(result.IsValid);
        Assert.Equal(LoanRuleCode.MaximumLoanPeriod, result.Code);
        Assert.Null(result.ErrorMessage);
    }

    [Theory]
    [InlineData(CarCondition.New, 84)]
    [InlineData(CarCondition.New, 60)]
    [InlineData(CarCondition.Used, 72)]
    public void Evaluate_IsValid_WhenLoanRatioIsJustAboveNinetyPercentWithinTerm(CarCondition carCondition,
                                                                                 int loanPeriodInMonths)
    {
        // 1,800,000.01 of 2,000,000 breaks the 90% cap of T1/T3, but that cap is MaximumLoanRatio's
        // to report. This rule judges the term alone.
        var loanTerms = _defaultLoanTerms with { LoanPeriodInMonths = loanPeriodInMonths, DownPayment = 199_999.99m };
        var loan = new Loan(loanTerms, new Car(carCondition, VehicleCategory.PetrolOrDiesel, 0));

        var result = _validator.Evaluate(loan);

        Assert.True(result.IsValid);
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
        var loan = new Loan(loanTerms, new Car(carCondition, VehicleCategory.PetrolOrDiesel, 0));

        var result = _validator.Evaluate(loan);

        Assert.False(result.IsValid);
        Assert.Equal(LoanRuleCode.MaximumLoanPeriod, result.Code);
        Assert.NotNull(result.ErrorMessage);
    }

    [Fact]
    public void Evaluate_ReturnsGeneralLimitParameters_WhenGeneralLimitsExceeded()
    {
        var loanTerms = _defaultLoanTerms with { LoanPeriodInMonths = 85, DownPayment = 200000m };
        var loan = new Loan(loanTerms, new Car(CarCondition.New, VehicleCategory.PetrolOrDiesel, 0));

        var result = _validator.Evaluate(loan);

        Assert.NotNull(result.Parameters);
        result.Parameters.Should().BeEquivalentTo(new Dictionary<LoanRuleParameter, decimal>
        {
            [LoanRuleParameter.MaximumLoanPeriodMonths] = 84m
        });
    }

    [Fact]
    public void Evaluate_ReturnsUsedCarLimitParameters_WhenUsedCarLimitsExceeded()
    {
        var loanTerms = _defaultLoanTerms with { LoanPeriodInMonths = 73, DownPayment = 300000m };
        var loan = new Loan(loanTerms, new Car(CarCondition.Used, VehicleCategory.PetrolOrDiesel, 0));

        var result = _validator.Evaluate(loan);

        Assert.NotNull(result.Parameters);
        result.Parameters.Should().BeEquivalentTo(new Dictionary<LoanRuleParameter, decimal>
        {
            [LoanRuleParameter.LoanRatioThreshold] = 80m,
            [LoanRuleParameter.MaximumLoanPeriodMonths] = 72m
        });
    }

    [Fact]
    public void Evaluate_ReturnsGeneralLimitMessage_WhenGeneralLimitsExceeded()
    {
        var loanTerms = _defaultLoanTerms with { LoanPeriodInMonths = 85, DownPayment = 200000m };
        var loan = new Loan(loanTerms, new Car(CarCondition.New, VehicleCategory.PetrolOrDiesel, 0));

        var result = _validator.Evaluate(loan);

        Assert.False(string.IsNullOrWhiteSpace(result.ErrorMessage));
    }

    [Fact]
    public void Evaluate_ReturnsUsedCarLimitMessage_WhenUsedCarLimitsExceeded()
    {
        var loanTerms = _defaultLoanTerms with { LoanPeriodInMonths = 73, DownPayment = 300000m };
        var loan = new Loan(loanTerms, new Car(CarCondition.Used, VehicleCategory.PetrolOrDiesel, 0));

        var result = _validator.Evaluate(loan);

        Assert.False(string.IsNullOrWhiteSpace(result.ErrorMessage));
    }

    [Fact]
    public void Evaluate_ReturnsNullParameters_WhenLoanPeriodIsValid()
    {
        var loan = new Loan(_defaultLoanTerms, new Car(CarCondition.New, VehicleCategory.PetrolOrDiesel, 0));

        var result = _validator.Evaluate(loan);

        Assert.Null(result.Parameters);
    }

    [Fact]
    public void Evaluate_IsNotValid_WhenLoanPeriodExceedsConfiguredLimit()
    {
        var validator = new MaximumLoanPeriodValidator(new LoanPeriodLimits(80m, 60, 48));
        var loanTerms = _defaultLoanTerms with { LoanPeriodInMonths = 72, DownPayment = 1000000m };
        var loan = new Loan(loanTerms, new Car(CarCondition.New, VehicleCategory.PetrolOrDiesel, 0));

        var result = validator.Evaluate(loan);

        Assert.False(result.IsValid);
        Assert.NotNull(result.Parameters);
        result.Parameters.Should().BeEquivalentTo(new Dictionary<LoanRuleParameter, decimal>
        {
            [LoanRuleParameter.MaximumLoanPeriodMonths] = 60m
        });
    }
}