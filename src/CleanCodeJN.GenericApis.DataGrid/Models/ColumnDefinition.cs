namespace CleanCodeJN.GenericApis.DataGrid.Models;

/// <summary>
/// Describes a single auto-detected column for the <see cref="Components.CCJNDataGrid{TDto}"/>.
/// </summary>
public class ColumnDefinition<TDto>
{
    /// <summary>Column header text shown in the table.</summary>
    public string Header { get; set; } = string.Empty;

    /// <summary>camelCase field name used in the GraphQL selection set and where-filter.</summary>
    public string FieldName { get; set; } = string.Empty;

    /// <summary>Original PascalCase property name forwarded to the GraphQL <c>order[].field</c> argument.</summary>
    public string SortFieldName { get; set; } = string.Empty;

    /// <summary>Underlying (non-nullable) CLR type of the property — used to decide formatting and search eligibility.</summary>
    public Type PropertyType { get; set; } = typeof(object);

    /// <summary>Whether the column supports server-side sorting.</summary>
    public bool Sortable { get; set; } = true;

    /// <summary>Produces a display string for a row item.</summary>
    public Func<TDto, string> GetDisplayValue { get; set; } = _ => string.Empty;
}
