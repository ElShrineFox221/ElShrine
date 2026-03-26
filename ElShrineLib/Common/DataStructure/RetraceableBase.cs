namespace ElShrine.Common.DataStructure
{
    public abstract class RetraceableCommand(bool isRetraceable)
    {
        public readonly bool IsRetraceable = isRetraceable;
        public virtual void Redo(string name) { }
        public virtual void Undo(string name) { }
    }
    public class NumIncreasingCommand(bool isDecreasing = false, double amount = 1d) : RetraceableCommand(true)
    {
        public readonly bool IsDecreasing = isDecreasing;
        public readonly double Amount = amount;
        public override void Redo(string name)
        {
            base.Redo(name);
        }
    }
    public abstract class RetraceableBase
    {
        protected record TraceRecord(RetraceableCommand Command, string Name);
        protected Stack<RetraceableCommand> TraceStack = [];

    }
}
