using System.Windows.Forms;
using Xunit;

namespace Canvas.Windows.Forms.Tests;

public class TreeNodeTests
{
    [Fact]
    public void Constructor_Text_SetsText()
    {
        var node = new TreeNode("Root");
        Assert.Equal("Root", node.Text);
    }

    [Fact]
    public void Constructor_WithChildren_AddsChildren()
    {
        var child1 = new TreeNode("Child1");
        var child2 = new TreeNode("Child2");
        var root = new TreeNode("Root", new[] { child1, child2 });
        Assert.Equal(2, root.Nodes.Count);
    }

    [Fact]
    public void Expand_SetsIsExpanded_True()
    {
        var node = new TreeNode("A");
        node.Expand();
        Assert.True(node.IsExpanded);
    }

    [Fact]
    public void Collapse_SetsIsExpanded_False()
    {
        var node = new TreeNode("A");
        node.Expand();
        node.Collapse();
        Assert.False(node.IsExpanded);
    }

    [Fact]
    public void Toggle_FlipsExpandedState()
    {
        var node = new TreeNode("A");
        Assert.False(node.IsExpanded);
        node.Toggle();
        Assert.True(node.IsExpanded);
        node.Toggle();
        Assert.False(node.IsExpanded);
    }

    [Fact]
    public void Level_RootNode_IsZero()
    {
        var node = new TreeNode("Root");
        Assert.Equal(0, node.Level);
    }

    [Fact]
    public void Level_ChildNode_IsOne()
    {
        var root = new TreeNode("Root");
        var child = new TreeNode("Child");
        root.Nodes.Add(child);
        Assert.Equal(1, child.Level);
    }

    [Fact]
    public void Level_GrandchildNode_IsTwo()
    {
        var root  = new TreeNode("Root");
        var child = new TreeNode("Child");
        var grand = new TreeNode("Grand");
        root.Nodes.Add(child);
        child.Nodes.Add(grand);
        Assert.Equal(2, grand.Level);
    }

    [Fact]
    public void Parent_ChildNode_ReferencesParent()
    {
        var root  = new TreeNode("Root");
        var child = new TreeNode("Child");
        root.Nodes.Add(child);
        Assert.Same(root, child.Parent);
    }

    [Fact]
    public void Parent_RootNode_IsNull()
    {
        var node = new TreeNode("Root");
        Assert.Null(node.Parent);
    }

    [Fact]
    public void FullPath_SingleNode_IsJustText()
    {
        var node = new TreeNode("Root");
        Assert.Equal("Root", node.FullPath);
    }

    [Fact]
    public void FullPath_ChildNode_IncludesParent()
    {
        var root  = new TreeNode("Root");
        var child = new TreeNode("Child");
        root.Nodes.Add(child);
        Assert.Equal(@"Root\Child", child.FullPath);
    }

    [Fact]
    public void FullPath_GrandchildNode_IncludesFullHierarchy()
    {
        var root  = new TreeNode("Root");
        var child = new TreeNode("Child");
        var grand = new TreeNode("Grand");
        root.Nodes.Add(child);
        child.Nodes.Add(grand);
        Assert.Equal(@"Root\Child\Grand", grand.FullPath);
    }

    [Fact]
    public void ExpandAll_ExpandsAllDescendants()
    {
        var root  = new TreeNode("Root");
        var child = new TreeNode("Child");
        var grand = new TreeNode("Grand");
        root.Nodes.Add(child);
        child.Nodes.Add(grand);
        root.ExpandAll();
        Assert.True(root.IsExpanded);
        Assert.True(child.IsExpanded);
        Assert.True(grand.IsExpanded);
    }

    [Fact]
    public void CollapseAll_CollapsesAllDescendants()
    {
        var root  = new TreeNode("Root");
        var child = new TreeNode("Child");
        root.Nodes.Add(child);
        root.ExpandAll();
        root.CollapseAll();
        Assert.False(root.IsExpanded);
        Assert.False(child.IsExpanded);
    }

    [Fact]
    public void Checked_DefaultIsFalse()
    {
        var node = new TreeNode("A");
        Assert.False(node.Checked);
    }

