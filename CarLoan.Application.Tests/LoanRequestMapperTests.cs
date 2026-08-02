using CarLoan.Application.Mapping;
using CarLoan.Application.Requests;
using CarLoan.Domain.Models;
using FluentAssertions;

namespace CarLoan.Application.Tests;

public class LoanRequestMapperTests
{
    [Fact]
    public void ToLoan_MapsRequestToLoan_WhenLoanRequestProvided()
    {
        var request = new LoanRequest(2000000m, 500000m, 60, CarCondition.Used);

        var loan = request.ToLoan();

        loan.Should().BeEquivalentTo(
            new Loan(new LoanTerms(2000000m, 500000m, 60, 0m), new Car(CarCondition.Used)));
    }
}
