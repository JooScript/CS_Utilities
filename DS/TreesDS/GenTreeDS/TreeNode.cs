namespace Utils.DS.TreesDS.GenTreeDS;

public class TreeNode<T>
{
    public T Value { get; set; }
    public List<TreeNode<T>> Children { get; set; }

    public TreeNode(T value)
    {
        Value = value;
        Children = new List<TreeNode<T>>();
    }

    public void AddChild(TreeNode<T> child)
    {
        Children.Add(child);
    }

    public void RemoveChild(TreeNode<T> child)
    {
        Children.Remove(child);
    }

    public TreeNode<T>? Find(T value)
    {
        if (EqualityComparer<T>.Default.Equals(Value, value))
            return this;

        foreach (var child in Children)
        {
            var found = child.Find(value);
            if (found != null)
                return found;
        }

        return null;
    }

}


