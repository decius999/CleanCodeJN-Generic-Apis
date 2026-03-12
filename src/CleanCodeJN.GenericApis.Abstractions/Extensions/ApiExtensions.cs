using System.Text.Json;
using CleanCodeJN.GenericApis.Abstractions.Models;

namespace CleanCodeJN.GenericApis.Abstractions.Extensions;

/// <summary>
/// Provides extension methods for common API utility operations such as sort order normalization and filter deserialization.
/// </summary>
public static class ApiExtensions
{
    /// <summary>
    /// Converts a sort direction string to its numeric representation ("-1" for descending, "1" for ascending).
    /// </summary>
    /// <param name="direction">The sort direction string, typically "asc" or "desc".</param>
    /// <returns>"-1" if the direction contains "desc"; otherwise "1".</returns>
    public static string GetSortOrder(this string direction) => direction?.Contains("desc") == true ? "-1" : "1";

    /// <summary>
    /// Deserializes a JSON string into a <see cref="SearchFilter"/> instance.
    /// </summary>
    /// <param name="filter">The JSON string representing a <see cref="SearchFilter"/>, or <c>null</c>.</param>
    /// <returns>A <see cref="SearchFilter"/> deserialized from the JSON string, or <c>null</c> if the input is <c>null</c>.</returns>
    public static SearchFilter ToFilter(this string filter) => filter is null ? null : JsonSerializer.Deserialize<SearchFilter>(filter);
}
