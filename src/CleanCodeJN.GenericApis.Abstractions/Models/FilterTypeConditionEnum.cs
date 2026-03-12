namespace CleanCodeJN.GenericApis.Abstractions.Models;

/// <summary>
/// Specifies how multiple filter criteria are combined when building a compound query.
/// </summary>
public enum FilterTypeConditionEnum
{
    /// <summary>All filter criteria must be satisfied (logical AND).</summary>
    AND = 0,

    /// <summary>At least one filter criterion must be satisfied (logical OR).</summary>
    OR = 1,
}