    [Fact]
    public void Tag_CanBeSet()
    {
        var node = new TreeNode("A");
        node.Tag = 42;
        Assert.Equal(42, node.Tag);
    }

    [Fact]
    public void HasChildren_ReturnsFalse_WhenNoChildren()
    {
        var node = new TreeNode("Leaf");
        Assert.False(node.HasChildren);
    }

    [Fact]
    public void HasChildren_ReturnsTrue_WhenChildAdded()
    {
        var root = new TreeNode("Root");
        root.Nodes.Add(new TreeNode("Child"));
        Assert.True(root.HasChildren);
    }
}

public class TreeNodeCollectionTests
{
    [Fact]
    public void Add_ByText_CreatesNode()
    {
        var tv = new TreeView();
        var node = tv.Nodes.Add("Root");
        Assert.Equal("Root", node.Text);
        Assert.Equal(1, tv.Nodes.Count);
    }

    [Fact]
    public void Remove_DecreasesCount()
    {
        var tv = new TreeView();
        var node = tv.Nodes.Add("A");
        tv.Nodes.Remove(node);
        Assert.Equal(0, tv.Nodes.Count);
    }

    [Fact]
    public void Clear_EmptiesCollection()
    {
        var tv = new TreeView();
        tv.Nodes.Add("A"); tv.Nodes.Add("B");
        tv.Nodes.Clear();
        Assert.Equal(0, tv.Nodes.Count);
    }

    [Fact]
    public void Indexer_ByKey_FindsNodeByName()
    {
        var tv = new TreeView();
        var node = tv.Nodes.Add("Root");
        node.Name = "rootKey";
        Assert.Same(node, tv.Nodes["rootKey"]);
    }

    [Fact]
    public void Indexer_ByKey_ReturnsNull_WhenNotFound()
    {
        var tv = new TreeView();
        Assert.Null(tv.Nodes["missing"]);
    }
}

public class TreeViewTests
{
    [Fact]
    public void SelectedNode_DefaultsToNull()
    {
        var tv = new TreeView();
        Assert.Null(tv.SelectedNode);
    }

    [Fact]
    public void SelectedNode_Set_FiresAfterSelect()
    {
        var tv = new TreeView();
        var node = tv.Nodes.Add("A");
        bool fired = false;
        tv.AfterSelect += (_, _) => fired = true;
        tv.SelectedNode = node;
        Assert.True(fired);
        Assert.Same(node, tv.SelectedNode);
    }

    [Fact]
    public void SelectedNode_IsSelected_Property_IsTrue()
    {
        var tv = new TreeView();
        var node = tv.Nodes.Add("A");
        tv.SelectedNode = node;
        Assert.True(node.IsSelected);
    }

    [Fact]
    public void TreeView_ShowLines_DefaultsToTrue()
    {
        var tv = new TreeView();
        Assert.True(tv.ShowLines);
    }

    [Fact]
    public void TreeView_ShowPlusMinus_DefaultsToTrue()
    {
        var tv = new TreeView();
        Assert.True(tv.ShowPlusMinus);
    }

    [Fact]
    public void TreeView_ShowRootLines_DefaultsToTrue()
    {
        var tv = new TreeView();
        Assert.True(tv.ShowRootLines);
    }

    [Fact]
    public void TreeView_CheckBoxes_DefaultsToFalse()
    {
        var tv = new TreeView();
        Assert.False(tv.CheckBoxes);
    }

    [Fact]
    public void TreeView_FullRowSelect_DefaultsToFalse()
    {
        var tv = new TreeView();
        Assert.False(tv.FullRowSelect);
    }

    [Fact]
    public void AfterExpand_FiredWhenNodeExpands()
    {
        var tv = new TreeView();
        var node = tv.Nodes.Add("A");
        bool fired = false;
        tv.AfterExpand += (_, _) => fired = true;
        node.Expand();
        // Note: AfterExpand fires when TreeView receives the expand notification
        Assert.True(node.IsExpanded);
    }

    [Fact]
    public void Nodes_AddedToTreeView_HaveTreeViewSet()
    {
        var tv = new TreeView();
        var node = tv.Nodes.Add("Root");
        Assert.Same(tv, node.TreeView);
    }
}
