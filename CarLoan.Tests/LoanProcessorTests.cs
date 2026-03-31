using Xunit;
using CarLoan.Domain;

namespace CarLoan.Tests
{
    public class LoanProcessorTests
    {
        [Fact]
        public void CalculatePrincipal_CurrentImplementation_ReturnsZero()
        {
            // The current implementation returns 0 regardless of inputs.
            Assert.Equal(0, LoanProcessor.CalculatePrincipal(20000, 5000));
        }
    }
}
