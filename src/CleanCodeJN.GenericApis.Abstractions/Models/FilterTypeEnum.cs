namespace CleanCodeJN.GenericApis.Abstractions.Models;

/// <summary>
/// Specifies the data type of a filter field, enabling type-aware query construction.
/// </summary>
public enum FilterTypeEnum
{
    /// <summary>The filter field is a string value.</summary>
    STRING = 0,

    /// <summary>The filter field is a non-nullable integer value.</summary>
    INTEGER = 1,

    /// <summary>The filter field is a non-nullable double-precision floating-point value.</summary>
    DOUBLE = 2,

    /// <summary>The filter field is a nullable integer value.</summary>
    INTEGER_NULLABLE = 3,

    /// <summary>The filter field is a nullable double-precision floating-point value.</summary>
    DOUBLE_NULLABLE = 4,

    /// <summary>The filter field is a non-nullable date and time value.</summary>
    DATETIME = 5,

    /// <summary>The filter field is a nullable date and time value.</summary>
    DATETIME_NULLABLE = 6,

    /// <summary>The filter field is a non-nullable GUID value.</summary>
    GUID = 7,

    /// <summary>The filter field is a nullable GUID value.</summary>
    GUID_NULLABLE = 8,
}
