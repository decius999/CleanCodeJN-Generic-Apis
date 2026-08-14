using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using CleanCodeJN.GenericApis.DataGrid.Models;
using CleanCodeJN.GenericApis.DataGrid.Services;
using FluentValidation;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor;

namespace CleanCodeJN.GenericApis.DataGrid.Components;

/// <summary>
/// A fully generic, server-side MudBlazor table that auto-discovers columns from
/// <typeparamref name="TDto"/> and queries a CleanCodeJN GraphQL endpoint with
/// paging, sorting, global text search, and optional CRUD mutations.
/// </summary>
/// <typeparam name="TDto">The DTO type used for displaying rows.</typeparam>
/// <typeparam name="TPostDto">The DTO type used for the create form. Defaults to <typeparamref name="TDto"/>.</typeparam>
/// <typeparam name="TPutDto">The DTO type used for the update form. Defaults to <typeparamref name="TDto"/>.</typeparam>
public partial class CCJNDataGrid<TDto, TPostDto, TPutDto> : IColumnRegistry<TDto>
    where TDto : class
    where TPostDto : class
    where TPutDto : class
{
    // ── Injected services ─────────────────────────────────────────────────────

    [Inject] private GraphQLDataGridService GraphQLService { get; set; } = default!;
    [Inject] private IDialogService DialogService { get; set; } = default!;
    [Inject] private IServiceProvider ServiceProvider { get; set; } = default!;

    // ── Required parameters ───────────────────────────────────────────────────

    /// <summary>Full URL of the GraphQL endpoint.</summary>
    [Parameter, EditorRequired] public string GraphQLEndpoint { get; set; } = string.Empty;

    /// <summary>Lowercase GraphQL query field name, e.g. <c>"customer"</c>.</summary>
    [Parameter, EditorRequired] public string EntityName { get; set; } = string.Empty;

    // ── Optional display parameters ───────────────────────────────────────────

    [Parameter] public string Title { get; set; } = string.Empty;
    [Parameter] public bool Searchable { get; set; } = true;
    [Parameter] public bool Dense { get; set; } = false;
    [Parameter] public bool Striped { get; set; } = true;
    [Parameter] public bool Hover { get; set; } = true;
    [Parameter] public int Elevation { get; set; } = 2;
    [Parameter] public string BearerToken { get; set; }
    [Parameter] public HashSet<string> ExcludedProperties { get; set; } = [];
    [Parameter] public int[] PageSizeOptions { get; set; } = [10, 25, 50, 100];
    [Parameter] public Dictionary<string, string> ColumnHeaders { get; set; }

    /// <summary>
    /// Properties that are requested from the server but get no column. Use them for values a
    /// row-click dialog or a cell template needs while the column itself would only cost width.
    /// Unlike <see cref="ExcludedProperties"/> these stay part of the query.
    /// </summary>
    [Parameter] public HashSet<string> HiddenProperties { get; set; } = [];

    // ── Column declaration ────────────────────────────────────────────────────

    /// <summary>
    /// Explicit columns as <c>CCJNColumn</c> children. When set, they replace auto-detection
    /// entirely and their order is the order of the table.
    /// </summary>
    [Parameter] public RenderFragment Columns { get; set; }

    // ── Row parameters ────────────────────────────────────────────────────────

    /// <summary>Raised with the clicked row's item. The edit and delete buttons do not trigger it.</summary>
    [Parameter] public EventCallback<TDto> OnRowClick { get; set; }

    /// <summary>CSS class put on every row — <c>cursor-pointer</c> for a clickable table, for instance.</summary>
    [Parameter] public string RowClass { get; set; }

    /// <summary>Inline style put on every row.</summary>
    [Parameter] public string RowStyle { get; set; }

    /// <summary>Per-row CSS class, receiving the item and its index. Added on top of <see cref="RowClass"/>.</summary>
    [Parameter] public Func<TDto, int, string> RowClassFunc { get; set; }

    /// <summary>Per-row inline style, receiving the item and its index. Added on top of <see cref="RowStyle"/>.</summary>
    [Parameter] public Func<TDto, int, string> RowStyleFunc { get; set; }

    /// <summary>
    /// Read-only detail view for a clicked row. When set, a click opens a dialog showing this
    /// content with nothing but a close button. <see cref="OnRowClick"/> still fires.
    /// </summary>
    [Parameter] public RenderFragment<TDto> ViewFormContent { get; set; }

    /// <summary>Heading of the detail dialog. Falls back to <see cref="Title"/>.</summary>
    [Parameter] public string ViewTitle { get; set; }

    // ── Dialog wording ────────────────────────────────────────────────────────

    /// <summary>Label of the detail dialog's only button.</summary>
    [Parameter] public string CloseLabel { get; set; } = "Close";

    /// <summary>Label of the save button in the add and edit dialogs.</summary>
    [Parameter] public string SubmitLabel { get; set; } = "Save";

    /// <summary>Label of the cancel button in every dialog.</summary>
    [Parameter] public string CancelLabel { get; set; } = "Cancel";

    /// <summary>Label of the confirming button in the delete dialog.</summary>
    [Parameter] public string DeleteLabel { get; set; } = "Delete";

    /// <summary>Question asked before deleting.</summary>
    [Parameter] public string DeleteConfirmText { get; set; } = "Are you sure you want to delete this entry?";

    /// <summary>Heading of the delete dialog.</summary>
    [Parameter] public string DeleteTitle { get; set; } = "Confirm Delete";

    // ── CRUD parameters ───────────────────────────────────────────────────────

    /// <summary>Show an Add button and open a create dialog.</summary>
    [Parameter] public bool AllowAdd { get; set; }

    /// <summary>Show an Edit button per row and open an update dialog.</summary>
    [Parameter] public bool AllowEdit { get; set; }

    /// <summary>
    /// Open the edit dialog by clicking the row instead of a pencil button, which is then not
    /// rendered. Takes precedence over <see cref="ViewFormContent"/> — editing already shows
    /// the values, a separate read-only view would only be in the way.
    /// </summary>
    [Parameter] public bool EditOnRowClick { get; set; }

    /// <summary>Show a Delete button per row with a confirmation dialog.</summary>
    [Parameter] public bool AllowDelete { get; set; }

    /// <summary>
    /// GraphQL mutation name prefixes. Defaults match <c>CleanCodeNamingConventions</c>
    /// (<c>create</c>, <c>update</c>, <c>delete</c>).
    /// </summary>
    [Parameter] public DataGridMutationConventions MutationConventions { get; set; } = new();

    /// <summary>
    /// DTO property names to exclude from the auto-generated Add/Edit forms.
    /// Use this to hide technical fields like <c>Id</c> from the create form.
    /// </summary>
    [Parameter] public HashSet<string> ExcludedEditProperties { get; set; } = [];

    /// <summary>
    /// Custom Add form content. Receives a fresh <typeparamref name="TPostDto"/> as context.
    /// When set, replaces the auto-generated form.
    /// </summary>
    [Parameter] public RenderFragment<TPostDto> AddFormContent { get; set; }

    /// <summary>
    /// Custom Edit form content. Receives the pre-populated <typeparamref name="TPutDto"/> as context.
    /// When set, replaces the auto-generated form.
    /// </summary>
    [Parameter] public RenderFragment<TPutDto> EditFormContent { get; set; }

    /// <summary>
    /// Custom Add handler. When set, called instead of the auto-generated GraphQL create mutation.
    /// </summary>
    [Parameter] public Func<TPostDto, Task> OnCustomAdd { get; set; }

    /// <summary>
    /// Custom Edit handler. Receives the entity ID and the populated <typeparamref name="TPutDto"/>.
    /// When set, called instead of the auto-generated GraphQL update mutation.
    /// </summary>
    [Parameter] public Func<object, TPutDto, Task> OnCustomEdit { get; set; }

    /// <summary>
    /// Custom Delete handler. Receives the entity ID.
    /// When set, called instead of the auto-generated GraphQL delete mutation.
    /// </summary>
    [Parameter] public Func<object, Task> OnCustomDelete { get; set; }

    // ── Internal state ────────────────────────────────────────────────────────

    private MudTable<TDto> _table = default!;
    private List<ColumnDefinition<TDto>> _columns = [];
    private readonly List<CCJNColumn<TDto>> _declaredColumns = [];
    private bool _columnsReady;
    private string _searchTerm = string.Empty;
    private string _activeSearchTerm = string.Empty;
    private CancellationTokenSource _debounceCts = new();
    private bool _loading;
    private string _errorMessage = string.Empty;
    private Func<TableState, CancellationToken, Task<TableData<TDto>>> _serverDataDelegate = default!;
    private MudForm _dialogForm;

    // ── Scalar type whitelist ─────────────────────────────────────────────────

    private static readonly HashSet<Type> ScalarTypes =
    [
        typeof(string),
        typeof(bool),
        typeof(char),
        typeof(byte), typeof(sbyte),
        typeof(short), typeof(ushort),
        typeof(int), typeof(uint),
        typeof(long), typeof(ulong),
        typeof(float), typeof(double), typeof(decimal),
        typeof(DateTime), typeof(DateTimeOffset),
        typeof(DateOnly), typeof(TimeOnly),
        typeof(TimeSpan),
        typeof(Guid),
    ];

    // ── Lifecycle ─────────────────────────────────────────────────────────────

    protected override void OnInitialized()
    {
        _serverDataDelegate = LoadServerData;

        if (Columns is null)
        {
            _columns = BuildColumns();
            _columnsReady = true;
        }
    }

    /// <summary>
    /// Declared columns announce themselves while the <c>Columns</c> fragment renders, which is
    /// after <c>OnInitialized</c>. The table therefore waits for the first render before it asks
    /// the server — otherwise the query would be built from the wrong set of fields.
    /// </summary>
    protected override void OnAfterRender(bool firstRender)
    {
        if (!firstRender || _columnsReady)
        {
            return;
        }

        _columns = [.. _declaredColumns.Select(ToColumnDefinition)];
        _columnsReady = true;
        StateHasChanged();
    }

    /// <inheritdoc />
    void IColumnRegistry<TDto>.Register(CCJNColumn<TDto> column) => _declaredColumns.Add(column);

    // ── Column detection ──────────────────────────────────────────────────────

    private List<ColumnDefinition<TDto>> BuildColumns() => typeof(TDto)
        .GetProperties(BindingFlags.Public | BindingFlags.Instance)
        .Where(p => p.CanRead && IsScalarProperty(p) && !ExcludedProperties.Contains(p.Name) && !HiddenProperties.Contains(p.Name))
        .Select(p =>
        {
            var underlyingType = Nullable.GetUnderlyingType(p.PropertyType) ?? p.PropertyType;
            return new ColumnDefinition<TDto>
            {
                Header = ResolveHeader(p.Name),
                FieldName = ToCamelCase(p.Name),
                SortFieldName = p.Name,
                PropertyType = underlyingType,
                Sortable = true,
                GetDisplayValue = dto => FormatValue(p.GetValue(dto), underlyingType),
            };
        })
        .ToList();

    private ColumnDefinition<TDto> ToColumnDefinition(CCJNColumn<TDto> column)
    {
        var property = string.IsNullOrWhiteSpace(column.Property)
            ? null
            : typeof(TDto).GetProperty(column.Property, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);

        var underlyingType = property is null
            ? typeof(object)
            : Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;

        var sortFieldName = column.SortBy ?? property?.Name;

        return new ColumnDefinition<TDto>
        {
            Header = column.Title ?? (property is null ? string.Empty : ResolveHeader(property.Name)),
            FieldName = property is null ? null : ToCamelCase(property.Name),
            SortFieldName = sortFieldName ?? string.Empty,
            PropertyType = underlyingType,
            Sortable = (column.Sortable ?? property is not null) && sortFieldName is not null,
            AdditionalFieldNames = [.. (column.Fields ?? []).Select(ToCamelCase)],
            CellTemplate = column.CellTemplate,
            GetDisplayValue = property is null
                ? _ => string.Empty
                : dto => FormatValue(property.GetValue(dto), underlyingType),
        };
    }

    private string ResolveHeader(string propertyName) => ColumnHeaders is not null && ColumnHeaders.TryGetValue(propertyName, out var custom)
            ? custom
            : Regex.Replace(propertyName, "([A-Z])", " $1").TrimStart();

    private static bool IsScalarProperty(PropertyInfo p)
    {
        var type = Nullable.GetUnderlyingType(p.PropertyType) ?? p.PropertyType;
        return ScalarTypes.Contains(type) || type.IsEnum;
    }

    // ── Server-side data loading ──────────────────────────────────────────────

    private async Task<TableData<TDto>> LoadServerData(TableState state, CancellationToken ct)
    {
        _loading = true;
        _errorMessage = string.Empty;
        try
        {
            var skip = state.Page * state.PageSize;
            var query = BuildBatchedGraphQLQuery(skip, state.PageSize, state.SortLabel, state.SortDirection, _activeSearchTerm);
            var (items, totalCount) = await GraphQLService.QueryWithCountAsync<TDto>(GraphQLEndpoint, query, EntityName, BearerToken, ct);
            return new TableData<TDto> { Items = items, TotalItems = totalCount };
        }
        catch (Exception ex)
        {
            _errorMessage = $"Error loading data: {ex.Message}";
            return new TableData<TDto> { Items = [], TotalItems = 0 };
        }
        finally
        {
            _loading = false;
        }
    }

    // ── GraphQL query builder ─────────────────────────────────────────────────

    private string BuildBatchedGraphQLQuery(int skip, int take, string sortLabel, SortDirection sortDirection, string activeSearch)
    {
        var fields = string.Join("\n    ", SelectionFieldNames());
        var whereClause = BuildWhereClause(activeSearch);
        var listArgs = new List<string> { $"skip: {skip}", $"take: {take}" };

        if (!string.IsNullOrWhiteSpace(sortLabel))
        {
            var dir = sortDirection == SortDirection.Descending ? "DESC" : "ASC";
            listArgs.Add($"order: [{{ field: \"{sortLabel}\", direction: {dir} }}]");
        }

        if (whereClause is not null)
        {
            listArgs.Add(whereClause);
        }

        var countArgs = whereClause is not null ? $"({whereClause})" : string.Empty;

        return $@"{{
  {EntityName}({string.Join(", ", listArgs)}) {{
    {fields}
  }}
  {EntityName}Count{countArgs}
}}";
    }

    /// <summary>
    /// Everything the rows need: the columns themselves, whatever their templates read, the
    /// hidden properties, and always the key — edit and delete are lost without it.
    /// The edit form's own fields come along even when they have no column: it writes every one
    /// of them back, so a value left unloaded would be saved as its default and quietly replace
    /// what was there.
    /// </summary>
    private List<string> SelectionFieldNames()
    {
        var fields = new List<string>();

        foreach (var column in _columns)
        {
            if (column.FieldName is not null)
            {
                fields.Add(column.FieldName);
            }

            fields.AddRange(column.AdditionalFieldNames);
        }

        fields.AddRange(HiddenProperties.Select(ToCamelCase));

        if (AllowEdit)
        {
            fields.AddRange(EditableFieldNames());
        }

        if (typeof(TDto).GetProperty("Id", BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase) is not null)
        {
            fields.Add("id");
        }

        return [.. fields.Distinct(StringComparer.Ordinal)];
    }

    private static IEnumerable<string> EditableFieldNames()
    {
        var readable = typeof(TDto)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.CanRead && IsScalarProperty(p))
            .Select(p => p.Name)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        return typeof(TPutDto)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.CanWrite && IsScalarProperty(p) && readable.Contains(p.Name))
            .Select(p => ToCamelCase(p.Name));
    }

    private string BuildWhereClause(string search)
    {
        if (string.IsNullOrWhiteSpace(search))
        {
            return null;
        }

        var conditions = new List<string>();
        var escaped = EscapeGraphQLString(search);

        foreach (var col in _columns.Where(c => c.FieldName is not null))
        {
            if (col.PropertyType == typeof(string))
            {
                conditions.Add($"{{ {col.FieldName}: {{ contains: \"{escaped}\" }} }}");
            }
            else
            {
                var eqValue = TryBuildEqLiteral(search, col.PropertyType);
                if (eqValue is not null)
                {
                    conditions.Add($"{{ {col.FieldName}: {{ eq: {eqValue} }} }}");
                }
            }
        }

        return conditions.Count == 0 ? null : $"where: {{ or: [{string.Join(", ", conditions)}] }}";
    }

    private static string TryBuildEqLiteral(string search, Type type)
    {
        if (type == typeof(Guid))
        {
            return Guid.TryParse(search, out var g) ? $"\"{g}\"" : null;
        }

        if (type == typeof(bool))
        {
            return bool.TryParse(search, out var b) ? b.ToString().ToLowerInvariant() : null;
        }

        return type == typeof(int) || type == typeof(short) || type == typeof(byte) || type == typeof(sbyte) || type == typeof(ushort) || type == typeof(uint)
            ? int.TryParse(search, out var i) ? i.ToString() : null
            : type == typeof(long) || type == typeof(ulong)
            ? long.TryParse(search, out var l) ? l.ToString() : null
            : type == typeof(decimal)
            ? decimal.TryParse(search, System.Globalization.NumberStyles.Number, System.Globalization.CultureInfo.InvariantCulture, out var d)
                ? d.ToString(System.Globalization.CultureInfo.InvariantCulture) : null
            : type == typeof(double)
            ? double.TryParse(search, System.Globalization.NumberStyles.Number, System.Globalization.CultureInfo.InvariantCulture, out var db)
                ? db.ToString(System.Globalization.CultureInfo.InvariantCulture) : null
            : type == typeof(float)
            ? float.TryParse(search, System.Globalization.NumberStyles.Number, System.Globalization.CultureInfo.InvariantCulture, out var f)
                ? f.ToString(System.Globalization.CultureInfo.InvariantCulture) : null
            : null;
    }

    private static string EscapeGraphQLString(string value)
    {
        var sb = new StringBuilder(value.Length);
        foreach (var ch in value)
        {
            sb.Append(ch switch
            {
                '\\' => "\\\\",
                '"' => "\\\"",
                '\n' => "\\n",
                '\r' => "\\r",
                '\t' => "\\t",
                _ => ch.ToString(),
            });
        }

        return sb.ToString();
    }

    // ── Display formatting ────────────────────────────────────────────────────

    private static string FormatValue(object value, Type type)
    {
        if (value is null)
        {
            return string.Empty;
        }

        if (type == typeof(DateTime))
        {
            return ((DateTime)value).ToString("dd.MM.yyyy HH:mm");
        }

        if (type == typeof(DateTimeOffset))
        {
            return ((DateTimeOffset)value).ToString("dd.MM.yyyy HH:mm");
        }

        return type == typeof(DateOnly)
            ? ((DateOnly)value).ToString("dd.MM.yyyy")
            : type == typeof(TimeOnly)
            ? ((TimeOnly)value).ToString("HH:mm")
            : type == typeof(TimeSpan)
            ? ((TimeSpan)value).ToString(@"hh\:mm\:ss")
            : type == typeof(bool)
            ? (bool)value ? "Yes" : "No"
            : type == typeof(decimal)
            ? ((decimal)value).ToString("N2")
            : type == typeof(double)
            ? ((double)value).ToString("N2")
            : type == typeof(float) ? ((float)value).ToString("N2") : value.ToString() ?? string.Empty;
    }

    private static string ToCamelCase(string name) =>
        string.IsNullOrEmpty(name) ? name : char.ToLowerInvariant(name[0]) + name[1..];

    // ── Search handlers ───────────────────────────────────────────────────────

    private async Task OnSearchTermChanged(string value)
    {
        _searchTerm = value;
        await _debounceCts.CancelAsync();
        _debounceCts = new CancellationTokenSource();
        try
        {
            await Task.Delay(500, _debounceCts.Token);
            _activeSearchTerm = _searchTerm;
            await ReloadTable();
        }
        catch (OperationCanceledException) { }
    }

    private async Task OnSearchCleared()
    {
        _searchTerm = string.Empty;
        _activeSearchTerm = string.Empty;
        await _debounceCts.CancelAsync();
        await ReloadTable();
    }

    // ── Public API ────────────────────────────────────────────────────────────

    /// <summary>Programmatically refreshes the table data from the server.</summary>
    public async Task ReloadTable()
    {
        if (_table is not null)
        {
            await _table.ReloadServerData();
        }
    }

    // ── Rows ──────────────────────────────────────────────────────────────────

    private async Task OnRowClickedAsync(TableRowClickEventArgs<TDto> args)
    {
        if (args?.Item is null)
        {
            return;
        }

        if (OnRowClick.HasDelegate)
        {
            await OnRowClick.InvokeAsync(args.Item);
        }

        if (EditOnRowClick && AllowEdit)
        {
            await OpenEditDialogAsync(args.Item);
        }
        else if (ViewFormContent is not null)
        {
            await OpenViewDialogAsync(args.Item);
        }
    }

    /// <summary>
    /// The action column is worth its width only while a button lives in it — with
    /// <see cref="EditOnRowClick"/> and no delete right, none does.
    /// </summary>
    private bool ShowActions => (AllowEdit && !EditOnRowClick) || AllowDelete;

    private string ResolveRowClass(TDto item, int index) => Combine(RowClass, RowClassFunc?.Invoke(item, index), " ");

    private string ResolveRowStyle(TDto item, int index) => Combine(RowStyle, RowStyleFunc?.Invoke(item, index), ";");

    private static string Combine(string constant, string perRow, string separator)
    {
        var parts = new[] { constant, perRow }.Where(x => !string.IsNullOrWhiteSpace(x)).ToArray();

        return parts.Length == 0 ? null : string.Join(separator, parts);
    }

    // ── CRUD: Dialog openers ──────────────────────────────────────────────────

    private async Task OpenViewDialogAsync(TDto item)
    {
        var parameters = new DialogParameters<CCJNDataGridDialog>
        {
            { d => d.Title, ViewTitle ?? (string.IsNullOrEmpty(Title) ? "Details" : Title) },
            { d => d.FormContent, ViewFormContent(item) },
            { d => d.ShowSubmit, false },
            { d => d.ShowCancel, false },
            { d => d.CloseLabel, CloseLabel },
        };

        await DialogService.ShowAsync<CCJNDataGridDialog>(string.Empty, parameters);
    }

    private async Task OpenAddDialogAsync()
    {
        var model = Activator.CreateInstance<TPostDto>()!;
        var formContent = AddFormContent is not null
            ? AddFormContent(model)
            : BuildAutoForm(model);

        var parameters = new DialogParameters<CCJNDataGridDialog>
        {
            { d => d.Title, string.IsNullOrEmpty(Title) ? "Add" : $"Add {Title}" },
            { d => d.FormContent, formContent },
            { d => d.SubmitLabel, SubmitLabel },
            { d => d.CancelLabel, CancelLabel },
            { d => d.OnSave, async () =>
                {
                    if (_dialogForm is not null) { await _dialogForm.ValidateAsync(); if (!_dialogForm.IsValid) { return string.Empty; } }

                    try
                    {
                        if (OnCustomAdd is not null) { await OnCustomAdd(model); } else { await GraphQLService.ExecuteMutationAsync(GraphQLEndpoint, BuildCreateMutation(model), BearerToken); } await ReloadTable();
                        return null; }
                    catch (Exception ex) { return ex.Message; } }
            },
        };

        await DialogService.ShowAsync<CCJNDataGridDialog>(string.Empty, parameters);
    }

    private async Task OpenEditDialogAsync(TDto item)
    {
        var id = GetEntityId(item);
        var model = MapToEditModel(item);
        var formContent = EditFormContent is not null
            ? EditFormContent(model)
            : BuildAutoForm(model);

        var parameters = new DialogParameters<CCJNDataGridDialog>
        {
            { d => d.Title, string.IsNullOrEmpty(Title) ? "Edit" : $"Edit {Title}" },
            { d => d.FormContent, formContent },
            { d => d.SubmitLabel, SubmitLabel },
            { d => d.CancelLabel, CancelLabel },
            { d => d.OnSave, async () =>
                {
                    if (_dialogForm is not null) { await _dialogForm.ValidateAsync(); if (!_dialogForm.IsValid) { return string.Empty; } }

                    try
                    {
                        if (OnCustomEdit is not null) { await OnCustomEdit(id!, model); } else { await GraphQLService.ExecuteMutationAsync(GraphQLEndpoint, BuildUpdateMutation(id!, model), BearerToken); } await ReloadTable();
                        return null; }
                    catch (Exception ex) { return ex.Message; } }
            },
        };

        await DialogService.ShowAsync<CCJNDataGridDialog>(string.Empty, parameters);
    }

    private async Task OpenDeleteDialogAsync(TDto item)
    {
        var id = GetEntityId(item);
        var question = DeleteConfirmText;
        void confirmText(RenderTreeBuilder b) => b.AddContent(0, question);

        var parameters = new DialogParameters<CCJNDataGridDialog>
        {
            { d => d.Title, DeleteTitle },
            { d => d.FormContent, confirmText },
            { d => d.SubmitLabel, DeleteLabel },
            { d => d.CancelLabel, CancelLabel },
            { d => d.SubmitColor, Color.Error },
            { d => d.OnSave, async () =>
                {
                    try
                    {
                        if (OnCustomDelete is not null) { await OnCustomDelete(id!); } else { await GraphQLService.ExecuteMutationAsync(GraphQLEndpoint, BuildDeleteMutation(id!), BearerToken); } await ReloadTable();
                        return null; }
                    catch (Exception ex) { return ex.Message; } }
            },
        };

        await DialogService.ShowAsync<CCJNDataGridDialog>(string.Empty, parameters);
    }

    // ── CRUD: Helpers ─────────────────────────────────────────────────────────

    private static object GetEntityId(TDto item) =>
        typeof(TDto)
            .GetProperty("Id", BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase)
            ?.GetValue(item);

    private static TPutDto MapToEditModel(TDto source)
    {
        var target = Activator.CreateInstance<TPutDto>()!;
        var sourceProps = typeof(TDto)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .ToDictionary(p => p.Name, StringComparer.OrdinalIgnoreCase);

        foreach (var tp in typeof(TPutDto).GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(p => p.CanWrite))
        {
            if (!sourceProps.TryGetValue(tp.Name, out var sp))
            {
                continue;
            }

            try
            {
                tp.SetValue(target, sp.GetValue(source));
            }
            catch { /* skip incompatible */ }
        }

        return target;
    }

    // ── CRUD: GraphQL mutation builders ──────────────────────────────────────

    private string MutationEntityName =>
        char.ToUpperInvariant(EntityName[0]) + EntityName[1..];

    private string BuildCreateMutation(TPostDto model) =>
        $"mutation {{ {MutationConventions.CreatePrefix}{MutationEntityName}(input: {BuildInputLiteral(model)}) {{ __typename }} }}";

    private string BuildUpdateMutation(object id, TPutDto model) =>
        $"mutation {{ {MutationConventions.UpdatePrefix}{MutationEntityName}(id: \"{id}\", input: {BuildInputLiteral(model)}) {{ id }} }}";

    private string BuildDeleteMutation(object id) =>
        $"mutation {{ {MutationConventions.DeletePrefix}{MutationEntityName}(id: \"{id}\") }}";

    private static string BuildInputLiteral(object dto)
    {
        var props = dto.GetType()
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.CanRead && !IsCollectionProperty(p));

        var pairs = new List<string>();
        foreach (var prop in props)
        {
            var value = prop.GetValue(dto);
            var underlying = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;

            if (underlying.IsEnum || ScalarTypes.Contains(underlying))
            {
                if (value is null)
                {
                    continue; // omit null scalars
                }

                pairs.Add($"{ToCamelCase(prop.Name)}: {FormatGraphQLInputValue(value, underlying)}");
            }
            else if (underlying.IsClass)
            {
                // Nested input object — serialize recursively; send null when the reference is null
                pairs.Add(value is null
                    ? $"{ToCamelCase(prop.Name)}: null"
                    : $"{ToCamelCase(prop.Name)}: {BuildInputLiteral(value)}");
            }
        }

        return "{ " + string.Join(", ", pairs) + " }";
    }

    private static bool IsCollectionProperty(PropertyInfo p)
    {
        var t = p.PropertyType;
        if (t.IsArray)
        {
            return true;
        }

        if (!t.IsGenericType)
        {
            return false;
        }

        var def = t.GetGenericTypeDefinition();
        return def == typeof(List<>) || def == typeof(IList<>) ||
               def == typeof(IEnumerable<>) || def == typeof(ICollection<>) ||
               def == typeof(IReadOnlyList<>) || def == typeof(IReadOnlyCollection<>);
    }

    private static string FormatGraphQLInputValue(object value, Type type)
    {
        if (value is null)
        {
            return "null";
        }

        if (type == typeof(string))
        {
            return $"\"{EscapeGraphQLString(value.ToString()!)}\"";
        }

        if (type == typeof(bool))
        {
            return ((bool)value).ToString().ToLowerInvariant();
        }

        if (type == typeof(Guid))
        {
            return $"\"{value}\"";
        }

        if (type == typeof(DateTime))
        {
            return $"\"{(DateTime)value:O}\"";
        }

        if (type == typeof(DateTimeOffset))
        {
            return $"\"{(DateTimeOffset)value:O}\"";
        }

        if (type == typeof(DateOnly))
        {
            return $"\"{(DateOnly)value:yyyy-MM-dd}\"";
        }

        if (type == typeof(TimeOnly))
        {
            return $"\"{(TimeOnly)value:HH:mm:ss}\"";
        }

        if (type == typeof(TimeSpan))
        {
            return $"\"{(TimeSpan)value}\"";
        }

        if (type.IsEnum)
        {
            return value.ToString()!;
        }
        // Use InvariantCulture for all numeric types to ensure '.' as decimal separator
        return ((IFormattable)value).ToString(null, System.Globalization.CultureInfo.InvariantCulture);
    }

    // ── Auto-form via reflection ──────────────────────────────────────────────

    private RenderFragment BuildAutoForm<TModel>(TModel model) where TModel : class => builder =>
    {
        var validator = ServiceProvider.GetService<IValidator<TModel>>();
        var seq = 0;

        builder.OpenComponent<MudForm>(seq++);
        builder.AddAttribute(seq++, "Model", model);

        if (validator is not null)
        {
            Func<object, string, Task<IEnumerable<string>>> validationFunc = async (obj, propName) =>
            {
                var ctx = ValidationContext<TModel>.CreateWithOptions(
                    (TModel)obj, s => s.IncludeProperties(propName));
                var result = await validator.ValidateAsync(ctx);
                return result.Errors
                    .Where(e => e.PropertyName == propName)
                    .Select(e => e.ErrorMessage);
            };
            builder.AddAttribute(seq++, "Validation", validationFunc);
        }

        builder.AddAttribute(seq++, "ChildContent", (RenderFragment)(innerBuilder =>
        {
            var innerSeq = 0;
            var props = typeof(TModel)
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.CanRead && p.CanWrite && IsScalarProperty(p) && !ExcludedEditProperties.Contains(p.Name));

            foreach (var prop in props)
            {
                var forExpr = validator is not null ? TryBuildForExpression(model, prop) : null;
                RenderFormField(innerBuilder, prop, model, forExpr, ref innerSeq);
            }
        }));

        // AddComponentReferenceCapture must come after all AddAttribute calls
        builder.AddComponentReferenceCapture(seq++, r => _dialogForm = r as MudForm);
        builder.CloseComponent();
    };

    /// <summary>
    /// Builds a typed <c>Expression&lt;Func&lt;T&gt;&gt;</c> for a property so MudBlazor can match
    /// it to the MudForm validation delegate via the property name.
    /// Returns null for types whose render component uses a different generic parameter (Guid, Enum,
    /// DateOnly, non-nullable DateTime) to avoid type mismatches.
    /// </summary>
    private static LambdaExpression TryBuildForExpression(object model, PropertyInfo prop)
    {
        var type = prop.PropertyType;
        var underlying = Nullable.GetUnderlyingType(type) ?? type;

        if (underlying == typeof(Guid) || underlying.IsEnum ||
            underlying == typeof(DateOnly) || underlying == typeof(TimeOnly) || underlying == typeof(TimeSpan))
        {
            return null;
        }

        // MudDatePicker.Date is DateTime? — only compatible when the property is already DateTime?.
        if (underlying == typeof(DateTime) && type != typeof(DateTime?))
        {
            return null;
        }

        try
        {
            var propAccess = Expression.Property(Expression.Constant(model), prop);
            var funcType = typeof(Func<>).MakeGenericType(type);
            return Expression.Lambda(funcType, propAccess);
        }
        catch
        {
            return null;
        }
    }

    private void RenderFormField(RenderTreeBuilder b, PropertyInfo prop, object model, LambdaExpression forExpr, ref int seq)
    {
        var underlying = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;
        var label = ResolveHeader(prop.Name);

        if (prop.Name.Equals("Id", StringComparison.OrdinalIgnoreCase))
        {
            RenderReadonlyTextField(b, prop, model, label, ref seq);
            return;
        }

        if (underlying == typeof(string))
        {
            RenderTextField(b, prop, model, label, forExpr, ref seq);
        }
        else if (underlying == typeof(bool))
        {
            RenderCheckBox(b, prop, model, label, forExpr, ref seq);
        }
        else if (underlying == typeof(int) || underlying == typeof(short) || underlying == typeof(byte)
              || underlying == typeof(sbyte) || underlying == typeof(ushort) || underlying == typeof(uint))
        {
            RenderNumericField<int>(b, prop, model, label, forExpr, ref seq);
        }
        else if (underlying == typeof(long) || underlying == typeof(ulong))
        {
            RenderNumericField<long>(b, prop, model, label, forExpr, ref seq);
        }
        else if (underlying == typeof(decimal))
        {
            RenderNumericField<decimal>(b, prop, model, label, forExpr, ref seq);
        }
        else if (underlying == typeof(double))
        {
            RenderNumericField<double>(b, prop, model, label, forExpr, ref seq);
        }
        else if (underlying == typeof(float))
        {
            RenderNumericField<float>(b, prop, model, label, forExpr, ref seq);
        }
        else if (underlying == typeof(DateTime))
        {
            RenderDateTimePicker(b, prop, model, label, ref seq);
        }
        else if (underlying == typeof(DateOnly))
        {
            RenderDateOnlyPicker(b, prop, model, label, ref seq);
        }
        else if (underlying == typeof(Guid))
        {
            RenderGuidField(b, prop, model, label, ref seq);
        }
        else if (underlying.IsEnum)
        {
            RenderEnumSelect(b, prop, model, underlying, label, ref seq);
        }
    }

    private static void RenderReadonlyTextField(RenderTreeBuilder b, PropertyInfo p, object m, string label, ref int seq)
    {
        b.OpenComponent<MudTextField<string>>(seq++);
        b.AddAttribute(seq++, "Value", p.GetValue(m)?.ToString() ?? string.Empty);
        b.AddAttribute(seq++, "Label", label);
        b.AddAttribute(seq++, "Variant", Variant.Outlined);
        b.AddAttribute(seq++, "ReadOnly", true);
        b.AddAttribute(seq++, "Class", "mb-3 d-block");
        b.CloseComponent();
    }

    private void RenderTextField(RenderTreeBuilder b, PropertyInfo p, object m, string label, LambdaExpression forExpr, ref int seq)
    {
        b.OpenComponent<MudTextField<string>>(seq++);
        b.AddAttribute(seq++, "Value", (string)p.GetValue(m));
        b.AddAttribute(seq++, "ValueChanged", EventCallback.Factory.Create<string>(this, v => p.SetValue(m, v)));
        b.AddAttribute(seq++, "Label", label);
        b.AddAttribute(seq++, "Variant", Variant.Outlined);
        b.AddAttribute(seq++, "Immediate", true);
        b.AddAttribute(seq++, "Class", "mb-3 d-block");
        if (forExpr is not null)
        {
            b.AddAttribute(seq++, "For", forExpr);
        }

        b.CloseComponent();
    }

    private void RenderCheckBox(RenderTreeBuilder b, PropertyInfo p, object m, string label, LambdaExpression forExpr, ref int seq)
    {
        if (Nullable.GetUnderlyingType(p.PropertyType) != null)
        {
            b.OpenComponent<MudCheckBox<bool?>>(seq++);
            b.AddAttribute(seq++, "Value", (bool?)p.GetValue(m));
            b.AddAttribute(seq++, "ValueChanged", EventCallback.Factory.Create<bool?>(this, v => p.SetValue(m, v)));
            if (forExpr is not null)
            {
                b.AddAttribute(seq++, "For", forExpr);
            }
        }
        else
        {
            b.OpenComponent<MudCheckBox<bool>>(seq++);
            b.AddAttribute(seq++, "Value", p.GetValue(m) is true);
            b.AddAttribute(seq++, "ValueChanged", EventCallback.Factory.Create<bool>(this, v => p.SetValue(m, v)));
            if (forExpr is not null)
            {
                b.AddAttribute(seq++, "For", forExpr);
            }
        }

        b.AddAttribute(seq++, "Label", label);
        b.AddAttribute(seq++, "Class", "mb-3 d-block");
        b.CloseComponent();
    }

    private void RenderNumericField<T>(RenderTreeBuilder b, PropertyInfo p, object m, string label, LambdaExpression forExpr, ref int seq) where T : struct
    {
        if (Nullable.GetUnderlyingType(p.PropertyType) != null)
        {
            b.OpenComponent<MudNumericField<T?>>(seq++);
            b.AddAttribute(seq++, "Value", p.GetValue(m) is T val ? (T?)val : null);
            b.AddAttribute(seq++, "ValueChanged", EventCallback.Factory.Create<T?>(this, v => p.SetValue(m, v)));
            if (forExpr is not null)
            {
                b.AddAttribute(seq++, "For", forExpr);
            }
        }
        else
        {
            b.OpenComponent<MudNumericField<T>>(seq++);
            b.AddAttribute(seq++, "Value", p.GetValue(m) is T val ? val : default);
            b.AddAttribute(seq++, "ValueChanged", EventCallback.Factory.Create<T>(this, v => p.SetValue(m, v)));
            if (forExpr is not null)
            {
                b.AddAttribute(seq++, "For", forExpr);
            }
        }

        b.AddAttribute(seq++, "Label", label);
        b.AddAttribute(seq++, "Variant", Variant.Outlined);
        b.AddAttribute(seq++, "Immediate", true);
        b.AddAttribute(seq++, "Class", "mb-3 d-block");
        b.CloseComponent();
    }

    private void RenderDateTimePicker(RenderTreeBuilder b, PropertyInfo p, object m, string label, ref int seq)
    {
        var isNullable = Nullable.GetUnderlyingType(p.PropertyType) != null;
        var dt = p.GetValue(m) is DateTime d ? (DateTime?)d : null;
        b.OpenComponent<MudDatePicker>(seq++);
        b.AddAttribute(seq++, "Date", dt);
        b.AddAttribute(seq++, "DateChanged", EventCallback.Factory.Create<DateTime?>(this,
            v => p.SetValue(m, isNullable ? (object)v : v ?? DateTime.Today)));
        b.AddAttribute(seq++, "Label", label);
        b.AddAttribute(seq++, "Variant", Variant.Outlined);
        b.AddAttribute(seq++, "Class", "mb-3 d-block");
        b.CloseComponent();
    }

    private void RenderDateOnlyPicker(RenderTreeBuilder b, PropertyInfo p, object m, string label, ref int seq)
    {
        var isNullable = Nullable.GetUnderlyingType(p.PropertyType) != null;
        var dt = p.GetValue(m) is DateOnly d ? (DateTime?)d.ToDateTime(TimeOnly.MinValue) : null;
        b.OpenComponent<MudDatePicker>(seq++);
        b.AddAttribute(seq++, "Date", dt);
        b.AddAttribute(seq++, "DateChanged", EventCallback.Factory.Create<DateTime?>(this, v =>
        {
            if (v.HasValue)
            {
                p.SetValue(m, DateOnly.FromDateTime(v.Value));
            }
            else if (isNullable)
            {
                p.SetValue(m, null);
            }
        }));
        b.AddAttribute(seq++, "Label", label);
        b.AddAttribute(seq++, "Variant", Variant.Outlined);
        b.AddAttribute(seq++, "Class", "mb-3 d-block");
        b.CloseComponent();
    }

    private void RenderGuidField(RenderTreeBuilder b, PropertyInfo p, object m, string label, ref int seq)
    {
        var isNullable = Nullable.GetUnderlyingType(p.PropertyType) != null;
        b.OpenComponent<MudTextField<string>>(seq++);
        b.AddAttribute(seq++, "Value", p.GetValue(m)?.ToString() ?? string.Empty);
        b.AddAttribute(seq++, "ValueChanged", EventCallback.Factory.Create<string>(this, v =>
        {
            if (Guid.TryParse(v, out var g))
            {
                p.SetValue(m, g);
            }
            else if (isNullable && string.IsNullOrEmpty(v))
            {
                p.SetValue(m, null);
            }
        }));
        b.AddAttribute(seq++, "Label", label);
        b.AddAttribute(seq++, "Variant", Variant.Outlined);
        b.AddAttribute(seq++, "Class", "mb-3 d-block");
        b.CloseComponent();
    }

    private void RenderEnumSelect(RenderTreeBuilder b, PropertyInfo p, object m, Type enumType, string label, ref int seq)
    {
        var names = Enum.GetNames(enumType);
        var current = p.GetValue(m)?.ToString() ?? string.Empty;
        b.OpenComponent<MudSelect<string>>(seq++);
        b.AddAttribute(seq++, "Value", current);
        b.AddAttribute(seq++, "ValueChanged", EventCallback.Factory.Create<string>(this, v =>
        {
            if (Enum.TryParse(enumType, v, out var val))
            {
                p.SetValue(m, val);
            }
        }));
        b.AddAttribute(seq++, "Label", label);
        b.AddAttribute(seq++, "Variant", Variant.Outlined);
        b.AddAttribute(seq++, "Class", "mb-3 d-block");
        b.AddAttribute(seq++, "ChildContent", (RenderFragment)(ib =>
        {
            var i = 0;
            foreach (var name in names)
            {
                ib.OpenComponent<MudSelectItem<string>>(i++);
                ib.AddAttribute(i++, "Value", name);
                ib.CloseComponent();
            }
        }));
        b.CloseComponent();
    }
}
