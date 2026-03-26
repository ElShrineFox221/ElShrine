using ElShrine.Common.DataStructure;

namespace ElShrine.Common.Interpreter
{
    public abstract class ASTNode(int childrenLimit) : ITreeNode<ASTNode>
    {
        public ASTNode? Parent
        {
            get => field;
            set
            {
                if (field != value)
                {
                    var oldParent = field;
                    field = value;
                    oldParent?.nodes.Remove(this);
                    if (value is not null)
                    {
                        if (value.childrenLimit < 0 || value.childrenLimit > value.nodes.Count) value.nodes.Add(this);
                        else throw new InvalidOperationException($"Children limit exceeded, targetTree({value.GetType}) has {value.nodes.Count}/{value.childrenLimit} nodes.");
                    }
                }
            }
        }

        private readonly int childrenLimit = childrenLimit;
        protected readonly List<ASTNode> nodes = [];
        protected virtual int MinimumChildrenCount => childrenLimit;
        public IReadOnlyList<ASTNode> Children => nodes;

        protected void ThrowIfChildrenAreInvaild(int min = -1)
        {
            if (min < 0) min = MinimumChildrenCount;
            if (nodes.Count < min) throw new IndexOutOfRangeException($"Children count is less than {min}.");
        }
        public abstract IASTResult Evaluate(IASTContext context);
    }
}
