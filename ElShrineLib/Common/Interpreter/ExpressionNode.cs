using ElShrine.Common.DataStructure;

namespace ElShrine.Common.Interpreter
{
    public abstract class ExpressionNode(int childrenLimit) : ASTNode(childrenLimit), ITreeNode<ExpressionNode>
    {
        IReadOnlyList<ExpressionNode> ITreeNode<ExpressionNode>.Children => [.. Children.Select(x => (ExpressionNode)x)];
        ExpressionNode? ITreeNode<ExpressionNode>.Parent
        {
            get => Parent as ExpressionNode;
            set => Parent = value;
        }

        public ExpressionNode this[int index] => (ExpressionNode)Children[index];
        //Use getter safely, the exceptions will be caught
        public override IASTResult Evaluate(IASTContext context)
        {
            try
            {
                var resultV = Access(context);
                return new ASTResult(null, this, resultV);
            }
            catch(Exception e)
            {
                return new ASTResult(e, this, null);
            }
        }
        //Use getter, exceptions will be directly thrown without catch block
        public abstract object? Access(IASTContext context);
        //Use setter, exceptions will be directly thrown without catch block
        public virtual object? Desinate(IASTContext context, object? value) => throw new NotImplementedException();
    }
}
