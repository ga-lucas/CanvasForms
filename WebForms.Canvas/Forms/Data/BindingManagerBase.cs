namespace System.Windows.Forms;

/// <summary>
/// Abstract base class for managing the binding between a data-bound control
/// and a data source.  Provides the common interface that <see cref="CurrencyManager"/>
/// and <see cref="PropertyManager"/> implement.
/// </summary>
public abstract class BindingManagerBase
{
    private readonly List<Binding> _bindings = new();

    /// <summary>Gets the collection of bindings managed by this manager.</summary>
    public BindingsCollection Bindings { get; } = new BindingsCollection();

    /// <summary>Gets the number of rows in the underlying data source.</summary>
    public abstract int Count { get; }

    /// <summary>Gets or sets the position in the underlying list.</summary>
    public abstract int Position { get; set; }

    /// <summary>Gets the current object from the underlying data source.</summary>
    public abstract object? Current { get; }

    /// <summary>Forces any pending changes to be written back to the data source.</summary>
    public abstract void EndCurrentEdit();

    /// <summary>Cancels the current edit operation.</summary>
    public abstract void CancelCurrentEdit();

    /// <summary>Moves to the first item in the list.</summary>
    public void MoveFirst() => Position = 0;

    /// <summary>Moves to the last item in the list.</summary>
    public void MoveLast() => Position = Math.Max(0, Count - 1);

    /// <summary>Moves to the next item in the list.</summary>
    public void MoveNext() { if (Position < Count - 1) Position++; }

    /// <summary>Moves to the previous item in the list.</summary>
    public void MovePrevious() { if (Position > 0) Position--; }

    /// <summary>Raises the <see cref="CurrentChanged"/> event.</summary>
    protected virtual void OnCurrentChanged(EventArgs e) => CurrentChanged?.Invoke(this, e);

    /// <summary>Raises the <see cref="PositionChanged"/> event.</summary>
    protected virtual void OnPositionChanged(EventArgs e) => PositionChanged?.Invoke(this, e);

    /// <summary>Occurs when the currently bound item changes.</summary>
    public event EventHandler? CurrentChanged;

    /// <summary>Occurs when the <see cref="Position"/> changes.</summary>
    public event EventHandler? PositionChanged;

    /// <summary>Occurs when an item is added to the underlying list.</summary>
    public event EventHandler? ItemChanged;
}

/// <summary>
/// Read-only collection of <see cref="Binding"/> objects for a <see cref="BindingManagerBase"/>.
/// </summary>
public class BindingsCollection : System.Collections.ObjectModel.Collection<Binding>
{
    internal BindingsCollection() { }
}
