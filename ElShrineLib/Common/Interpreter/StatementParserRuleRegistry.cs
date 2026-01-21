using ElShrine.Common.DataStructure;

namespace ElShrine.Common.Interpreter
{
    public class StatementParserRuleRegistry : Registry<string, StatementParserRule>, IStatementParserRuleRegistry, ICloneable<StatementParserRuleRegistry>
    {
        public StatementParserRuleRegistry(bool registerDefaultRules = true)
        {
            if (registerDefaultRules) RegisterDefaultRules();
        }
        protected virtual void RegisterDefaultRules()
        {
            RegisterDefaultStatementParserRules();
        }

        public override object Clone()
        {
            var clone = new StatementParserRuleRegistry(false);
            foreach (var kv in Storage) clone.Storage.Add(kv.Key, kv.Value);
            return clone;
        }

        protected void RegisterDefaultStatementParserRules()
        {
            Register(nameof(IfStatementNode), new(IfStatementNode.IfPattern));
            Register(nameof(ExpressionStatementNode), new(ExpressionStatementNode.ExprPattern));
        }
    }
}
