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
            SelectedPath = @"C:\x",
            Description  = "test",
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
            Color        = System.Drawing.Color.Red,
            AnyColor     = true,
            AllowFullOpen = false,
            FullOpen     = true,
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
            Font           = new Canvas.Windows.Forms.Drawing.Font("Courier New", 18f),
            ShowEffects    = false,
            ShowColor      = true,
            MinSize        = 6,
            MaxSize        = 72,
        };
        d.Reset();
        Assert.Equal("Arial", d.Font.Family);
        Assert.Equal(12f,     d.Font.Size);
        Assert.True(d.ShowEffects);
        Assert.False(d.ShowColor);
        Assert.Equal(1,  d.MinSize);
        Assert.Equal(0,  d.MaxSize);
    }

    [Fact]
    public void Apply_EventCanBeSubscribed()
    {
        var d     = new FontDialog();
        bool fired = false;
        d.Apply  += (_, __) => fired = true;
        // Just verify subscription doesn't throw; firing requires dialog UI
        Assert.False(fired);
    }
}

// ════════════════════════════════════════════════════════════════════════════════
// StatusStrip + ToolStripStatusLabel
// ════════════════════════════════════════════════════════════════════════════════
public class StatusStripTests
{
    // ── StatusStrip defaults ──────────────────────────────────────────────────

    [Fact]
    public void StatusStrip_DefaultDock_IsBottom()
        => Assert.Equal(DockStyle.Bottom, new StatusStrip().Dock);

    [Fact]
    public void StatusStrip_DefaultGripStyle_IsHidden()
        => Assert.Equal(ToolStripGripStyle.Hidden, new StatusStrip().GripStyle);

    [Fact]
    public void StatusStrip_DefaultStretch_IsTrue()
        => Assert.True(new StatusStrip().Stretch);

    [Fact]
    public void StatusStrip_DefaultSizingGrip_IsTrue()
        => Assert.True(new StatusStrip().SizingGrip);

    [Fact]
    public void StatusStrip_SizingGrip_RoundTrips()
    {
        var s = new StatusStrip { SizingGrip = false };
        Assert.False(s.SizingGrip);
    }

    [Fact]
    public void StatusStrip_Items_StartsEmpty()
        => Assert.Empty(new StatusStrip().Items);

    [Fact]
    public void StatusStrip_CanAddStatusLabel()
    {
        var s   = new StatusStrip();
        var lbl = new ToolStripStatusLabel { Text = "Ready" };
        s.Items.Add(lbl);
        Assert.Single(s.Items);
        Assert.Equal("Ready", ((ToolStripStatusLabel)s.Items[0]).Text);
    }

    [Fact]
    public void StatusStrip_CreateDefaultItem_ReturnsStatusLabel()
    {
        var s    = new StatusStrip();
        var item = s.CreateDefaultItem("Hello", null, null);
        Assert.IsType<ToolStripStatusLabel>(item);
        Assert.Equal("Hello", item.Text);
    }

    [Fact]
    public void StatusStrip_CreateDefaultItem_Separator_ReturnsSeparator()
    {
        var s    = new StatusStrip();
        var item = s.CreateDefaultItem("-", null, null);
        Assert.IsType<ToolStripSeparator>(item);
    }

    // ── ToolStripStatusLabel defaults ─────────────────────────────────────────

    [Fact]
    public void StatusLabel_DefaultSpring_IsFalse()
        => Assert.False(new ToolStripStatusLabel().Spring);

    [Fact]
    public void StatusLabel_DefaultBorderSides_IsNone()
        => Assert.Equal(ToolStripStatusLabelBorderSides.None, new ToolStripStatusLabel().BorderSides);

    [Fact]
    public void StatusLabel_DefaultBorderStyle_IsFlat()
        => Assert.Equal(Border3DStyle.Flat, new ToolStripStatusLabel().BorderStyle);

    [Fact]
    public void StatusLabel_DefaultLiveSetting_IsOff()
        => Assert.Equal(LiveSetting.Off, new ToolStripStatusLabel().LiveSetting);

