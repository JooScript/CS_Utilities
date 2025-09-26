namespace Utils.DS.TreesDS.BinTreeDS;

public class BinaryTreeNode<T>
{
    public T Value { get; set; }
    public BinaryTreeNode<T> Left { get; set; }
    public BinaryTreeNode<T> Right { get; set; }

    public BinaryTreeNode(T value)
    {
        Value = value;
        Left = null;
        Right = null;
    }

}