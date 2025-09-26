namespace Utils.DS.TreesDS.BinTreeDS;

/// <summary>
/// Represents a generic binary tree data structure that supports 
/// level-order insertion and standard traversal operations.
/// </summary>
/// <typeparam name="T">The type of values stored in the tree nodes.</typeparam>
public class BinaryTree<T>
{
    /// <summary>
    /// Gets the root node of the binary tree.
    /// </summary>
    public BinaryTreeNode<T> Root { get; private set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="BinaryTree{T}"/> class with an empty tree.
    /// </summary>
    public BinaryTree()
    {
        Root = null;
    }

    /// <summary>
    /// Inserts a new value into the binary tree using level-order insertion.
    /// Level-order insertion fills the tree from left to right,
    /// ensuring each level is filled before creating a new level.
    /// </summary>
    /// <param name="value">The value to insert into the tree.</param>
    public void Insert(T value)
    {
        var newNode = new BinaryTreeNode<T>(value);

        if (Root == null)
        {
            Root = newNode;
            return;
        }

        Queue<BinaryTreeNode<T>> queue = new Queue<BinaryTreeNode<T>>();
        queue.Enqueue(Root);

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();

            if (current.Left == null)
            {
                current.Left = newNode;
                break;
            }
            else
            {
                queue.Enqueue(current.Left);
            }

            if (current.Right == null)
            {
                current.Right = newNode;
                break;
            }
            else
            {
                queue.Enqueue(current.Right);
            }
        }
    }

    /// <summary>
    /// Gets the list of values obtained from a PreOrder traversal.
    /// PreOrder traversal visits: Current → Left → Right.
    /// </summary>
    public List<T> PreOrder
    {
        get
        {
            PreOrderList.Clear();
            TraversePreOrder(Root);
            return PreOrderList;
        }
    }

    /// <summary>
    /// Gets the list of values obtained from a PostOrder traversal.
    /// PostOrder traversal visits: Left → Right → Current.
    /// </summary>
    public List<T> PostOrder
    {
        get
        {
            PostOrderList.Clear();
            TraversePostOrder(Root);
            return PostOrderList;
        }
    }

    /// <summary>
    /// Gets the list of values obtained from an InOrder traversal.
    /// InOrder traversal visits: Left → Current → Right.
    /// </summary>
    public List<T> InOrder
    {
        get
        {
            InOrderList.Clear();
            TraverseInOrder(Root);
            return InOrderList;
        }
    }

    #region Traversal Methods

    private readonly List<T> PreOrderList = new List<T>();
    private void TraversePreOrder(BinaryTreeNode<T> node)
    {
        if (node != null)
        {
            PreOrderList.Add(node.Value);
            TraversePreOrder(node.Left);
            TraversePreOrder(node.Right);
        }
    }

    private readonly List<T> PostOrderList = new List<T>();
    private void TraversePostOrder(BinaryTreeNode<T> node)
    {
        if (node != null)
        {
            TraversePostOrder(node.Left);
            TraversePostOrder(node.Right);
            PostOrderList.Add(node.Value);
        }
    }

    private readonly List<T> InOrderList = new List<T>();
    private void TraverseInOrder(BinaryTreeNode<T> node)
    {
        if (node != null)
        {
            TraverseInOrder(node.Left);
            InOrderList.Add(node.Value);
            TraverseInOrder(node.Right);
        }
    }

    #endregion
}
