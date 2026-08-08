using CarLoan.Application.Mapping;
using CarLoan.Application.Requests;
using CarLoan.Domain.Models;
using FluentAssertions;

namespace CarLoan.Application.Tests;

public class LoanRequestMapperTests
{
    private const RequestedCarCondition UnsupportedCarCondition = (RequestedCarCondition)999;

    [Fact]
    public void ToLoan_ThrowsArgumentNullException_WhenRequestIsNull()
    {
        Assert.Throws<ArgumentNullException>(() => LoanRequestMapper.ToLoan(null!));
    }

    [Fact]
    public void ToLoan_ThrowsArgumentOutOfRangeExceptionWithMessage_WhenCarConditionIsUnsupported()
    {
        var request = new LoanRequest(2000000m, 500000m, 60, UnsupportedCarCondition);

        Assert.Throws<ArgumentOutOfRangeException>(() => LoanRequestMapper.ToLoan(request));
    }

    [Fact]
    public void ToLoan_MapsRequestToLoan_WhenLoanRequestProvided()
    {
        var request = new LoanRequest(2000000m, 500000m, 60, RequestedCarCondition.Used);

        var loan = LoanRequestMapper.ToLoan(request);

        loan.Should().BeEquivalentTo(
            new Loan(new LoanTerms(2000000m, 500000m, 60, 0m), new Car(CarCondition.Used)));
    }

    [Fact]
    public void ToLoan_MapsNewCarCondition_WhenNewConditionRequested()
    {
        var request = new LoanRequest(2000000m, 500000m, 60, RequestedCarCondition.New);

        var loan = LoanRequestMapper.ToLoan(request);

        loan.Car.Should().BeEquivalentTo(new Car(CarCondition.New));
    }
}
