namespace ElShrine.Common.DataStructure
{
    public interface ITreeNode<TNode> where TNode : ITreeNode<TNode>
    {   
        IReadOnlyList<TNode> Children { get; }
        TNode? Parent { get; set; }
    }
}