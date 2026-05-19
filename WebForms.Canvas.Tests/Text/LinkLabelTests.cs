using System.Windows.Forms;
using Xunit;

namespace Canvas.Windows.Forms.Tests;

public class LinkLabelLinkTests
{
    [Fact]
    public void Link_Constructor_SetsStartAndLength()
    {
        var link = new LinkLabel.Link(3, 5);
        Assert.Equal(3, link.Start);
        Assert.Equal(5, link.Length);
    }

    [Fact]
    public void Link_Constructor_SetsLinkData()
    {
        var link = new LinkLabel.Link(0, 4, "https://example.com");
        Assert.Equal("https://example.com", link.LinkData);
    }

    [Fact]
    public void Link_Visited_DefaultIsFalse()
    {
        var link = new LinkLabel.Link();
        Assert.False(link.Visited);
    }

    [Fact]
    public void Link_Enabled_DefaultIsTrue()
    {
        var link = new LinkLabel.Link();
        Assert.True(link.Enabled);
    }

    [Fact]
    public void Link_Enabled_CanBeDisabled()
    {
        var link = new LinkLabel.Link();
        link.Enabled = false;
        Assert.False(link.Enabled);
    }
}

public class LinkLabelCollectionTests
{
    [Fact]
    public void Links_DefaultIsEmpty()
    {
        var ll = new LinkLabel();
        Assert.Equal(0, ll.Links.Count);
    }

    [Fact]
    public void Links_Add_ByStartAndLength_IncreasesCount()
    {
        var ll = new LinkLabel { Text = "Click here for info" };
        ll.Links.Add(6, 4); // "here"
        Assert.Equal(1, ll.Links.Count);
    }

    [Fact]
    public void Links_Add_WithLinkData_StoresData()
    {
        var ll = new LinkLabel { Text = "Click here" };
        var link = ll.Links.Add(6, 4, "http://example.com");
        Assert.Equal("http://example.com", link.LinkData);
    }

    [Fact]
    public void Links_Remove_DecreasesCount()
    {
        var ll = new LinkLabel { Text = "Click here" };
        var link = ll.Links.Add(6, 4);
        ll.Links.Remove(link);
        Assert.Equal(0, ll.Links.Count);
    }

    [Fact]
    public void Links_RemoveAt_DecreasesCount()
    {
        var ll = new LinkLabel { Text = "Click here" };
        ll.Links.Add(0, 5);
        ll.Links.RemoveAt(0);
        Assert.Equal(0, ll.Links.Count);
    }

    [Fact]
    public void Links_Clear_EmptiesCollection()
    {
        var ll = new LinkLabel { Text = "Click here for more info" };
        ll.Links.Add(0, 5);
        ll.Links.Add(10, 3);
        ll.Links.Clear();
        Assert.Equal(0, ll.Links.Count);
    }

    [Fact]
    public void Links_Add_MultipleLinks_AreIndexable()
    {
        var ll = new LinkLabel { Text = "Link1 and Link2" };
        ll.Links.Add(0, 5);
        ll.Links.Add(10, 5);
        Assert.Equal(2, ll.Links.Count);
        Assert.Equal(0, ll.Links[0].Start);
        Assert.Equal(10, ll.Links[1].Start);
    }
}

public class LinkLabelPropertyTests
{
    [Fact]
    public void LinkColor_DefaultIsBlue()
    {
        var ll = new LinkLabel();
        // WinForms default is Blue (0, 0, 255)
        Assert.Equal(Color.FromArgb(0, 0, 255), ll.LinkColor);
    }

    [Fact]
    public void LinkColor_RoundTrips()
    {
        var ll = new LinkLabel();
        ll.LinkColor = Color.FromArgb(255, 0, 0);
        Assert.Equal(Color.FromArgb(255, 0, 0), ll.LinkColor);
    }

    [Fact]
    public void VisitedLinkColor_RoundTrips()
    {
        var ll = new LinkLabel();
        ll.VisitedLinkColor = Color.FromArgb(128, 0, 128);
        Assert.Equal(Color.FromArgb(128, 0, 128), ll.VisitedLinkColor);
    }

    [Fact]
    public void ActiveLinkColor_RoundTrips()
    {
        var ll = new LinkLabel();
        ll.ActiveLinkColor = Color.FromArgb(0, 128, 0);
        Assert.Equal(Color.FromArgb(0, 128, 0), ll.ActiveLinkColor);
    }

    [Fact]
    public void LinkBehavior_DefaultIsSystemDefault()
    {
        var ll = new LinkLabel();
        Assert.Equal(LinkBehavior.SystemDefault, ll.LinkBehavior);
    }

    [Fact]
    public void LinkBehavior_RoundTrips()
    {
        var ll = new LinkLabel();
        ll.LinkBehavior = LinkBehavior.AlwaysUnderline;
        Assert.Equal(LinkBehavior.AlwaysUnderline, ll.LinkBehavior);
    }

    [Fact]
    public void LinkVisited_DefaultIsFalse()
    {
        var ll = new LinkLabel();
        Assert.False(ll.LinkVisited);
    }

    [Fact]
    public void LinkVisited_RoundTrips()
    {
        var ll = new LinkLabel();
        ll.LinkVisited = true;
        Assert.True(ll.LinkVisited);
    }
}

public class LinkLabelEventTests
{
    [Fact]
    public void LinkClicked_Event_IsSubscribable()
    {
        var ll = new LinkLabel { Text = "Click me" };
        ll.Links.Add(0, 5);
        bool fired = false;
        ll.LinkClicked += (_, _) => fired = true;
        // Simulate programmatic click — since we can't send mouse events,
        // just verify the event subscription doesn't throw
        Assert.False(fired); // not fired without actual click
    }
}
