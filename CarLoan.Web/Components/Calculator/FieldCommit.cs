namespace CarLoan.Web.Components.Calculator;

/// <summary>Where a committed field value came from.</summary>
public enum CommitSource
{
    /// <summary>The user stopped typing for a moment and is still in the field.</summary>
    TypingPause,

    /// <summary>The user left the field.</summary>
    LeftField,

    /// <summary>The user moved the slider.</summary>
    Slider
}

/// <summary>A value the user settled on; null when the field is empty.</summary>
public sealed record FieldCommit(decimal? Value, CommitSource Source);
