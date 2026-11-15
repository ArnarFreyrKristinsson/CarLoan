using CarLoan.Application;
using CarLoan.Application.Requests;
using static CarLoan.Web.Presentation.IcelandicFormat;

namespace CarLoan.Web.Presentation;

public enum PaymentState
{
    /// <summary>Every rule passes and the payment exists.</summary>
    Available,

    /// <summary>A rule fails, but the lender's schedule still prices the loan (US-5 AC7).</summary>
    NotOnTheseTerms,

    /// <summary>No payment exists for these inputs (US-5 AC8).</summary>
    CannotBeCalculated,

    /// <summary>A field is empty, so there is nothing to calculate yet.</summary>
    AwaitingInput
}

public sealed record FigureRow(string Label, string Value, string? Note = null);

/// <summary>The input a message is about, so a narrow screen can show it beside that input.</summary>
public enum InputField
{
    None,
    DownPayment,
    Term
}

/// <summary>A message, and the input whose change fixes it.</summary>
public sealed record FieldMessage(string Text, InputField Field);

/// <summary>
/// What the results show, read straight off the engine's answer: which of the three states the
/// payment is in, the figures as text ("—" where the engine has no number), and the messages.
/// No figure is worked out here.
/// </summary>
public sealed record ResultView(
    PaymentState State,
    string? MonthlyPayment,
    string? TotalInterest,
    IReadOnlyList<FieldMessage> Reasons,
    IReadOnlyList<FieldMessage> RuleMessages,
    IReadOnlyList<FigureRow> Figures,
    LoanFigures? Loan,
    LenderLoanEvaluationResult? Lender)
{
    /// <summary>The lender this calculator is for; the engine may evaluate others.</summary>
    public const string LenderName = "Lykill";

    private const string NoValue = "—";

    internal static ResultView From(LoanEvaluationOutcome? outcome, string? missingInputPrompt, bool showsAmountSaved)
    {
        if (missingInputPrompt is not null)
            return new(PaymentState.AwaitingInput, null, null, [new(missingInputPrompt, InputField.None)], [], Rows(null, null, showsAmountSaved), null, null);

        switch (outcome)
        {
            case LoanEvaluationOutcome.InvalidRequest invalid:
                return CannotCalculate(
                    [.. invalid.Errors
                        .Select(error => new FieldMessage(LoanText.InputErrorMessage(error), LoanText.FieldFor(error)))
                        .Distinct()],
                    showsAmountSaved);

            case LoanEvaluationOutcome.Evaluated evaluated:
                var lender = evaluated.Lenders.Single(result => result.LenderName == LenderName);
                var ruleMessages = lender.ValidationResults
                    .Where(rule => !rule.IsValid)
                    .Select(rule => new FieldMessage(LoanText.RuleMessage(rule, lender.MaximumLoanPeriodInMonths), LoanText.FieldFor(rule.Code)))
                    .ToList();
                var figures = Rows(evaluated.Loan, lender, showsAmountSaved);

                if (lender.MonthlyPayment is not decimal payment || lender.TotalInterest is not decimal totalInterest)
                    return new(PaymentState.CannotBeCalculated, null, null,
                        [.. lender.PricingGaps.Select(gap => new FieldMessage(LoanText.PricingGapReason(gap), LoanText.FieldFor(gap.Code)))],
                        ruleMessages, figures, evaluated.Loan, lender);

                return new(lender.IsAccepted ? PaymentState.Available : PaymentState.NotOnTheseTerms,
                    Krona(payment), Krona(totalInterest), [], ruleMessages, figures, evaluated.Loan, lender);

            default:
                return CannotCalculate([], showsAmountSaved);
        }
    }

    /// <summary>The rule messages and payment reasons that concern one input.</summary>
    public IReadOnlyList<string> MessagesFor(InputField field) =>
        [.. RuleMessages.Concat(Reasons).Where(message => message.Field == field).Select(message => message.Text)];

    private static ResultView CannotCalculate(IReadOnlyList<FieldMessage> reasons, bool showsAmountSaved) =>
        new(PaymentState.CannotBeCalculated, null, null, reasons, [], Rows(null, null, showsAmountSaved), null, null);

    private static List<FigureRow> Rows(LoanFigures? loan, LenderLoanEvaluationResult? lender, bool showsAmountSaved)
    {
        var fee = lender?.OriginationFee;

        List<FigureRow> rows =
        [
            new("Lánsfjárhæð", Text(loan?.LoanAmount, Krona)),
            new(Labels.FinancingRatio, Text(loan?.FinancingRatioRoundedUp, Ratio)),
            new("Vextir", Text(lender?.InterestRate, Rate)),
            new("Hlutfall lántökugjalds", Text(fee?.EffectiveRate, Rate)),
            new("Lántökugjald", Text(fee?.Amount, Krona), fee?.MinimumFeeApplied == true ? "Lágmarksgjald" : null)
        ];

        // US-2 AC2–3: only plug-in hybrids and green cars have a discount to save on.
        if (showsAmountSaved)
            rows.Add(new("Sparnaður á lántökugjaldi", Text(fee?.AmountSaved, Krona)));

        rows.Add(new("Lánsfjárhæð með lántökugjaldi", Text(lender?.FinancedAmount, Krona)));

        return rows;
    }

    private static string Text(decimal? value, Func<decimal, string> format) =>
        value is decimal number ? format(number) : NoValue;
}
