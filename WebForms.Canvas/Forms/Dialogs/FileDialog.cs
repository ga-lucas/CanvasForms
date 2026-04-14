using Canvas.Windows.Forms;
using System.IO;

namespace System.Windows.Forms;

/// <summary>
/// Base class for file selection dialogs.
/// In the browser, filesystem calls are proxied to the host via Canvas.Windows.Forms.HostFileSystem.Current.
/// </summary>
public abstract class FileDialog : CommonDialog
{
    private string _fileName = string.Empty;

    public bool AddExtension { get; set; } = true;
    public bool CheckFileExists { get; set; }
    public bool CheckPathExists { get; set; } = true;
    public string DefaultExt { get; set; } = string.Empty;

    public string FileName
    {
        get => _fileName;
        set => _fileName = value ?? string.Empty;
    }

    public string[] FileNames { get; private set; } = Array.Empty<string>();

    public string Filter { get; set; } = string.Empty;

    /// <summary>
    /// 1-based filter index.
    /// </summary>
    public int FilterIndex { get; set; } = 1;

    public string InitialDirectory { get; set; } = string.Empty;
    public bool Multiselect { get; set; }
    public string Title { get; set; } = string.Empty;
    public bool ValidateNames { get; set; } = true;

    public bool RestoreDirectory { get; set; }

    public string SafeFileName => Path.GetFileName(FileName);
    public string[] SafeFileNames => FileNames.Select(Path.GetFileName).ToArray();

    public event CancelEventHandler? FileOk;

    protected virtual void OnFileOk(CancelEventArgs e)
    {
        FileOk?.Invoke(this, e);
    }

    public override void Reset()
    {
        FileName = string.Empty;
        FileNames = Array.Empty<string>();
        FilterIndex = 1;
    }

    protected sealed override DialogResult RunDialog(IWin32Window? owner)
    {
        // WASM can't reliably block without freezing rendering.
        // For compatibility with translated apps, callers should prefer ShowDialogAsync in browser mode.
        if (OperatingSystem.IsBrowser())
        {
            _ = ShowDialogAsync(owner);
            return DialogResult.None;
        }

        // Host execution: we can block.
        return ShowDialogAsync(owner).GetAwaiter().GetResult();
    }

    public Task<DialogResult> ShowDialogAsync() => ShowDialogAsync(owner: null);

    public Task<DialogResult> ShowDialogAsync(IWin32Window? owner)
    {
        var tcs = new TaskCompletionSource<DialogResult>(TaskCreationOptions.RunContinuationsAsynchronously);

        var originalDirectory = string.Empty;
        try { originalDirectory = Directory.GetCurrentDirectory(); } catch { }

        bool accepted = false;

        var form = CreateDialogForm(
            onAccept: selectedPaths =>
            {
                if (selectedPaths.Count == 0)
                {
                    return;
                }

                FileNames = selectedPaths.ToArray();
                FileName = selectedPaths[0];

                var args = new CancelEventArgs();
                OnFileOk(args);
                if (args.Cancel)
                {
                    return;
                }

                accepted = true;

                if (RestoreDirectory && !string.IsNullOrWhiteSpace(originalDirectory))
                {
                    try { Directory.SetCurrentDirectory(originalDirectory); } catch { }
                }

                tcs.TrySetResult(DialogResult.OK);
            },
            onCancel: () =>
            {
                if (RestoreDirectory && !string.IsNullOrWhiteSpace(originalDirectory))
                {
                    try { Directory.SetCurrentDirectory(originalDirectory); } catch { }
                }

                tcs.TrySetResult(DialogResult.Cancel);
            });

        form.FormClosed += (_, __) =>
        {
            if (!tcs.Task.IsCompleted)
            {
                if (RestoreDirectory && !string.IsNullOrWhiteSpace(originalDirectory))
                {
                    try { Directory.SetCurrentDirectory(originalDirectory); } catch { }
                }

                tcs.TrySetResult(DialogResult.Cancel);
            }
        };

        // Show on desktop.
        var manager = CanvasApplication.FormManager;
        if (manager != null)
        {
            manager.ShowForm(form);
        }
        else
        {
            form.Show();
        }

        _ = tcs.Task.ContinueWith(_ =>
        {
            if (accepted)
            {
                try { form.Close(); } catch { }
            }
        }, TaskScheduler.Default);

        return tcs.Task;
    }

