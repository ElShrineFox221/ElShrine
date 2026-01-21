using ElShrine.Common.DataStructure;

namespace ElShrine.Common.Interpreter
{
    public class StatementNode(int childrenLimit = -1) : ASTNode(childrenLimit), ITreeNode<StatementNode>
    {
        IReadOnlyList<StatementNode> ITreeNode<StatementNode>.Children => [.. Children.Select(x => (StatementNode)x)];
        StatementNode? ITreeNode<StatementNode>.Parent
        {
            get => Parent as StatementNode;
            set => Parent = value;
        }
        public StatementNode this[int index] => (StatementNode)Children[index];
        public override IASTResult Evaluate(IASTContext context)
        {
            foreach (var child in Children)
            {
                var r = child.Evaluate(context);
                if (r.Error is not null) return r;
            }
            return new ASTResult(null, this, null);
        }
    }
}
