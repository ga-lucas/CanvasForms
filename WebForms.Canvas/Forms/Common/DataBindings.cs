using System.ComponentModel;
using System.Reflection;

namespace System.Windows.Forms;

// ── Binding ───────────────────────────────────────────────────────────────────

/// <summary>
/// Represents a simple data binding between a control property and a data source member.
/// Matches the WinForms <c>System.Windows.Forms.Binding</c> API.
///
/// When the data source implements <see cref="INotifyPropertyChanged"/> or is a
/// <see cref="BindingSource"/>, changes are pushed automatically to the control property.
/// </summary>
public class Binding
{
    private readonly string _propertyName;
    private readonly string _dataMember;
    private object? _dataSource;
    private Control? _control;

    // ── Construction ──────────────────────────────────────────────────────────

    public Binding(string propertyName, object? dataSource, string dataMember)
    {
        _propertyName = propertyName ?? string.Empty;
        _dataSource   = dataSource;
        _dataMember   = dataMember ?? string.Empty;
    }

    // ── Public API ────────────────────────────────────────────────────────────

    public string   PropertyName => _propertyName;
    public object?  DataSource   => _dataSource;
    public string   DataMember   => _dataMember;
    public Control? Control      => _control;

    public ControlUpdateMode ControlUpdateMode { get; set; } = ControlUpdateMode.OnPropertyChanged;
    public DataSourceUpdateMode DataSourceUpdateMode { get; set; } = DataSourceUpdateMode.OnValidation;
    public bool FormattingEnabled { get; set; } = false;
    public string FormatString    { get; set; } = string.Empty;
    public object? NullValue      { get; set; }

    public event BindingCompleteEventHandler? BindingComplete;
    public event ConvertEventHandler?         Format;
    public event ConvertEventHandler?         Parse;

    // ── Internal wiring ───────────────────────────────────────────────────────

    internal void Attach(Control control)
    {
        _control = control;
        Subscribe();
        PushToControl();
    }

    internal void Detach()
    {
        Unsubscribe();
        _control = null;
    }

    private void Subscribe()
    {
        if (_dataSource is BindingSource bs)
        {
            bs.CurrentChanged += OnSourceCurrentChanged;
        }
        else if (_dataSource is INotifyPropertyChanged inpc)
        {
            inpc.PropertyChanged += OnSourcePropertyChanged;
        }
    }

    private void Unsubscribe()
    {
        if (_dataSource is BindingSource bs)
        {
            bs.CurrentChanged -= OnSourceCurrentChanged;
        }
        else if (_dataSource is INotifyPropertyChanged inpc)
        {
            inpc.PropertyChanged -= OnSourcePropertyChanged;
        }
    }

