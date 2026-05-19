using System.Windows.Forms;

namespace Canvas.Windows.Forms.Tests;

// ════════════════════════════════════════════════════════════════════════════════
// Button — ImageList / ImageIndex / ImageKey / EffectiveImage via reflection
// ════════════════════════════════════════════════════════════════════════════════
public class ButtonImageListTests
{
    // ── Helpers ───────────────────────────────────────────────────────────────

    /// <summary>Reads the protected EffectiveImage property via reflection.</summary>
    private static Canvas.Windows.Forms.Drawing.Image? GetEffectiveImage(Button btn)
    {
        var prop = typeof(Button).GetProperty(
            "EffectiveImage",
            System.Reflection.BindingFlags.Instance |
            System.Reflection.BindingFlags.NonPublic |
            System.Reflection.BindingFlags.Public);
        Assert.NotNull(prop);
        return (Canvas.Windows.Forms.Drawing.Image?)prop!.GetValue(btn);
    }

    private static ImageList MakeImageList(params string[] urls)
    {
        var il = new ImageList { ImageSize = new System.Drawing.Size(16, 16) };
        foreach (var url in urls)
            il.Images.Add(url);
        return il;
    }

    // ── Image property takes precedence ──────────────────────────────────────

    [Fact]
    public void EffectiveImage_ReturnsDirectImage_WhenImageIsSet()
    {
        var img = new Canvas.Windows.Forms.Drawing.Image { Source = "icon.png", Width = 16, Height = 16 };
        var btn = new Button { Image = img };
        Assert.Same(img, GetEffectiveImage(btn));
    }

    // ── Null when nothing is set ──────────────────────────────────────────────

    [Fact]
    public void EffectiveImage_ReturnsNull_WhenNothingSet()
    {
        var btn = new Button();
        Assert.Null(GetEffectiveImage(btn));
    }

    [Fact]
    public void EffectiveImage_ReturnsNull_WhenImageListSetButIndexNegative()
    {
        var btn = new Button
        {
            ImageList  = MakeImageList("a.png", "b.png"),
            ImageIndex = -1    // default
        };
        Assert.Null(GetEffectiveImage(btn));
    }

    // ── ImageIndex ────────────────────────────────────────────────────────────

    [Fact]
    public void EffectiveImage_ResolvesFromImageIndex()
    {
        var il  = MakeImageList("first.png", "second.png");
        var btn = new Button { ImageList = il, ImageIndex = 1 };
        var eff = GetEffectiveImage(btn);
        Assert.NotNull(eff);
        Assert.Equal("second.png", eff!.Source);
    }

    [Fact]
    public void EffectiveImage_ImageIndex_UsesImageListImageSize()
    {
        var il  = new ImageList { ImageSize = new System.Drawing.Size(32, 32) };
        il.Images.Add("icon.png");
        var btn = new Button { ImageList = il, ImageIndex = 0 };
        var eff = GetEffectiveImage(btn);
        Assert.NotNull(eff);
        Assert.Equal(32, eff!.Width);
        Assert.Equal(32, eff!.Height);
    }

    [Fact]
    public void EffectiveImage_ReturnsNull_WhenImageIndexOutOfRange()
    {
        var il  = MakeImageList("only.png");
        var btn = new Button { ImageList = il, ImageIndex = 5 };
        Assert.Null(GetEffectiveImage(btn));
    }

    // ── ImageKey ──────────────────────────────────────────────────────────────

    [Fact]
    public void EffectiveImage_ResolvesFromImageKey()
    {
        var il = new ImageList { ImageSize = new System.Drawing.Size(16, 16) };
        il.Images.Add("save.png", "save");
        il.Images.Add("open.png", "open");
        var btn = new Button { ImageList = il, ImageKey = "open" };
        var eff = GetEffectiveImage(btn);
        Assert.NotNull(eff);
        Assert.Equal("open.png", eff!.Source);
    }

    [Fact]
    public void EffectiveImage_ImageKey_IsCaseInsensitive()
    {
        var il = new ImageList { ImageSize = new System.Drawing.Size(16, 16) };
        il.Images.Add("icon.png", "MyIcon");
        var btn = new Button { ImageList = il, ImageKey = "myicon" };
        var eff = GetEffectiveImage(btn);
        Assert.NotNull(eff);
        Assert.Equal("icon.png", eff!.Source);
    }

    [Fact]
    public void EffectiveImage_ReturnsNull_WhenImageKeyNotFound()
    {
        var il  = MakeImageList("a.png");
        var btn = new Button { ImageList = il, ImageKey = "missing" };
        Assert.Null(GetEffectiveImage(btn));
    }

    // ── Precedence: Image > ImageKey > ImageIndex ─────────────────────────────

    [Fact]
    public void EffectiveImage_DirectImage_WinsOverImageKey()
    {
        var il  = new ImageList { ImageSize = new System.Drawing.Size(16, 16) };
        il.Images.Add("list.png", "key");
        var direct = new Canvas.Windows.Forms.Drawing.Image { Source = "direct.png" };
        var btn = new Button { Image = direct, ImageList = il, ImageKey = "key" };
        Assert.Same(direct, GetEffectiveImage(btn));
    }

    [Fact]
    public void EffectiveImage_ImageKey_WinsOverImageIndex()
    {
        var il = new ImageList { ImageSize = new System.Drawing.Size(16, 16) };
        il.Images.Add("idx0.png", "first");
        il.Images.Add("idx1.png", "second");
        // ImageKey = "second" should win over ImageIndex = 0
        var btn = new Button { ImageList = il, ImageKey = "second", ImageIndex = 0 };
        var eff = GetEffectiveImage(btn);
        Assert.NotNull(eff);
        Assert.Equal("idx1.png", eff!.Source);
    }

    // ── Property defaults ─────────────────────────────────────────────────────

    [Fact]
    public void ImageIndex_DefaultsToNegativeOne()
    {
        var btn = new Button();
        Assert.Equal(-1, btn.ImageIndex);
    }

    [Fact]
    public void ImageKey_DefaultsToNullOrEmpty()
    {
        var btn = new Button();
        Assert.True(btn.ImageKey == null || btn.ImageKey == string.Empty);
    }

    [Fact]
    public void ImageList_DefaultsToNull()
    {
        var btn = new Button();
        Assert.Null(btn.ImageList);
    }

    [Fact]
    public void TextImageRelation_DefaultsToOverlay()
    {
        var btn = new Button();
        Assert.Equal(TextImageRelation.Overlay, btn.TextImageRelation);
    }

    // ── TextImageRelation round-trips ─────────────────────────────────────────

    [Theory]
    [InlineData(TextImageRelation.ImageBeforeText)]
    [InlineData(TextImageRelation.TextBeforeImage)]
    [InlineData(TextImageRelation.ImageAboveText)]
    [InlineData(TextImageRelation.TextAboveImage)]
    [InlineData(TextImageRelation.Overlay)]
    public void TextImageRelation_RoundTrips(TextImageRelation relation)
    {
        var btn = new Button { TextImageRelation = relation };
        Assert.Equal(relation, btn.TextImageRelation);
    }
}
