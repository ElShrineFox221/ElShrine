using ElShrine.Common.DataStructure;

namespace ElShrine.Common.Interpreter
{
    public interface ITokenRegistry : IRegistry<string, IToken>
    {
        IReadOnlyDictionary<string, IToken> ProtoTokens => Items;
        bool Register(IToken protoToken) => Register(protoToken.RegName, protoToken);
    }
    public interface IStatementParserRuleRegistry : IRegistry<string, StatementParserRule>
    {
        IReadOnlyDictionary<string, StatementParserRule> Rules => Items;
    }
    public interface IPrattParserRuleRegistry : IRegistry<string, PrattParserRule>
    {
        IReadOnlyDictionary<string, PrattParserRule> Rules => Items;
    }
}
