using System;
using System.Windows.Forms;
using Xunit;

namespace Canvas.Windows.Forms.Tests;

// ════════════════════════════════════════════════════════════════════════════════
// FileDialog (base — tested via OpenFileDialog / SaveFileDialog)
// ════════════════════════════════════════════════════════════════════════════════
public class FileDialogTests
{
    // ── Default property values ───────────────────────────────────────────────

    [Fact]
    public void OpenFileDialog_DefaultCheckFileExists_IsTrue()
        => Assert.True(new OpenFileDialog().CheckFileExists);

    [Fact]
    public void OpenFileDialog_DefaultMultiselect_IsFalse()
        => Assert.False(new OpenFileDialog().Multiselect);

    [Fact]
    public void OpenFileDialog_DefaultFileName_IsEmpty()
        => Assert.Equal(string.Empty, new OpenFileDialog().FileName);

    [Fact]
    public void OpenFileDialog_DefaultFileNames_IsEmpty()
        => Assert.Empty(new OpenFileDialog().FileNames);

    [Fact]
    public void OpenFileDialog_DefaultFilterIndex_Is1()
        => Assert.Equal(1, new OpenFileDialog().FilterIndex);

    [Fact]
    public void OpenFileDialog_DefaultTitle_IsEmpty()
        => Assert.Equal(string.Empty, new OpenFileDialog().Title);

    [Fact]
    public void OpenFileDialog_DefaultFilter_IsEmpty()
        => Assert.Equal(string.Empty, new OpenFileDialog().Filter);

    // ── Property round-trips ──────────────────────────────────────────────────

    [Fact]
    public void FileName_RoundTrips()
    {
        var d = new OpenFileDialog { FileName = @"C:\foo\bar.txt" };
        Assert.Equal(@"C:\foo\bar.txt", d.FileName);
    }

    [Fact]
    public void Filter_RoundTrips()
    {
        var d = new OpenFileDialog { Filter = "Text|*.txt|All|*.*" };
        Assert.Equal("Text|*.txt|All|*.*", d.Filter);
    }

    [Fact]
    public void FilterIndex_RoundTrips()
    {
        var d = new OpenFileDialog { FilterIndex = 2 };
        Assert.Equal(2, d.FilterIndex);
    }

    [Fact]
    public void Title_RoundTrips()
    {
        var d = new OpenFileDialog { Title = "Pick a file" };
        Assert.Equal("Pick a file", d.Title);
    }

    [Fact]
    public void Multiselect_RoundTrips()
    {
        var d = new OpenFileDialog { Multiselect = true };
        Assert.True(d.Multiselect);
    }

    [Fact]
    public void InitialDirectory_RoundTrips()
    {
        var d = new OpenFileDialog { InitialDirectory = @"C:\temp" };
        Assert.Equal(@"C:\temp", d.InitialDirectory);
    }

    [Fact]
    public void DefaultExt_RoundTrips()
    {
        var d = new OpenFileDialog { DefaultExt = "txt" };
        Assert.Equal("txt", d.DefaultExt);
    }

    [Fact]
    public void FileName_SetNull_TreatedAsEmpty()
    {
        var d = new OpenFileDialog { FileName = null! };
        Assert.Equal(string.Empty, d.FileName);
    }

    // ── SafeFileName ──────────────────────────────────────────────────────────

    [Fact]
    public void SafeFileName_ReturnsBareFilename()
    {
        var d = new OpenFileDialog { FileName = @"C:\some\path\report.xlsx" };
        Assert.Equal("report.xlsx", d.SafeFileName);
    }

    [Fact]
    public void SafeFileName_EmptyWhenNoFile()
        => Assert.Equal(string.Empty, new OpenFileDialog().SafeFileName);

    // ── Reset ─────────────────────────────────────────────────────────────────

    [Fact]
    public void Reset_ClearsFileName()
    {
        var d = new OpenFileDialog { FileName = "foo.txt" };
        d.Reset();
        Assert.Equal(string.Empty, d.FileName);
    }

    [Fact]
    public void Reset_ResetsFilterIndex()
    {
        var d = new OpenFileDialog { FilterIndex = 3 };
        d.Reset();
        Assert.Equal(1, d.FilterIndex);
    }

    [Fact]
    public void Reset_ClearsFileNames()
    {
        var d = new OpenFileDialog();
        d.Reset();
        Assert.Empty(d.FileNames);
    }
}

