using CleanCodeJN.GenericApis.DataGrid.Components;

namespace CleanCodeJN.GenericApis.DataGrid.Models;

/// <summary>
/// Cascaded by <c>CCJNDataGrid</c> so that the <c>CCJNColumn</c> children declared in its
/// <c>Columns</c> fragment can announce themselves. Only the DTO type is involved, so a column
/// does not have to repeat the grid's other type parameters.
/// </summary>
public interface IColumnRegistry<TDto>
    where TDto : class
{
    /// <summary>Called once per column while the grid renders its <c>Columns</c> fragment.</summary>
    void Register(CCJNColumn<TDto> column);
}
