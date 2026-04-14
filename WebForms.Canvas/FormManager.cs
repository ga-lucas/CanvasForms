using System.Windows.Forms;

namespace Canvas.Windows.Forms;

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
    private Form? _modalForm;

    /// <summary>
    /// Fired whenever a new form is added to the desktop for the first time.
    /// </summary>
    public event EventHandler<Form>? FormAdded;

    public FormManager(Action? onFormsChanged = null)
    {
        _onFormsChanged = onFormsChanged;
        Canvas.Windows.Forms.CanvasApplication.FormManager = this;
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
    /// Gets the currently modal form (if any). When non-null, only this form should receive input.
    /// </summary>
    public Form? ModalForm => _modalForm;

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
        // If a modal dialog is open, new forms should not steal focus unless they are the modal form.
        if (!_forms.Contains(form))
        {
            _forms.Add(form);
            SetupFormEvents(form);
            FormAdded?.Invoke(this, form);
        }

        // Call Show() to ensure PerformLayout is called for docking/anchoring
        form.Show();

        // Track the active form unless a modal form is currently shown.
        if (_modalForm == null || ReferenceEquals(_modalForm, form))
        {
            _activeForm = form;
        }

        NotifyChanged();
    }

    /// <summary>
    /// Shows a form as a dialog (blocks until closed)
    /// Note: In web environment, this won't truly block but will bring form to front
    /// </summary>
    public DialogResult ShowDialog(Form form)
    {
        // In the browser we can't block, but we can enforce modality by capturing input.
        var previousModal = _modalForm;
        _modalForm = form;

        void OnDialogClosed(object? sender, FormClosedEventArgs e)
        {
            form.FormClosed -= OnDialogClosed;
            if (ReferenceEquals(_modalForm, form))
            {
                _modalForm = previousModal;
            }
            NotifyChanged();
        }

        form.FormClosed += OnDialogClosed;

        ShowForm(form);
        form.BringToFront();

        // In a web environment, we can't truly block.
        return DialogResult.None;
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
    /// Closes a form and removes it from the collection.
    /// The form's FormClosing event can cancel this operation.
    /// </summary>
    public void CloseForm(Form form)
    {
        if (!_forms.Contains(form)) return;

        // Call the form's Close method which handles FormClosing/FormClosed events
        form.Close();

        // Note: The actual removal from _forms happens in OnFormClosed handler
        // if the close wasn't cancelled
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

        Canvas.Windows.Forms.CanvasApplication.OnApplicationExit();
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

        // Enforce modality.
        if (_modalForm != null && !ReferenceEquals(_modalForm, form))
        {
            _modalForm.BringToFront();
            _activeForm = _modalForm;
            NotifyChanged();
            return;
        }

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

    private void OnFormClosed(object? sender, FormClosedEventArgs e)
    {
        if (sender is Form form)
        {
            if (ReferenceEquals(_modalForm, form))
            {
                _modalForm = null;
            }

            // Remove from our list (form already handled the closing logic)
            _forms.Remove(form);

            // If closing the main form, clear the reference
            if (form == _mainForm)
            {
                _mainForm = null;
            }

            // If closing the active form, select another one
            if (form == _activeForm)
            {
                _activeForm = _forms.LastOrDefault();
            }

            NotifyChanged();
        }
    }

    private void NotifyChanged()
    {
        _onFormsChanged?.Invoke();
    }
}