// ════════════════════════════════════════════════════════════════════════════════
// SaveFileDialog
// ════════════════════════════════════════════════════════════════════════════════
public class SaveFileDialogTests
{
    [Fact]
    public void DefaultOverwritePrompt_IsTrue()
        => Assert.True(new SaveFileDialog().OverwritePrompt);

    [Fact]
    public void DefaultCreatePrompt_IsFalse()
        => Assert.False(new SaveFileDialog().CreatePrompt);

    [Fact]
    public void DefaultCheckFileExists_IsFalse()
        => Assert.False(new SaveFileDialog().CheckFileExists);

    [Fact]
    public void OverwritePrompt_RoundTrips()
    {
        var d = new SaveFileDialog { OverwritePrompt = false };
        Assert.False(d.OverwritePrompt);
    }

    [Fact]
    public void CreatePrompt_RoundTrips()
    {
        var d = new SaveFileDialog { CreatePrompt = true };
        Assert.True(d.CreatePrompt);
    }

    [Fact]
    public void Filter_RoundTrips()
    {
        var d = new SaveFileDialog { Filter = "CSV|*.csv" };
        Assert.Equal("CSV|*.csv", d.Filter);
    }

    [Fact]
    public void Reset_RestoresDefaults()
    {
        var d = new SaveFileDialog { OverwritePrompt = false, CreatePrompt = true };
        d.Reset();
        Assert.True(d.OverwritePrompt);
        Assert.False(d.CreatePrompt);
    }
}

// ════════════════════════════════════════════════════════════════════════════════
// FolderBrowserDialog
// ════════════════════════════════════════════════════════════════════════════════
public class FolderBrowserDialogTests
{
    [Fact]
    public void DefaultSelectedPath_IsEmpty()
        => Assert.Equal(string.Empty, new FolderBrowserDialog().SelectedPath);

    [Fact]
    public void DefaultDescription_IsEmpty()
        => Assert.Equal(string.Empty, new FolderBrowserDialog().Description);

    [Fact]
    public void DefaultShowNewFolderButton_IsTrue()
        => Assert.True(new FolderBrowserDialog().ShowNewFolderButton);

    [Fact]
    public void DefaultRootFolder_IsDesktop()
        => Assert.Equal(Environment.SpecialFolder.Desktop, new FolderBrowserDialog().RootFolder);

    [Fact]
    public void SelectedPath_RoundTrips()
    {
        var d = new FolderBrowserDialog { SelectedPath = @"C:\projects" };
        Assert.Equal(@"C:\projects", d.SelectedPath);
    }

    [Fact]
    public void Description_RoundTrips()
    {
        var d = new FolderBrowserDialog { Description = "Choose output folder" };
        Assert.Equal("Choose output folder", d.Description);
    }

    [Fact]
    public void ShowNewFolderButton_RoundTrips()
    {
        var d = new FolderBrowserDialog { ShowNewFolderButton = false };
        Assert.False(d.ShowNewFolderButton);
    }

    [Fact]
    public void RootFolder_RoundTrips()
    {
        var d = new FolderBrowserDialog { RootFolder = Environment.SpecialFolder.MyDocuments };
        Assert.Equal(Environment.SpecialFolder.MyDocuments, d.RootFolder);
    }

    [Fact]
    public void InitialDirectory_RoundTrips()
    {
        var d = new FolderBrowserDialog { InitialDirectory = @"C:\temp" };
        Assert.Equal(@"C:\temp", d.InitialDirectory);
    }

    [Fact]
    public void Reset_RestoresDefaults()
    {
        var d = new FolderBrowserDialog
        {
            SelectedPath        = @"C:\x",
            Description         = "test",
            ShowNewFolderButton = false,
        };
        d.Reset();
        Assert.Equal(string.Empty, d.SelectedPath);
        Assert.Equal(string.Empty, d.Description);
        Assert.True(d.ShowNewFolderButton);
    }
}

// ════════════════════════════════════════════════════════════════════════════════
// ColorDialog
// ════════════════════════════════════════════════════════════════════════════════
public class ColorDialogTests
{
    [Fact]
    public void DefaultColor_IsBlack()
    {
        var d = new ColorDialog();
        Assert.Equal(System.Drawing.Color.Black, d.Color);
    }

    [Fact]
    public void DefaultAllowFullOpen_IsTrue()
        => Assert.True(new ColorDialog().AllowFullOpen);

    [Fact]
    public void DefaultAnyColor_IsFalse()
        => Assert.False(new ColorDialog().AnyColor);

