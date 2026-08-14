using CleanCodeJN.GenericApis.DataGrid.Models;
using Microsoft.AspNetCore.Components;

namespace CleanCodeJN.GenericApis.DataGrid.Components;

/// <summary>
/// Declares one column of a <c>CCJNDataGrid</c>. Place these inside the grid's <c>Columns</c>
/// fragment. As soon as a single column is declared, auto-detection is switched off and the
/// declared columns define both the set of columns and their order.
/// </summary>
/// <typeparam name="TDto">The DTO type used for displaying rows.</typeparam>
public class CCJNColumn<TDto> : ComponentBase
    where TDto : class
{
    [CascadingParameter] private IColumnRegistry<TDto> Registry { get; set; }

    /// <summary>
    /// Name of the DTO property this column shows. Leave empty for a column that is built
    /// entirely by <see cref="CellTemplate"/>; name the properties it reads in <see cref="Fields"/>
    /// so they are still requested from the server.
    /// </summary>
    [Parameter] public string Property { get; set; }

    /// <summary>Header text. Falls back to the property name split on capitals.</summary>
    [Parameter] public string Title { get; set; }

    /// <summary>
    /// Server-side sorting. Enabled by default for property-backed columns, disabled for
    /// template-only ones unless <see cref="SortBy"/> names a property.
    /// </summary>
    [Parameter] public bool? Sortable { get; set; }

    /// <summary>Property to sort by when it differs from <see cref="Property"/>.</summary>
    [Parameter] public string SortBy { get; set; }

    /// <summary>Further DTO properties the template reads. They are requested but get no column.</summary>
    [Parameter] public string[] Fields { get; set; }

    /// <summary>Custom cell rendering. Without it the property value is formatted as usual.</summary>
    [Parameter] public RenderFragment<TDto> CellTemplate { get; set; }

    protected override void OnInitialized() => Registry?.Register(this);
}
