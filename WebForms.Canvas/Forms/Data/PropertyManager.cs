namespace System.Windows.Forms;

/// <summary>
/// Manages a single-object (scalar) data source for data-bound controls.
/// Returned by <see cref="BindingContext"/> when the data source is not a list.
/// Position is always 0 and Count is always 1.
/// </summary>
public class PropertyManager : BindingManagerBase
{
    private object? _dataSource;

    internal PropertyManager(object? dataSource) => _dataSource = dataSource;

    /// <inheritdoc/>
    public override int Count => _dataSource is null ? 0 : 1;

    /// <inheritdoc/>
    public override int Position
    {
        get => 0;
        set { /* scalar — position is always 0 */ }
    }

    /// <inheritdoc/>
    public override object? Current => _dataSource;

    /// <inheritdoc/>
    public override void EndCurrentEdit() { }

    /// <inheritdoc/>
    public override void CancelCurrentEdit() { }
}