    public Stream OpenFile()
    {
        if (string.IsNullOrWhiteSpace(FileName))
        {
            throw new InvalidOperationException("No file selected.");
        }

        if (OperatingSystem.IsBrowser())
        {
            var fs = HostFileSystem.Current ?? throw new InvalidOperationException("HostFileSystem.Current has not been configured.");
            return fs.OpenReadAsync(FileName).GetAwaiter().GetResult();
        }

        return File.OpenRead(FileName);
    }

    protected abstract string DefaultTitle { get; }

    protected virtual bool IsSaveDialog => false;

    public bool EnableUpload { get; set; } = Canvas.Windows.Forms.CanvasFormsOptions.EnableFileDialogUpload;

    private Form CreateDialogForm(Action<List<string>> onAccept, Action onCancel)
    {
        var dialog = new Form
        {
            Text = string.IsNullOrWhiteSpace(Title) ? DefaultTitle : Title,
            Width = 720,
            Height = 520,
            Left = 120,
            Top = 80,
        };

        var pathBox = new TextBox
        {
            Left = 12,
            Top = 12,
            Width = 560,
            Height = 26,
            Text = string.IsNullOrWhiteSpace(InitialDirectory) ? GetDefaultStartDirectory() : InitialDirectory,
        };

        var upButton = new Button
        {
            Text = "Up",
            Left = 580,
            Top = 10,
            Width = 52,
            Height = 28,
        };

        var refreshButton = new Button
        {
            Text = "↻",
            Left = 638,
            Top = 10,
            Width = 32,
            Height = 28,
        };

        var uploadButton = new Button
        {
            Text = "Upload",
            Left = 12,
            Top = 464,
            Width = 70,
            Height = 28,
            Visible = OperatingSystem.IsBrowser() && EnableUpload,
        };

        var list = new ListView
        {
            Left = 12,
            Top = 44,
            Width = 658,
            Height = 360,
            View = View.Details,
            FullRowSelect = true,
            GridLines = false,
            MultiSelect = Multiselect,
            Sorting = SortOrder.Ascending,
        };
        list.Columns.Add("Name", 460);
        list.Columns.Add("Type", 120);
        list.Columns.Add("Size", 70);

        var fileNameLabel = new Label
        {
            Text = "File name:",
            Left = 12,
            Top = 412,
            Width = 90,
            Height = 20,
        };

        var fileNameBox = new TextBox
        {
            Left = 100,
            Top = 408,
            Width = 570,
            Height = 26,
            Text = string.Empty,
        };

        var statusLabel = new Label
        {
            Left = 12,
            Top = 440,
            Width = 658,
            Height = 18,
            Text = string.Empty,
        };

        var okButton = new Button
        {
            Text = IsSaveDialog ? "Save" : "Open",
            Left = 540,
            Top = 464,
            Width = 60,
            Height = 28,
        };

        var cancelButton = new Button
        {
            Text = "Cancel",
            Left = 610,
            Top = 464,
            Width = 60,
            Height = 28,
        };

        dialog.Controls.Add(pathBox);
        dialog.Controls.Add(upButton);
        dialog.Controls.Add(refreshButton);
        dialog.Controls.Add(list);
        dialog.Controls.Add(fileNameLabel);
        dialog.Controls.Add(fileNameBox);
        dialog.Controls.Add(statusLabel);
        dialog.Controls.Add(uploadButton);
        dialog.Controls.Add(okButton);
        dialog.Controls.Add(cancelButton);

        string currentDirectory = pathBox.Text;

        void SetStatus(string message)
        {
            statusLabel.Text = message;
            dialog.Invalidate();
        }

        async Task<bool> DirectoryExists(string path)
        {
            if (OperatingSystem.IsBrowser())
            {
                var fs = HostFileSystem.Current;
                if (fs == null) return false;
                return await fs.DirectoryExistsAsync(path);
            }

            return Directory.Exists(path);
        }

        async Task<HostFileSystemEntry[]> List(string path)
        {
            if (OperatingSystem.IsBrowser())
            {
                var fs = HostFileSystem.Current;
                if (fs == null) return Array.Empty<HostFileSystemEntry>();
                return await fs.ListAsync(path);
            }

            try
            {
                var entries = new List<HostFileSystemEntry>();
                foreach (var d in Directory.EnumerateDirectories(path))
                {
                    entries.Add(new HostFileSystemEntry(Path.GetFileName(d), d, true, null));
                }

                foreach (var f in Directory.EnumerateFiles(path))
                {
                    if (!IsAllowedByFilter(f)) continue;
                    var fi = new FileInfo(f);
                    entries.Add(new HostFileSystemEntry(Path.GetFileName(f), f, false, fi.Length));
                }

                return entries.ToArray();
            }
            catch
            {
                return Array.Empty<HostFileSystemEntry>();
            }
        }

        void Populate(string dir)
        {
            _ = PopulateAsync(dir);
        }

        async Task PopulateAsync(string dir)
        {
            try
            {
                currentDirectory = dir;
                pathBox.Text = currentDirectory;

                list.Items.Clear();

                if (!await DirectoryExists(currentDirectory))
                {
                    SetStatus("Directory does not exist.");
                    return;
                }

                var entries = await List(currentDirectory);

                foreach (var e in entries.Where(e => e.IsDirectory).OrderBy(e => e.Name))
                {
                    var item = new ListViewItem(e.Name) { Tag = new Entry(e.FullPath, true) };
                    item.SubItems.Add("<DIR>");
                    item.SubItems.Add(string.Empty);
                    list.Items.Add(item);
                }

                foreach (var e in entries.Where(e => !e.IsDirectory).OrderBy(e => e.Name))
                {
                    var item = new ListViewItem(e.Name) { Tag = new Entry(e.FullPath, false) };
                    item.SubItems.Add(Path.GetExtension(e.Name));
                    item.SubItems.Add(e.Size?.ToString() ?? string.Empty);
                    list.Items.Add(item);
                }

                SetStatus(string.Empty);
            }
            catch (Exception ex)
            {
                SetStatus(ex.Message);
            }
        }

        void NavigateUp()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(currentDirectory))
                {
                    return;
                }

