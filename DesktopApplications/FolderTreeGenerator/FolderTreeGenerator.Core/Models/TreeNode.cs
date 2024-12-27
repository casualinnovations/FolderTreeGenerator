namespace FolderTreeGenerator.Core.Models;

public class TreeNode
{
    public required string Name { get; init; }
    public required string FullPath { get; init; }
    public bool IsFile { get; init; }
    public int Depth { get; init; }
    public List<TreeNode> Children { get; } = new();
    public DateTime LastModified { get; init; }
    public long? Size { get; init; }
    public bool IsLast { get; set; }
    public Dictionary<string, object> Metadata { get; init; } = new();

    public void AddChild(TreeNode child)
    {
        Children.Add(child);
        UpdateLastFlags();
    }

    public void AddChildren(IEnumerable<TreeNode> children)
    {
        Children.AddRange(children);
        UpdateLastFlags();
    }

    private void UpdateLastFlags()
    {
        if (Children.Count > 0)
        {
            // Clear all flags first
            foreach (var child in Children)
            {
                child.IsLast = false;
            }
            // Set last flag on the last child
            Children[^1].IsLast = true;
        }
    }
}