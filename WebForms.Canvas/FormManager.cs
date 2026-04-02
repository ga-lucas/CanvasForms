using WebForms.Canvas.Forms;

namespace WebForms.Canvas;

/// <summary>
/// Manages the lifecycle of forms in the application
/// Handles creation, showing, hiding, and disposal of forms
/// </summary>
public class FormManager
{
    private readonly List<Form> _forms = new();
    private readonly Action? _onFormsChanged;
    private Form? _mainForm;
    private Form? _activeForm;

    public FormManager(Action? onFormsChanged = null)
    {
        _onFormsChanged = onFormsChanged;
        Application.FormManager = this;
    }

    /// <summary>
    /// Gets the collection of open forms
    /// </summary>
    public IReadOnlyList<Form> OpenForms => _forms.AsReadOnly();

    /// <summary>
    /// Gets the currently active form (the one with focus/on top)
    /// </summary>
    public Form? ActiveForm => _activeForm;

    /// <summary>
    /// Gets or sets the main form of the application
    /// </summary>
    public Form? MainForm
    {
        get => _mainForm;
        set
        {
            _mainForm = value;
            if (value != null)
            {
                // Ensure main form is always visible
                value.Visible = true;

                if (!_forms.Contains(value))
                {
                    _forms.Add(value);
                    SetupFormEvents(value);
                }
                NotifyChanged();
            }
        }
    }

    /// <summary>
    /// Shows a form and adds it to the managed collection
    /// </summary>
    public void ShowForm(Form form)
    {
        if (!_forms.Contains(form))
        {
            _forms.Add(form);
            SetupFormEvents(form);
        }

        // Call Show() to ensure PerformLayout is called for docking/anchoring
        form.Show();

        // Track the active form
        _activeForm = form;

        NotifyChanged();
    }

    /// <summary>
    /// Shows a form as a dialog (blocks until closed)
    /// Note: In web environment, this won't truly block but will bring form to front
    /// </summary>
    public DialogResult ShowDialog(Form form)
    {
        ShowForm(form);
        form.BringToFront();

        // In a web environment, we can't truly block
        // Return OK for now - could be enhanced with callbacks
        return DialogResult.OK;
    }

    /// <summary>
    /// Hides a form without removing it from the collection
    /// </summary>
    public void HideForm(Form form)
    {
        // Don't hide the main form
        if (form == _mainForm)
        {
            return;
        }

        form.Visible = false;
        NotifyChanged();
    }

    /// <summary>
    /// Closes a form and removes it from the collection
    /// </summary>
    public void CloseForm(Form form)
    {
        // Don't close the main form - just hide other forms
        if (form == _mainForm)
        {
            // Main form should never be closed this way, only hidden by user action
            return;
        }

        form.Visible = false;
        _forms.Remove(form);

        NotifyChanged();
    }

    /// <summary>
    /// Closes all forms
    /// </summary>
    public void CloseAll()
    {
        var formsToClose = _forms.ToList();
        foreach (var form in formsToClose)
        {
            form.Visible = false;
        }
        _forms.Clear();
        NotifyChanged();

        Application.OnApplicationExit();
    }

    /// <summary>
    /// Gets a form by its type
    /// </summary>
    public T? GetForm<T>() where T : Form
    {
        return _forms.OfType<T>().FirstOrDefault();
    }

    /// <summary>
    /// Gets or creates a form by its type (singleton pattern)
    /// </summary>
    public T GetOrCreateForm<T>() where T : Form, new()
    {
        var existing = GetForm<T>();
        if (existing != null)
        {
            return existing;
        }

        var form = new T();
        ShowForm(form);
        return form;
    }

    /// <summary>
    /// Shows or brings to front an existing form, or creates it if it doesn't exist
    /// </summary>
    public T ShowOrCreateForm<T>() where T : Form, new()
    {
        var form = GetOrCreateForm<T>();

        if (form.WindowState == FormWindowState.Minimized)
        {
            form.WindowState = FormWindowState.Normal;
        }

        // Call Show() to ensure PerformLayout is called for docking/anchoring
        form.Show();
        form.BringToFront();

        // Track the active form
        _activeForm = form;

        NotifyChanged();
        return form;
    }

    /// <summary>
    /// Activates a form (brings to front and sets as active)
    /// </summary>
    public void ActivateForm(Form form)
    {
        if (!_forms.Contains(form)) return;

        if (form.WindowState == FormWindowState.Minimized)
        {
            form.WindowState = FormWindowState.Normal;
        }

        form.BringToFront();
        _activeForm = form;
        NotifyChanged();
    }

    private void SetupFormEvents(Form form)
    {
        // Subscribe to FormClosed event
        form.FormClosed -= OnFormClosed;
        form.FormClosed += OnFormClosed;

        // Subscribe to Activated event to track active form when clicked
        form.Activated -= OnFormActivated;
        form.Activated += OnFormActivated;
    }

    private void OnFormActivated(object? sender, EventArgs e)
    {
        if (sender is Form form && _forms.Contains(form))
        {
            _activeForm = form;
            NotifyChanged();
        }
    }

    private void OnFormClosed(object? sender, EventArgs e)
    {
        if (sender is Form form)
        {
            CloseForm(form);
        }
    }

    private void NotifyChanged()
    {
        _onFormsChanged?.Invoke();
    }
}

/// <summary>
/// Specifies identifiers to indicate the return value of a dialog box
/// </summary>
public enum DialogResult
{
    None = 0,
    OK = 1,
    Cancel = 2,
    Abort = 3,
    Retry = 4,
    Ignore = 5,
    Yes = 6,
    No = 7
}