    // ── ToolStripStatusLabel property round-trips ─────────────────────────────

    [Fact]
    public void StatusLabel_Spring_RoundTrips()
    {
        var l = new ToolStripStatusLabel { Spring = true };
        Assert.True(l.Spring);
    }

    [Fact]
    public void StatusLabel_BorderSides_RoundTrips()
    {
        var l = new ToolStripStatusLabel
            { BorderSides = ToolStripStatusLabelBorderSides.Left | ToolStripStatusLabelBorderSides.Right };
        Assert.Equal(ToolStripStatusLabelBorderSides.Left | ToolStripStatusLabelBorderSides.Right, l.BorderSides);
    }

    [Fact]
    public void StatusLabel_BorderStyle_RoundTrips()
    {
        var l = new ToolStripStatusLabel { BorderStyle = Border3DStyle.Sunken };
        Assert.Equal(Border3DStyle.Sunken, l.BorderStyle);
    }

    [Fact]
    public void StatusLabel_LiveSetting_RoundTrips()
    {
        var l = new ToolStripStatusLabel { LiveSetting = LiveSetting.Polite };
        Assert.Equal(LiveSetting.Polite, l.LiveSetting);
    }

    [Fact]
    public void StatusLabel_Text_RoundTrips()
    {
        var l = new ToolStripStatusLabel("Status: OK");
        Assert.Equal("Status: OK", l.Text);
    }

    // ── ToolStripStatusLabel constructors ─────────────────────────────────────

    [Fact]
    public void StatusLabel_DefaultCtor_TextIsEmpty()
        => Assert.Equal(string.Empty, new ToolStripStatusLabel().Text);

    [Fact]
    public void StatusLabel_TextCtor_SetsText()
        => Assert.Equal("Ready", new ToolStripStatusLabel("Ready").Text);

    [Fact]
    public void StatusLabel_TextImageCtor_SetsTextAndImage()
    {
        var img = new Canvas.Windows.Forms.Drawing.Image { Source = "/img.png" };
        var l   = new ToolStripStatusLabel("Ready", img);
        Assert.Equal("Ready", l.Text);
        Assert.Same(img, l.Image);
    }

    [Fact]
    public void StatusLabel_ClickCtor_WiresHandler()
    {
        bool clicked = false;
        var l = new ToolStripStatusLabel("x", null, (_, __) => clicked = true);
        l.PerformClick();
        Assert.True(clicked);
    }

    [Fact]
    public void StatusLabel_NameCtor_SetsName()
    {
        var l = new ToolStripStatusLabel("x", null, null, "myLabel");
        Assert.Equal("myLabel", l.Name);
    }

    // ── ToolStripStatusLabelBorderSides flags ─────────────────────────────────

    [Fact]
    public void BorderSides_All_ContainsAllSides()
    {
        const ToolStripStatusLabelBorderSides all = ToolStripStatusLabelBorderSides.All;
        Assert.True((all & ToolStripStatusLabelBorderSides.Left)   != 0);
        Assert.True((all & ToolStripStatusLabelBorderSides.Top)    != 0);
        Assert.True((all & ToolStripStatusLabelBorderSides.Right)  != 0);
        Assert.True((all & ToolStripStatusLabelBorderSides.Bottom) != 0);
    }

    // ── Border3DStyle enum presence ───────────────────────────────────────────

    [Fact]
    public void Border3DStyle_HasExpectedValues()
    {
        // These values must exist for WinForms API compat
        _ = Border3DStyle.Flat;
        _ = Border3DStyle.Raised;
        _ = Border3DStyle.RaisedInner;
        _ = Border3DStyle.RaisedOuter;
        _ = Border3DStyle.Sunken;
        _ = Border3DStyle.SunkenInner;
        _ = Border3DStyle.SunkenOuter;
        _ = Border3DStyle.Etched;
        _ = Border3DStyle.Bump;
    }
}