    private void OnSourceCurrentChanged(object? sender, EventArgs e) => PushToControl();
    private void OnSourcePropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (string.IsNullOrEmpty(e.PropertyName) ||
            string.Equals(e.PropertyName, _dataMember, StringComparison.OrdinalIgnoreCase))
            PushToControl();
    }

    /// <summary>Reads the current data-source value and writes it to the bound control property.</summary>
    internal void PushToControl()
    {
        if (_control is null || string.IsNullOrEmpty(_propertyName)) return;

        try
        {
            var value = GetSourceValue();
            if (Format != null)
            {
                var args = new ConvertEventArgs(value, GetControlPropertyType());
                Format(this, args);
                value = args.Value;
            }

            SetControlProperty(value);
            BindingComplete?.Invoke(this, new BindingCompleteEventArgs(this, BindingCompleteContext.ControlUpdate));
        }
        catch
        {
            // Silently ignore binding errors — match WinForms permissive data-binding behaviour.
        }
    }

    /// <summary>Reads the bound control property value and writes it back to the data source.</summary>
    internal void PullFromControl()
    {
        if (_control is null || DataSourceUpdateMode == DataSourceUpdateMode.Never) return;

        try
        {
            var value = GetControlProperty();
            if (Parse != null)
            {
                var args = new ConvertEventArgs(value, GetSourceValueType());
                Parse(this, args);
                value = args.Value;
            }
            SetSourceValue(value);
            BindingComplete?.Invoke(this, new BindingCompleteEventArgs(this, BindingCompleteContext.DataSourceUpdate));
        }
        catch { }
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private object? GetSourceValue()
    {
        if (_dataSource is BindingSource bs)
        {
            var current = bs.Current;
            if (current is null) return null;
            return string.IsNullOrEmpty(_dataMember)
                ? current
                : ReadMember(current, _dataMember);
        }

        if (string.IsNullOrEmpty(_dataMember)) return _dataSource;
        return _dataSource is null ? null : ReadMember(_dataSource, _dataMember);
    }

    private void SetSourceValue(object? value)
    {
        if (_dataSource is BindingSource bs)
        {
            var current = bs.Current;
            if (current is null || string.IsNullOrEmpty(_dataMember)) return;
            WriteMember(current, _dataMember, value);
        }
        else if (_dataSource is not null && !string.IsNullOrEmpty(_dataMember))
        {
            WriteMember(_dataSource, _dataMember, value);
        }
    }

    private object? GetControlProperty()
    {
        if (_control is null) return null;
        var prop = _control.GetType().GetProperty(_propertyName,
            BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
        return prop?.GetValue(_control);
    }

    private void SetControlProperty(object? value)
    {
        if (_control is null) return;
        var prop = _control.GetType().GetProperty(_propertyName,
            BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
        if (prop is null || !prop.CanWrite) return;
        var converted = ConvertValue(value, prop.PropertyType);
        prop.SetValue(_control, converted);
    }

    private Type GetControlPropertyType()
    {
        if (_control is null) return typeof(object);
        return _control.GetType()
            .GetProperty(_propertyName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase)
            ?.PropertyType ?? typeof(object);
    }

    private Type GetSourceValueType()
    {
        var v = GetSourceValue();
        return v?.GetType() ?? typeof(object);
    }

    private static object? ReadMember(object obj, string member)
    {
        var prop = obj.GetType().GetProperty(member,
            BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
        return prop?.GetValue(obj);
    }

    private static void WriteMember(object obj, string member, object? value)
    {
        var prop = obj.GetType().GetProperty(member,
            BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
        if (prop is not null && prop.CanWrite)
            prop.SetValue(obj, ConvertValue(value, prop.PropertyType));
    }

    private static object? ConvertValue(object? value, Type targetType)
    {
        if (value is null)
            return targetType.IsValueType ? Activator.CreateInstance(targetType) : null;
        if (targetType.IsAssignableFrom(value.GetType()))
            return value;
        try { return Convert.ChangeType(value, Nullable.GetUnderlyingType(targetType) ?? targetType); }
        catch { return value; }
    }
}

// ── ControlBindingsCollection ─────────────────────────────────────────────────

/// <summary>
/// Collection of <see cref="Binding"/> objects attached to a <see cref="Control"/>.
/// Matches the WinForms <c>ControlBindingsCollection</c> API.
/// </summary>
public class ControlBindingsCollection : System.Collections.ObjectModel.Collection<Binding>
{
    private readonly Control _owner;

    internal ControlBindingsCollection(Control owner)
    {
        _owner = owner;
    }

    /// <summary>
    /// Adds a new <see cref="Binding"/> for the specified property, data source, and member.
    /// </summary>
    public Binding Add(string propertyName, object? dataSource, string dataMember)
    {
        var b = new Binding(propertyName, dataSource, dataMember);
        Add(b);
        return b;
    }

    /// <summary>
    /// Adds a new <see cref="Binding"/> with formatting options.
    /// </summary>
    public Binding Add(string propertyName, object? dataSource, string dataMember,
                       bool formattingEnabled)
    {
        var b = new Binding(propertyName, dataSource, dataMember) { FormattingEnabled = formattingEnabled };
        Add(b);
        return b;
    }

    /// <summary>
    /// Adds a new <see cref="Binding"/> with full formatting and update options.
    /// </summary>
    public Binding Add(string propertyName, object? dataSource, string dataMember,
                       bool formattingEnabled, DataSourceUpdateMode updateMode)
    {
        var b = new Binding(propertyName, dataSource, dataMember)
        {
            FormattingEnabled  = formattingEnabled,
            DataSourceUpdateMode = updateMode,
        };
        Add(b);
        return b;
    }

    protected override void InsertItem(int index, Binding item)
    {
        base.InsertItem(index, item);
        item.Attach(_owner);
    }

    protected override void RemoveItem(int index)
    {
        this[index].Detach();
        base.RemoveItem(index);
    }

    protected override void ClearItems()
    {
        foreach (var b in this) b.Detach();
        base.ClearItems();
    }
}

// ── BindingContext ────────────────────────────────────────────────────────────

/// <summary>
/// Stub for <c>System.Windows.Forms.BindingContext</c>.
/// Accepted for API compatibility — the canvas host uses direct <see cref="Binding"/>
/// push/pull without a separate currency manager layer.
/// </summary>
public class BindingContext : System.Collections.Hashtable { }

public enum ControlUpdateMode   { OnPropertyChanged = 0, Never = 1 }
public enum DataSourceUpdateMode { OnValidation = 0, OnPropertyChanged = 1, Never = 2 }
public enum BindingCompleteState { Success = 0, DataError = 1, Exception = 2 }

public delegate void ConvertEventHandler(object? sender, ConvertEventArgs e);

public sealed class ConvertEventArgs : EventArgs
{
    public object? Value        { get; set; }
    public Type    DesiredType  { get; }

    public ConvertEventArgs(object? value, Type desiredType)
    {
        Value       = value;
        DesiredType = desiredType;
    }
}