    [Fact]
    public void DefaultSolidColorOnly_IsFalse()
        => Assert.False(new ColorDialog().SolidColorOnly);

    [Fact]
    public void DefaultFullOpen_IsFalse()
        => Assert.False(new ColorDialog().FullOpen);

    [Fact]
    public void DefaultCustomColors_IsNull()
        => Assert.Null(new ColorDialog().CustomColors);

    [Fact]
    public void Color_RoundTrips()
    {
        var c = System.Drawing.Color.FromArgb(128, 64, 32);
        var d = new ColorDialog { Color = c };
        Assert.Equal(c, d.Color);
    }

    [Fact]
    public void CustomColors_RoundTrips()
    {
        var colors = new[] { unchecked((int)0xFF_FF0000), unchecked((int)0xFF_00FF00) };
        var d = new ColorDialog { CustomColors = colors };
        Assert.Equal(colors, d.CustomColors);
    }

    [Fact]
    public void FullOpen_RoundTrips()
    {
        var d = new ColorDialog { FullOpen = true };
        Assert.True(d.FullOpen);
    }

    [Fact]
    public void Reset_RestoresDefaults()
    {
        var d = new ColorDialog
        {
            Color         = System.Drawing.Color.Red,
            AnyColor      = true,
            AllowFullOpen = false,
            FullOpen      = true,
        };
        d.Reset();
        Assert.Equal(System.Drawing.Color.Black, d.Color);
        Assert.False(d.AnyColor);
        Assert.True(d.AllowFullOpen);
        Assert.False(d.FullOpen);
    }
}

// ════════════════════════════════════════════════════════════════════════════════
// FontDialog
// ════════════════════════════════════════════════════════════════════════════════
public class FontDialogTests
{
    [Fact]
    public void DefaultFont_IsArial12()
    {
        var d = new FontDialog();
        Assert.Equal("Arial", d.Font.Family);
        Assert.Equal(12f, d.Font.Size);
    }

    [Fact]
    public void DefaultColor_IsBlack()
        => Assert.Equal(System.Drawing.Color.Black, new FontDialog().Color);

    [Fact]
    public void DefaultShowEffects_IsTrue()
        => Assert.True(new FontDialog().ShowEffects);

    [Fact]
    public void DefaultShowColor_IsFalse()
        => Assert.False(new FontDialog().ShowColor);

    [Fact]
    public void DefaultShowApplyButton_IsFalse()
        => Assert.False(new FontDialog().ShowApplyButton);

    [Fact]
    public void DefaultMinSize_Is1()
        => Assert.Equal(1, new FontDialog().MinSize);

    [Fact]
    public void DefaultMaxSize_IsZero()
        => Assert.Equal(0, new FontDialog().MaxSize);

    [Fact]
    public void Font_RoundTrips()
    {
        var f = new Canvas.Windows.Forms.Drawing.Font("Verdana", 14f);
        var d = new FontDialog { Font = f };
        Assert.Equal("Verdana", d.Font.Family);
        Assert.Equal(14f, d.Font.Size);
    }

    [Fact]
    public void Color_RoundTrips()
    {
        var c = System.Drawing.Color.Blue;
        var d = new FontDialog { Color = c };
        Assert.Equal(c, d.Color);
    }

    [Fact]
    public void ShowEffects_RoundTrips()
    {
        var d = new FontDialog { ShowEffects = false };
        Assert.False(d.ShowEffects);
    }

    [Fact]
    public void MinSize_MaxSize_RoundTrip()
    {
        var d = new FontDialog { MinSize = 8, MaxSize = 48 };
        Assert.Equal(8,  d.MinSize);
        Assert.Equal(48, d.MaxSize);
    }

    [Fact]
    public void Reset_RestoresDefaults()
    {
        var d = new FontDialog
        {
            Font        = new Canvas.Windows.Forms.Drawing.Font("Courier New", 18f),
            ShowEffects = false,
            ShowColor   = true,
            MinSize     = 6,
            MaxSize     = 72,
        };
        d.Reset();
        Assert.Equal("Arial", d.Font.Family);
        Assert.Equal(12f,     d.Font.Size);
        Assert.True(d.ShowEffects);
        Assert.False(d.ShowColor);
        Assert.Equal(1, d.MinSize);
        Assert.Equal(0, d.MaxSize);
    }

    [Fact]
    public void Apply_EventCanBeSubscribed()
    {
        var d      = new FontDialog();
        bool fired = false;
        d.Apply   += (_, __) => fired = true;
        Assert.False(fired);
    }
}
