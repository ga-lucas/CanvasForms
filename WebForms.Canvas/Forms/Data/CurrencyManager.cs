namespace System.Windows.Forms;

/// <summary>
/// Manages a list data source for data-bound controls.
/// Wraps any <see cref="System.Collections.IList"/> (including <see cref="BindingSource"/>)
/// and tracks the current row position.
/// </summary>
public class CurrencyManager : BindingManagerBase
{
    private System.Collections.IList _list;
    private int _position = -1;

    internal CurrencyManager(object dataSource)
    {
        if (dataSource is System.Collections.IList list)
            _list = list;
        else if (dataSource is System.ComponentModel.IListSource src)
            _list = src.GetList();
        else
            _list = new List<object> { dataSource };

        _position = _list.Count > 0 ? 0 : -1;
    }

    /// <summary>Gets the underlying list managed by this currency manager.</summary>
    public System.Collections.IList List => _list;

    /// <inheritdoc/>
    public override int Count => _list.Count;

    /// <inheritdoc/>
    public override int Position
    {
        get => _position;
        set
        {
            int clamped = _list.Count == 0 ? -1 : Math.Max(0, Math.Min(value, _list.Count - 1));
            if (clamped == _position) return;
            _position = clamped;
            OnPositionChanged(EventArgs.Empty);
            OnCurrentChanged(EventArgs.Empty);
        }
    }

    /// <inheritdoc/>
    public override object? Current => (_position >= 0 && _position < _list.Count) ? _list[_position] : null;

    /// <inheritdoc/>
    public override void EndCurrentEdit() { /* no pending edit model in stub */ }

    /// <inheritdoc/>
    public override void CancelCurrentEdit() { /* no pending edit model in stub */ }

    /// <summary>Adds a new item to the list (delegates to <see cref="IBindingList"/> if available).</summary>
    public void AddNew()
    {
        if (_list is System.ComponentModel.IBindingList bl && bl.AllowNew)
            bl.AddNew();
    }

    /// <summary>Removes the item at <paramref name="index"/> from the list.</summary>
    public void RemoveAt(int index)
    {
        if (index >= 0 && index < _list.Count)
            _list.RemoveAt(index);
    }
}
