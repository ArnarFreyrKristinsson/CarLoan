using CarLoan.Application.Requests;
using CarLoan.Domain.Models;
using static CarLoan.Web.Presentation.IcelandicFormat;

namespace CarLoan.Web.Presentation;

/// <summary>
/// The words the screen uses for what the engine reports: one message per rule code, worded
/// from its parameters (docs/api-contract.md), never from the engine's own diagnostic text.
/// Each message says what is wrong, the limit, and what to change (US-5 AC3), with no codes.
/// </summary>
internal static class LoanText
{

    public static string RuleMessage(LoanRuleResult rule, int? maximumLoanPeriodInMonths)
    {
        var parameters = rule.Parameters ?? new Dictionary<LoanRuleParameter, decimal>();
        decimal Parameter(LoanRuleParameter key) => parameters.GetValueOrDefault(key);

        return rule.Code switch
        {
            LoanRuleCode.MinimumLoanAmount =>
                $"Lánsfjárhæðin þarf að vera að minnsta kosti {Krona(Parameter(LoanRuleParameter.Minimum))} Lækkaðu útborgunina eða hækkaðu kaupverðið.",
            LoanRuleCode.MaximumLoanAmount =>
                $"Lánsfjárhæðin má ekki vera hærri en {Krona(Parameter(LoanRuleParameter.Maximum))} Hækkaðu útborgunina eða lækkaðu kaupverðið.",
            LoanRuleCode.MinimumDownPayment =>
                $"Útborgunin þarf að vera að minnsta kosti {Krona(Parameter(LoanRuleParameter.Minimum))} Hækkaðu útborgunina.",
            LoanRuleCode.MinimumLoanPeriod =>
                $"Lánstíminn þarf að vera að minnsta kosti {Months(Parameter(LoanRuleParameter.Minimum))}. Lengdu lánstímann.",
            LoanRuleCode.MaximumLoanRatio =>
                $"Fjármögnunarhlutfallið má ekki vera hærra en {Limit(Parameter(LoanRuleParameter.Maximum))}. Hækkaðu útborgunina eða lækkaðu kaupverðið.",
            LoanRuleCode.MaximumLoanPeriod when parameters.ContainsKey(LoanRuleParameter.LoanRatioThreshold) =>
                $"Á notuðum bíl með fjármögnunarhlutfall yfir {Limit(Parameter(LoanRuleParameter.LoanRatioThreshold))} er lengsti lánstími {Months(Parameter(LoanRuleParameter.MaximumLoanPeriodMonths))}. Styttu lánstímann eða hækkaðu útborgunina.",
            LoanRuleCode.MaximumLoanPeriod =>
                $"Lengsti lánstími er {Months(Parameter(LoanRuleParameter.MaximumLoanPeriodMonths))}. Styttu lánstímann.",
            LoanRuleCode.CarAge =>
                CarAgeMessage(Parameter(LoanRuleParameter.MaximumCombinedYears), maximumLoanPeriodInMonths),
            _ => "Lánið stenst ekki eina af lánareglum Lykils."
        };
    }

    /// <summary>The input whose change fixes a failed rule.</summary>
    public static InputField FieldFor(LoanRuleCode code) => code switch
    {
        LoanRuleCode.MinimumLoanPeriod or LoanRuleCode.MaximumLoanPeriod or LoanRuleCode.CarAge => InputField.Term,
        _ => InputField.DownPayment
    };

    /// <summary>
    /// The input a malformed request is repeated beside. Only "no loan" is: the other input
    /// errors already show as a message on their own field.
    /// </summary>
    public static InputField FieldFor(InputError error) =>
        error.Kind == InputErrorKind.MustBeLessThanPurchasePrice ? InputField.DownPayment : InputField.None;

    /// <summary>The input whose change brings a missing price back.</summary>
    public static InputField FieldFor(PricingGapCode code) =>
        code == PricingGapCode.OriginationFee ? InputField.Term : InputField.DownPayment;

    public static string PricingGapReason(PricingGap gap) => gap.Code switch
    {
        PricingGapCode.InterestRate =>
            $"Lykill býður enga vexti fyrir fjármögnunarhlutfall yfir {Limit(gap.Maximum)}.",
        PricingGapCode.OriginationFee =>
            $"Lykill veitir aðeins lán til {Number(gap.Minimum)} til {Number(gap.Maximum)} mánaða.",
        _ => "Lykill verðleggur ekki lán á þessum kjörum."
    };

    public const string NoLoanReason = "Útborgunin nær yfir allt kaupverðið, svo það er ekkert lán.";

    /// <summary>A malformed input, worded for the field it belongs to.</summary>
    public static string InputErrorMessage(InputError error) => (error.Field, error.Kind) switch
    {
        (nameof(LoanRequest.PurchasePrice), InputErrorKind.MustBePositive) => "Kaupverðið þarf að vera hærra en 0 kr.",
        (nameof(LoanRequest.PurchasePrice), InputErrorKind.TooLarge) => "Kaupverðið er of hátt. Lækkaðu kaupverðið.",
        (nameof(LoanRequest.DownPayment), InputErrorKind.MustBeLessThanPurchasePrice) => NoLoanReason,
        (nameof(LoanRequest.LoanPeriodInMonths), InputErrorKind.MustBePositive) => "Lánstíminn þarf að vera lengri en 0 mánuðir.",
        (nameof(LoanRequest.LoanPeriodInMonths), InputErrorKind.TooLarge) => "Lánstíminn er of langur. Styttu lánstímann.",
        _ => "Athugaðu þetta gildi."
    };

    private static string CarAgeMessage(decimal maximumCombinedYears, int? maximumLoanPeriodInMonths)
    {
        // The contract does not say which band of the car-age rule failed, so the message is tied
        // to the current inputs: true in both bands, and it sends nobody to a change that cannot
        // help (docs/ui-plans/loan-calculator.md, review rounds 1–2).
        string limit = $"Miðað við núverandi forsendur mega aldur bílsins og lánstíminn samanlagt ekki vera meira en {Number(maximumCombinedYears)} ár.";

        return maximumLoanPeriodInMonths switch
        {
            0 => $"{limit} Bíllinn nær því hámarki einn og sér, svo enginn lánstími er í boði miðað við þessar forsendur.",
            int months => $"{limit} Styttu lánstímann í {MonthsAfterPreposition(months)} eða skemur.",
            null => $"{limit} Styttu lánstímann."
        };
    }
}
