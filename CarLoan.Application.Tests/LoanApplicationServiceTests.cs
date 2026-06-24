using CarLoan.Domain.Calculators;
using Moq;
using Xunit;

namespace CarLoan.Application.Tests;

public class LoanApplicationServiceTests
{
    private readonly Mock<ILoanCalculator> _loanCalculatorMock;
    private readonly LoanApplicationService _service;

    public LoanApplicationServiceTests()
    {
        _loanCalculatorMock = new Mock<ILoanCalculator>();
        _service = new LoanApplicationService(_loanCalculatorMock.Object);
    }

    [Fact]
    public void GetMonthlyPayment_ReturnsDelegatedResult_WhenLoanCalculatorProvided()
    {
        _loanCalculatorMock.Setup(c => c.CalculateMonthlyPayment()).Returns(258415.03m);

        decimal result = _service.GetMonthlyPayment();

        Assert.Equal(258415.03m, result);
    }
}
