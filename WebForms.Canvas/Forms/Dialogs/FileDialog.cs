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
        const int W      = 740;
        const int H      = 570;   // extra height so bottom row clears form chrome (~30px title bar)
        const int Pad    = 12;
        const int BtnH   = 28;
        const int BtnW   = 80;
        const int UploadW = 94;

        // innerW: leave equal Pad margin on both sides; no extra scrollbar fudge
        // (ListView scrollbar is internal to the control)
        int innerW = W - Pad * 2;  // 716

        var dialog = new Form
        {
            Text          = string.IsNullOrWhiteSpace(Title) ? DefaultTitle : Title,
            Width         = W,
            Height        = H,
            Left          = 100,
            Top           = 60,
            AllowResize   = true,
            MinimumWidth  = 500,
            MinimumHeight = 420,
        };

        // ── Row 0: path bar ──────────────────────────────────────────────────
        // refreshButton right-aligns flush with innerW; upButton sits to its left
        const int RefreshW = 28;
        const int UpW      = 46;
        const int BtnGap   = 4;

        var refreshButton = new Button
        {
            Text   = "↻",
            Left   = Pad + innerW - RefreshW,
            Top    = Pad,
            Width  = RefreshW,
            Height = 26,
            Anchor = AnchorStyles.Top | AnchorStyles.Right,
        };

        var upButton = new Button
        {
            Text   = "↑ Up",
            Left   = refreshButton.Left - BtnGap - UpW,
            Top    = Pad,
            Width  = UpW,
            Height = 26,
            Anchor = AnchorStyles.Top | AnchorStyles.Right,
        };

        var pathBox = new TextBox
        {
            Left   = Pad,
            Top    = Pad,
            Width  = upButton.Left - Pad - BtnGap,
            Height = 26,
            Text   = string.IsNullOrWhiteSpace(InitialDirectory) ? GetDefaultStartDirectory() : InitialDirectory,
            Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
        };

        // ── Bottom section: work upward from form bottom ─────────────────────
        // Row B (buttons + upload):  bottom edge = H - Pad - BtnH
        // Row S (status):            24px above button row
        // Row 3 (filter combo):      34px above status row
        // Row 2 (filename box):      34px above filter row
        // List fills remaining space
        int btnRowTop    = H - Pad - BtnH - 30;   // -30 accounts for form chrome
        int statusRowTop = btnRowTop - 24;
        int row3Top      = statusRowTop - 34;
        int row2Top      = row3Top - 34;
        int listTop      = Pad + 34;
        int listH        = row2Top - listTop - 8;

        // ── Row 1: file list ─────────────────────────────────────────────────
        var list = new ListView
        {
            Left          = Pad,
            Top           = listTop,
            Width         = innerW,
            Height        = listH,
            View          = View.Details,
            FullRowSelect = true,
            GridLines     = false,
            MultiSelect   = Multiselect,
            Sorting       = SortOrder.Ascending,
            Anchor        = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom,
        };
        list.Columns.Add("Name", (int)(innerW * 0.60));
        list.Columns.Add("Type", (int)(innerW * 0.20));
        list.Columns.Add("Size", (int)(innerW * 0.17));

        // ── Row 2: filename label + box ──────────────────────────────────────
        var fileNameLabel = new Label
        {
            Text   = "File name:",
            Left   = Pad,
            Top    = row2Top + 4,
            Width  = 80,
            Height = 20,
            Anchor = AnchorStyles.Bottom | AnchorStyles.Left,
        };

        var fileNameBox = new TextBox
        {
            Left   = Pad + 84,
            Top    = row2Top,
            Width  = innerW - 84,
            Height = 26,
            Text   = string.Empty,
            Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
        };

        // ── Row 3: filter label + combo ──────────────────────────────────────
        var filterLabel = new Label
        {
            Text   = "File type:",
            Left   = Pad,
            Top    = row3Top + 4,
            Width  = 80,
            Height = 20,
            Anchor = AnchorStyles.Bottom | AnchorStyles.Left,
        };

        var filterCombo = new ComboBox
        {
            Left          = Pad + 84,
            Top           = row3Top,
            Width         = innerW - 84,
            Height        = 26,
            DropDownStyle = ComboBoxStyle.DropDownList,
            Anchor        = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
        };

        // Populate filter combo
        var filterParts    = (Filter ?? string.Empty).Split('|');
        int filterPairCount = filterParts.Length / 2;
        for (int fi = 0; fi < filterPairCount; fi++)
            filterCombo.Items.Add(filterParts[fi * 2]);
        if (filterCombo.Items.Count > 0)
            filterCombo.SelectedIndex = Math.Max(0, Math.Min(FilterIndex - 1, filterCombo.Items.Count - 1));

        // ── Row S: status label (full width, own row above buttons) ──────────
        var statusLabel = new Label
        {
            Left   = Pad,
            Top    = statusRowTop,
            Width  = innerW,
            Height = 20,
            Text   = string.Empty,
            Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
        };

        // ── Row B: upload | (space) | Open  Cancel ───────────────────────────
        var uploadButton = new Button
        {
            Text    = "⬆ Upload",
            Left    = Pad,
            Top     = btnRowTop,
            Width   = UploadW,
            Height  = BtnH,
            Visible = OperatingSystem.IsBrowser() && EnableUpload,
            Anchor  = AnchorStyles.Bottom | AnchorStyles.Left,
        };

        var cancelButton = new Button
        {
            Text   = "Cancel",
            Left   = Pad + innerW - BtnW,
            Top    = btnRowTop,
            Width  = BtnW,
            Height = BtnH,
            Anchor = AnchorStyles.Bottom | AnchorStyles.Right,
        };

        var okButton = new Button
        {
            Text   = IsSaveDialog ? "Save" : "Open",
            Left   = cancelButton.Left - BtnGap - BtnW,
            Top    = btnRowTop,
            Width  = BtnW,
            Height = BtnH,
            Anchor = AnchorStyles.Bottom | AnchorStyles.Right,
        };

        dialog.Controls.Add(pathBox);
        dialog.Controls.Add(upButton);
        dialog.Controls.Add(refreshButton);
        dialog.Controls.Add(list);
        dialog.Controls.Add(fileNameLabel);
        dialog.Controls.Add(fileNameBox);
        dialog.Controls.Add(filterLabel);
        dialog.Controls.Add(filterCombo);
        dialog.Controls.Add(statusLabel);
        dialog.Controls.Add(uploadButton);
        dialog.Controls.Add(okButton);
        dialog.Controls.Add(cancelButton);

        string currentDirectory = pathBox.Text;

        // Sync FilterIndex when combo changes
        filterCombo.SelectedIndexChanged += (_, __) =>
        {
            FilterIndex = filterCombo.SelectedIndex + 1;
            _ = PopulateAsync(currentDirectory);
        };

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
                    entries.Add(new HostFileSystemEntry(Path.GetFileName(d), d, true, null));

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

        void Populate(string dir)  => _ = PopulateAsync(dir);

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
                    item.SubItems.Add(Path.GetExtension(e.Name).TrimStart('.').ToUpperInvariant());
                    item.SubItems.Add(FormatSize(e.Size));
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
                var parent = Directory.GetParent(currentDirectory);
                if (parent != null) Populate(parent.FullName);
            }
            catch (Exception ex) { SetStatus(ex.Message); }
        }

        Entry? GetSelectedEntry() => list.SelectedItems.FirstOrDefault()?.Tag as Entry;

        void AcceptSelection()
        {
            try
            {
                var selected      = list.SelectedItems.ToList();
                var selectedEntries = selected
                    .Select(i => i.Tag as Entry).Where(e => e != null).Cast<Entry>().ToList();

                if (selectedEntries.Count == 0)
                {
                    if (!string.IsNullOrWhiteSpace(fileNameBox.Text))
                    {
                        var typed = fileNameBox.Text.Trim();
                        var full  = Path.IsPathRooted(typed) ? typed : Path.Combine(currentDirectory, typed);
                        selectedEntries.Add(new Entry(full, false));
                    }
                    else { SetStatus("Select a file."); return; }
                }

                if (selectedEntries.Count == 1 && selectedEntries[0].IsDirectory)
                {
                    Populate(selectedEntries[0].Path);
                    return;
                }

                var filePaths = selectedEntries.Where(e => !e.IsDirectory).Select(e => e.Path).ToList();
                if (filePaths.Count == 0) { SetStatus("Select a file."); return; }

                onAccept(filePaths);
            }
            catch (Exception ex) { SetStatus(ex.Message); }
        }

        void CancelSelection() { onCancel(); dialog.Close(); }

        upButton.Click       += (_, __) => NavigateUp();
        refreshButton.Click  += (_, __) => Populate(currentDirectory);
        uploadButton.Click   += (_, __) => _ = UploadAsync();

        list.SelectedIndexChanged += (_, __) =>
        {
            var entry = GetSelectedEntry();
            if (entry is { IsDirectory: false })
                fileNameBox.Text = Path.GetFileName(entry.Path);
        };

        list.MouseDoubleClick += (_, __) => AcceptSelection();
        okButton.Click        += (_, __) => AcceptSelection();
        cancelButton.Click    += (_, __) => CancelSelection();

        pathBox.KeyDown += (_, e) =>
        {
            if (e.KeyCode == Keys.Enter) Populate(pathBox.Text.Trim());
        };

        fileNameBox.KeyDown += (_, e) =>
        {
            if (e.KeyCode == Keys.Enter) AcceptSelection();
        };

        Populate(currentDirectory);
        return dialog;

        async Task UploadAsync()
        {
            try
            {
                SetStatus("Opening browser picker...");
                var uploader = HostFileSystem.Current as IHostFileUpload;
                if (uploader == null) { SetStatus("Upload is not available."); return; }

                var responseJson = await uploader.UploadFromBrowserAsync(Multiselect, GetAcceptForUpload());
                if (string.IsNullOrWhiteSpace(responseJson)) { SetStatus(string.Empty); return; }

                using var doc = System.Text.Json.JsonDocument.Parse(responseJson);
                if (!doc.RootElement.TryGetProperty("directory", out var dirProp))
                { SetStatus("Upload response missing directory."); return; }

                var dir = dirProp.GetString() ?? string.Empty;
                if (string.IsNullOrWhiteSpace(dir)) { SetStatus("Upload directory is empty."); return; }

                currentDirectory = dir;
                pathBox.Text = currentDirectory;
                await PopulateAsync(currentDirectory);
                SetStatus("Upload complete.");
            }
            catch (Exception ex) { SetStatus(ex.Message); }
        }

        string GetAcceptForUpload()
        {
            if (string.IsNullOrWhiteSpace(Filter)) return string.Empty;
            var exts = GetSelectedFilterPatterns()
                .Select(p => p.Trim())
                .Where(p => p.StartsWith("*.", StringComparison.Ordinal))
                .Select(p => p[1..])
                .Distinct(StringComparer.OrdinalIgnoreCase);
            return string.Join(',', exts);
        }
    }

    private static string FormatSize(long? bytes)
    {
        if (bytes is null) return string.Empty;
        if (bytes < 1024)      return $"{bytes} B";
        if (bytes < 1024*1024) return $"{bytes/1024} KB";
        return $"{bytes/(1024*1024)} MB";
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
