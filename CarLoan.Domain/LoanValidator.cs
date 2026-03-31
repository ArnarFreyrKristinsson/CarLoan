namespace CarLoan.Domain;

public class LoanValidator(LoanTerms loanTerms)
{
    private readonly LoanTerms _loanTerms = loanTerms;

    private const decimal MinimumLoanAmount = 750000m;
    private const decimal MinimumDownPayment = 150000m;
    private const int MinimumLoanPeriodMonths = 6;
    private const decimal MaximumLoanRatio = 90m;
    private const decimal UsedCarLoanRatioThreshold = 80m;
    private const int MaximumLoanPeriodMonths = 84;
    private const int UsedCarMaximumLoanPeriodMonths = 72;

    private static bool ExceedsGeneralLimits(decimal ratio, int period)
    {
        return ratio > MaximumLoanRatio || period > MaximumLoanPeriodMonths;
    }

    private static bool ExceedsUsedCarRatio(decimal ratio, CarCondition carCondition)
    {
        return carCondition == CarCondition.Used && ratio > UsedCarLoanRatioThreshold;
    }

    public bool IsMinimumLoanAmountSatisfied()
    {
        decimal loanAmount = _loanTerms.PurchasePrice - _loanTerms.DownPayment;
        return loanAmount >= MinimumLoanAmount;
    }

    public bool IsMinimumDownPaymentSatisfied()
    {
        return _loanTerms.DownPayment >= MinimumDownPayment;
    }

    public bool IsMinimumLoanPeriodSatisfied()
    {
        return _loanTerms.LoanPeriodInMonths >= MinimumLoanPeriodMonths;
    }

    public bool IsMaximumLoanPeriodSatisfied(CarCondition carCondition)
    {
        int period = _loanTerms.LoanPeriodInMonths;
        decimal ratio = _loanTerms.LoanRatio;

        if (ExceedsGeneralLimits(ratio, period))
            return false;

        if (ExceedsUsedCarRatio(ratio, carCondition))
            return period <= UsedCarMaximumLoanPeriodMonths;
        return true;
    }
}