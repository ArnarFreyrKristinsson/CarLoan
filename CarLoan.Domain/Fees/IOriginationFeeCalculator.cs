using CarLoan.Domain.Models;

namespace CarLoan.Domain.Fees;

public interface IOriginationFeeCalculator
{
    OriginationFee Calculate(Loan loan);
}
