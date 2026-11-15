namespace CarLoan.Web.Components.Calculator;

/// <summary>One option in a <see cref="SegmentedChoice{TValue}"/>.</summary>
public sealed record ChoiceOption<TValue>(TValue Value, string Label);
