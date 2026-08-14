using Microsoft.AspNetCore.Components;

namespace CleanCodeJN.GenericApis.DataGrid.Models;

/// <summary>
/// Describes a single column for <c>CCJNDataGrid</c> — either auto-detected from the DTO or
/// declared through a <c>CCJNColumn</c>.
/// </summary>
public class ColumnDefinition<TDto>
{
    /// <summary>Column header text shown in the table.</summary>
    public string Header { get; set; } = string.Empty;

    /// <summary>
    /// camelCase field name used in the GraphQL selection set and where-filter.
    /// <c>null</c> for template-only columns that are not backed by a single property.
    /// </summary>
    public string FieldName { get; set; }

    /// <summary>Original PascalCase property name forwarded to the GraphQL <c>order[].field</c> argument.</summary>
    public string SortFieldName { get; set; } = string.Empty;

    /// <summary>Underlying (non-nullable) CLR type of the property — used to decide formatting and search eligibility.</summary>
    public Type PropertyType { get; set; } = typeof(object);

    /// <summary>Whether the column supports server-side sorting.</summary>
    public bool Sortable { get; set; } = true;

    /// <summary>
    /// Further camelCase field names the cell template reads. They are requested from the server
    /// but get no column of their own.
    /// </summary>
    public string[] AdditionalFieldNames { get; set; } = [];

    /// <summary>Custom cell rendering. When set it replaces <see cref="GetDisplayValue"/>.</summary>
    public RenderFragment<TDto> CellTemplate { get; set; }

    /// <summary>Produces a display string for a row item.</summary>
    public Func<TDto, string> GetDisplayValue { get; set; } = _ => string.Empty;
}
