using Canvas.Windows.Forms;
using System.IO;

namespace System.Windows.Forms;

/// <summary>
/// Prompts the user to select a folder.
/// Matches the WinForms <c>FolderBrowserDialog</c> API surface.
/// Uses the same host FS infrastructure as <see cref="FileDialog"/>.
/// </summary>
public class FolderBrowserDialog : CommonDialog
{
    // ── WinForms-compatible properties ────────────────────────────────────────

    /// <summary>Gets or sets the path selected by the user.</summary>
    public string SelectedPath { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the descriptive text displayed above the tree view in the dialog.
    /// Matches WinForms <c>FolderBrowserDialog.Description</c>.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>Gets or sets the root folder where browsing starts.</summary>
    public Environment.SpecialFolder RootFolder { get; set; } = Environment.SpecialFolder.Desktop;

    /// <summary>
    /// Gets or sets whether the New Folder button appears in the dialog.
    /// Matches WinForms <c>FolderBrowserDialog.ShowNewFolderButton</c>.
    /// </summary>
    public bool ShowNewFolderButton { get; set; } = true;

    /// <summary>
    /// Gets or sets the initial directory shown when the dialog opens.
    /// Matches WinForms <c>FolderBrowserDialog.InitialDirectory</c>.
    /// </summary>
    public string InitialDirectory { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets whether the dialog uses the Vista-style (IFileOpenDialog) picker.
    /// Stub — always uses the canvas tree view.
    /// </summary>
    public bool UseDescriptionForTitle { get; set; } = false;

    // ── CommonDialog overrides ────────────────────────────────────────────────

    public override void Reset()
    {
        SelectedPath        = string.Empty;
        Description         = string.Empty;
        RootFolder          = Environment.SpecialFolder.Desktop;
        ShowNewFolderButton = true;
        InitialDirectory    = string.Empty;
    }

    protected sealed override DialogResult RunDialog(IWin32Window? owner)
    {
        if (OperatingSystem.IsBrowser())
        {
            _ = ShowDialogAsync(owner);
            return DialogResult.None;
        }
        return ShowDialogAsync(owner).GetAwaiter().GetResult();
    }

    public Task<DialogResult> ShowDialogAsync() => ShowDialogAsync(null);

    public Task<DialogResult> ShowDialogAsync(IWin32Window? owner)
    {
        var tcs = new TaskCompletionSource<DialogResult>(TaskCreationOptions.RunContinuationsAsynchronously);
        bool accepted = false;

        var form = BuildForm(
            onAccept: path =>
            {
                SelectedPath = path;
                accepted = true;
                tcs.TrySetResult(DialogResult.OK);
            },
            onCancel: () => tcs.TrySetResult(DialogResult.Cancel));

        form.FormClosed += (_, __) =>
        {
            if (!tcs.Task.IsCompleted)
                tcs.TrySetResult(DialogResult.Cancel);
        };

        var manager = CanvasApplication.FormManager;
        if (manager != null) manager.ShowForm(form);
        else form.Show();

        _ = tcs.Task.ContinueWith(_ =>
        {
            if (accepted) try { form.Close(); } catch { }
        }, TaskScheduler.Default);

        return tcs.Task;
    }

    // ── Dialog UI ─────────────────────────────────────────────────────────────

    private Form BuildForm(Action<string> onAccept, Action onCancel)
    {
        const int W   = 420;
        const int H   = 480;
        const int Pad = 12;

        var dialog = new Form
        {
            Text          = UseDescriptionForTitle && !string.IsNullOrWhiteSpace(Description)
                            ? Description : "Browse For Folder",
            Width         = W,
            Height        = H,
            Left          = 140,
            Top           = 80,
            AllowResize   = true,
            MinimumWidth  = 320,
            MinimumHeight = 360,
        };

        int innerW = W - Pad * 2 - 16;

        // Description label
        var descLabel = new Label
        {
            Text    = Description,
            Left    = Pad,
            Top     = Pad,
            Width   = innerW,
            Height  = string.IsNullOrWhiteSpace(Description) ? 0 : 30,
            Visible = !string.IsNullOrWhiteSpace(Description),
            Anchor  = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
        };

        int listTop = Pad + (descLabel.Visible ? 36 : 0);

        // Path display
        var pathLabel = new Label
        {
            Text   = "Folder:",
            Left   = Pad,
            Top    = listTop,
            Width  = 50,
            Height = 20,
            Anchor = AnchorStyles.Top | AnchorStyles.Left,
        };

        var pathBox = new TextBox
        {
            Left     = Pad + 54,
            Top      = listTop,
            Width    = innerW - 54,
            Height   = 24,
            ReadOnly = true,
            Anchor   = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
        };

        // Folder list
        var list = new ListView
        {
            Left          = Pad,
            Top           = listTop + 30,
            Width         = innerW,
            Height        = H - listTop - 110,
            View          = View.List,
            FullRowSelect = true,
            MultiSelect   = false,
            Anchor        = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom,
        };

        var upButton = new Button
        {
            Text   = "↑ Up",
            Left   = Pad,
            Top    = H - 72,
            Width  = 60,
            Height = 26,
            Anchor = AnchorStyles.Bottom | AnchorStyles.Left,
        };

        var newFolderButton = new Button
        {
            Text    = "+ New Folder",
            Left    = Pad + 66,
            Top     = H - 72,
            Width   = 100,
            Height  = 26,
            Visible = ShowNewFolderButton,
            Anchor  = AnchorStyles.Bottom | AnchorStyles.Left,
        };

        var statusLabel = new Label
        {
            Left   = Pad,
            Top    = H - 40,
            Width  = innerW - 172,
            Height = 20,
            Text   = string.Empty,
            Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
        };

        var okButton = new Button
        {
            Text   = "OK",
            Left   = Pad + innerW - 164,
            Top    = H - 44,
            Width  = 80,
            Height = 28,
            Anchor = AnchorStyles.Bottom | AnchorStyles.Right,
        };

        var cancelButton = new Button
        {
            Text   = "Cancel",
            Left   = Pad + innerW - 80,
            Top    = H - 44,
            Width  = 80,
            Height = 28,
            Anchor = AnchorStyles.Bottom | AnchorStyles.Right,
        };

        dialog.Controls.Add(descLabel);
        dialog.Controls.Add(pathLabel);
        dialog.Controls.Add(pathBox);
        dialog.Controls.Add(list);
        dialog.Controls.Add(upButton);
        dialog.Controls.Add(newFolderButton);
        dialog.Controls.Add(statusLabel);
        dialog.Controls.Add(okButton);
        dialog.Controls.Add(cancelButton);

        string currentDir = string.IsNullOrWhiteSpace(InitialDirectory)
            ? GetRootPath()
            : InitialDirectory;

        void SetStatus(string msg) { statusLabel.Text = msg; dialog.Invalidate(); }

        async Task PopulateAsync(string dir)
        {
            try
            {
                currentDir  = dir;
                pathBox.Text = currentDir;
                list.Items.Clear();

                IEnumerable<string> subdirs;
                if (OperatingSystem.IsBrowser())
                {
                    var fs = HostFileSystem.Current;
                    if (fs == null) return;
                    var entries = await fs.ListAsync(dir);
                    subdirs = entries.Where(e => e.IsDirectory).Select(e => e.FullPath).OrderBy(p => p);
                }
                else
                {
                    subdirs = Directory.EnumerateDirectories(dir).OrderBy(p => p);
                }

                foreach (var sub in subdirs)
                    list.Items.Add(new ListViewItem(Path.GetFileName(sub)) { Tag = sub });

                SetStatus(string.Empty);
            }
            catch (Exception ex) { SetStatus(ex.Message); }
        }

        void Populate(string dir) => _ = PopulateAsync(dir);

        list.SelectedIndexChanged += (_, __) =>
        {
            if (list.SelectedItems.FirstOrDefault()?.Tag is string sub)
                pathBox.Text = sub;
        };

        list.MouseDoubleClick += (_, __) =>
        {
            if (list.SelectedItems.FirstOrDefault()?.Tag is string sub)
                Populate(sub);
        };

        upButton.Click += (_, __) =>
        {
            var parent = Directory.GetParent(currentDir);
            if (parent != null) Populate(parent.FullName);
        };

        newFolderButton.Click += (_, __) =>
        {
            try
            {
                var name = $"New folder";
                var path = Path.Combine(currentDir, name);
                int n = 1;
                while (Directory.Exists(path)) path = Path.Combine(currentDir, $"{name} ({++n})");
                Directory.CreateDirectory(path);
                Populate(currentDir);
                pathBox.Text = path;
            }
            catch (Exception ex) { SetStatus(ex.Message); }
        };

        okButton.Click += (_, __) =>
        {
            var chosen = pathBox.Text.Trim();
            if (string.IsNullOrWhiteSpace(chosen)) { SetStatus("Select a folder."); return; }
            onAccept(chosen);
        };

        cancelButton.Click += (_, __) => { onCancel(); dialog.Close(); };

        Populate(currentDir);
        return dialog;
    }

    private string GetRootPath()
    {
        try
        {
            var path = Environment.GetFolderPath(RootFolder);
            if (!string.IsNullOrWhiteSpace(path)) return path;
        }
        catch { }
        return Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
    }
}
