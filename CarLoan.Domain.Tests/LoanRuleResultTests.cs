using CarLoan.Domain.Models;
using Xunit;

namespace CarLoan.Domain.Tests;

public class LoanRuleResultTests
{
    [Fact]
    public void Create_SetsErrorMessageAndParamsToNull_WhenIsValidIsTrue()
    {
        var failureParams = new Dictionary<string, object> { ["min"] = 150000m };

        var result = LoanRuleResult.Create("TestRule", true, "failure message", failureParams);

        Assert.True(result.IsValid);
        Assert.Equal("TestRule", result.RuleName);
        Assert.Null(result.ErrorMessage);
        Assert.Null(result.Parameters);
    }

    [Fact]
    public void Create_SetsErrorMessageAndParams_WhenIsValidIsFalse()
    {
        var failureParams = new Dictionary<string, object> { ["min"] = 150000m };

        var result = LoanRuleResult.Create("TestRule", false, "failure message", failureParams);

        Assert.False(result.IsValid);
        Assert.Equal("TestRule", result.RuleName);
        Assert.Equal("failure message", result.ErrorMessage);
        Assert.Equal(failureParams, result.Parameters);
    }
}
