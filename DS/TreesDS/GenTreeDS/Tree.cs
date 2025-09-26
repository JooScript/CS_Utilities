namespace Utils.DS.TreesDS.GenTreeDS;

public class Tree<T>
{
    public TreeNode<T> Root { get; private set; }

    public Tree(T rootValue)
    {
        Root = new TreeNode<T>(rootValue);
    }

    public TreeNode<T> Find(T value)
    {
        return Root?.Find(value);
    }

}


