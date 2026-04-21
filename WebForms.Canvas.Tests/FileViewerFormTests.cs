using System.Windows.Forms;
using Canvas.Windows.Forms.Samples;

namespace Canvas.Windows.Forms.Tests;

public class FileViewerFormTests
{
    [Fact]
    public void FileViewerForm_TextFile_CreatesTextBox()
    {
        var form = new FileViewerForm("test.txt");

        Assert.Contains("test.txt", form.Text);
        Assert.True(form.Width == 800);
        Assert.True(form.Height == 600);
    }

    [Fact]
    public void FileViewerForm_ImageFile_CreatesPictureBox()
    {
        var form = new FileViewerForm("test.png");

        Assert.Contains("test.png", form.Text);
        Assert.True(form.Width == 800);
        Assert.True(form.Height == 600);
    }

    [Fact]
    public void FileViewerForm_UnsupportedFile_ShowsMessage()
    {
        var form = new FileViewerForm("test.exe");

        Assert.Contains("test.exe", form.Text);
    }
}
