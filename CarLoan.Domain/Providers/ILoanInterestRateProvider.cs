namespace CarLoan.Domain.Providers;

public interface ILoanInterestRateProvider
{
    decimal GetInterestRate(decimal downPayment);
}