                var parent = Directory.GetParent(currentDirectory);
                if (parent == null)
                {
                    return;
                }

                Populate(parent.FullName);
            }
            catch (Exception ex)
            {
                SetStatus(ex.Message);
            }
        }

        Entry? GetSelectedEntry()
        {
            var selected = list.SelectedItems.FirstOrDefault();
            return selected?.Tag as Entry;
        }

        void AcceptSelection()
        {
            try
            {
                var selected = list.SelectedItems.ToList();
                var selectedEntries = selected.Select(i => i.Tag as Entry).Where(e => e != null).Cast<Entry>().ToList();

                if (selectedEntries.Count == 0)
                {
                    if (!string.IsNullOrWhiteSpace(fileNameBox.Text))
                    {
                        var typed = fileNameBox.Text.Trim();
                        var full = Path.IsPathRooted(typed) ? typed : Path.Combine(currentDirectory, typed);
                        selectedEntries.Add(new Entry(full, false));
                    }
                    else
                    {
                        SetStatus("Select a file.");
                        return;
                    }
                }

                if (selectedEntries.Count == 1 && selectedEntries[0].IsDirectory)
                {
                    Populate(selectedEntries[0].Path);
                    return;
                }

                var filePaths = selectedEntries.Where(e => !e.IsDirectory).Select(e => e.Path).ToList();
                if (filePaths.Count == 0)
                {
                    SetStatus("Select a file.");
                    return;
                }

                onAccept(filePaths);
            }
            catch (Exception ex)
            {
                SetStatus(ex.Message);
            }
        }

        void CancelSelection()
        {
            onCancel();
            dialog.Close();
        }

        upButton.Click += (_, __) => NavigateUp();
        refreshButton.Click += (_, __) => Populate(currentDirectory);

        uploadButton.Click += (_, __) =>
        {
            _ = UploadAsync();
        };

        list.SelectedIndexChanged += (_, __) =>
        {
            var entry = GetSelectedEntry();
            if (entry == null) return;
            if (!entry.IsDirectory)
            {
                fileNameBox.Text = Path.GetFileName(entry.Path);
            }
        };

        list.MouseDoubleClick += (_, __) => AcceptSelection();

        okButton.Click += (_, __) => AcceptSelection();
        cancelButton.Click += (_, __) => CancelSelection();

        pathBox.KeyDown += (_, e) =>
        {
            if (e.KeyCode == Keys.Enter)
            {
                var p = pathBox.Text.Trim();
                Populate(p);
            }
        };

        Populate(currentDirectory);

        return dialog;

        async Task UploadAsync()
        {
            try
            {
                SetStatus("Opening browser picker...");

                var uploader = HostFileSystem.Current as IHostFileUpload;
                if (uploader == null)
                {
                    SetStatus("Upload is not available.");
                    return;
                }

                var responseJson = await uploader.UploadFromBrowserAsync(Multiselect, GetAcceptForUpload());
                if (string.IsNullOrWhiteSpace(responseJson))
                {
                    SetStatus(string.Empty);
                    return;
                }

                // Minimal JSON parsing without extra dependencies.
                // Expected: { uploadId, directory, files:[{ name, fullPath, size }] }
                using var doc = System.Text.Json.JsonDocument.Parse(responseJson);
                if (!doc.RootElement.TryGetProperty("directory", out var dirProp))
                {
                    SetStatus("Upload response missing directory.");
                    return;
                }

                var dir = dirProp.GetString() ?? string.Empty;
                if (string.IsNullOrWhiteSpace(dir))
                {
                    SetStatus("Upload directory is empty.");
                    return;
                }

                currentDirectory = dir;
                pathBox.Text = currentDirectory;
                await PopulateAsync(currentDirectory);
                SetStatus("Upload complete.");
            }
            catch (Exception ex)
            {
                SetStatus(ex.Message);
            }
        }

        string GetAcceptForUpload()
        {
            if (string.IsNullOrWhiteSpace(Filter))
            {
                return string.Empty;
            }

            // Convert the selected filter patterns into an accept string.
            // We'll keep it simple: take extensions like *.png;*.jpg => .png,.jpg
            var patterns = GetSelectedFilterPatterns();
            var exts = new List<string>();
            foreach (var p in patterns)
            {
                var trimmed = p.Trim();
                if (trimmed.StartsWith("*.", StringComparison.Ordinal))
                {
                    exts.Add(trimmed[1..]); // ".png"
                }
            }

            return string.Join(',', exts.Distinct(StringComparer.OrdinalIgnoreCase));
        }
    }

    private static Task<string> UploadFromBrowserAsync(bool multiple, string accept)
    {
        // JSImport requires unsafe code + partial method/type.
        // This keeps WebForms.Canvas (the shared library) free of those constraints.
        throw new NotSupportedException("Upload is only supported in the browser host. Provide HostFileSystem upload integration from the host project.");
    }

    private string GetDefaultStartDirectory()
    {
        try
        {
            var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            if (!string.IsNullOrWhiteSpace(home))
            {
                return home;
            }
        }
        catch { }

        return ".";
    }

    private sealed record Entry(string Path, bool IsDirectory);

    private bool IsAllowedByFilter(string filePath)
    {
        if (string.IsNullOrWhiteSpace(Filter))
        {
            return true;
        }

        var patterns = GetSelectedFilterPatterns();
        if (patterns.Count == 0)
        {
            return true;
        }

        var fileName = Path.GetFileName(filePath);
        foreach (var p in patterns)
        {
            if (MatchesSimplePattern(fileName, p))
            {
                return true;
            }
        }

        return false;
    }

    private List<string> GetSelectedFilterPatterns()
    {
        var parts = Filter.Split('|');
        var patterns = new List<string>();
        if (parts.Length < 2)
        {
            return patterns;
        }

        var index = Math.Max(1, FilterIndex);
        var patternPartIndex = (index * 2) - 1;
        if (patternPartIndex < 0 || patternPartIndex >= parts.Length)
        {
            patternPartIndex = 1;
        }

        var raw = parts[patternPartIndex];
        foreach (var p in raw.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            patterns.Add(p);
        }

        return patterns;
    }

    private static bool MatchesSimplePattern(string fileName, string pattern)
    {
        int fi = 0;
        int pi = 0;
        int star = -1;
        int match = 0;

        while (fi < fileName.Length)
        {
            if (pi < pattern.Length && (pattern[pi] == '?' || char.ToLowerInvariant(pattern[pi]) == char.ToLowerInvariant(fileName[fi])))
            {
                pi++;
                fi++;
                continue;
            }

            if (pi < pattern.Length && pattern[pi] == '*')
            {
                star = pi++;
                match = fi;
                continue;
            }

            if (star != -1)
            {
                pi = star + 1;
                fi = ++match;
                continue;
            }

            return false;
        }

        while (pi < pattern.Length && pattern[pi] == '*')
        {
            pi++;
        }

        return pi == pattern.Length;
    }
}
