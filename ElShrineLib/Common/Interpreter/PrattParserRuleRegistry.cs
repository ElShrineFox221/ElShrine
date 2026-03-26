using ElShrine.Common.DataStructure;

namespace ElShrine.Common.Interpreter
{
    public class PrattParserRuleRegistry : Registry<string, PrattParserRule>, IPrattParserRuleRegistry, ICloneable<PrattParserRuleRegistry>
    {
        public PrattParserRuleRegistry(bool registerDefaultRules = true)
        {
            if (registerDefaultRules) RegisterDefaultRules();
        }
        protected virtual void RegisterDefaultRules()
        {
            RegisterDefaultPrattParserRules();
        }

        public override object Clone()
        {
            var clone = new PrattParserRuleRegistry(false);
            foreach (var kv in Storage) clone.Storage.Add(kv.Key, kv.Value);
            return clone;
        }

        #region Pratt Default Rules
        protected void RegisterDefaultPrattParserRules()
        {
            var prattParserRules = new Dictionary<string, PrattParserRule>()
            {
                { TokenRegistry.TOKEN_END.RegName, new(0, null, null) },
                { TokenRegistry.TOKEN_STR.RegName, new(0, LiteralNud, null) },
                { TokenRegistry.TOKEN_NUM.RegName, new(0, LiteralNud, null) },
                { TokenRegistry.TOKEN_ID.RegName, new(0, VariableNud, null) },

                { TokenRegistry.TOKEN_LB.RegName, new(PRECEDENCE_MEMBER, GroupNud, CallLed) },
                { TokenRegistry.TOKEN_RB.RegName, new(0, null, null) },
                { TokenRegistry.TOKEN_COMMA.RegName, new(0, null, null) },
                { TokenRegistry.TOKEN_SPLIT.RegName, new(0, null, null) },
                { TokenRegistry.TOKEN_ASSIGN.RegName, new(PRECEDENCE_ASSIGN, null, AssignmentLed) },
                { TokenRegistry.TOKEN_DOT.RegName, new(PRECEDENCE_MEMBER, null, MemberLed) },

                { TokenRegistry.TOKEN_OPR_SUB.RegName, new(PRECEDENCE_SUM, UnaryNud, BinaryLed) },
                { TokenRegistry.TOKEN_OPR_ADD.RegName, new(PRECEDENCE_SUM, UnaryNud, BinaryLed) },
                { TokenRegistry.TOKEN_OPR_NOT.RegName, new(PRECEDENCE_UNARY, UnaryNud, null) },
                { TokenRegistry.TOKEN_OPR_MUL.RegName, new(PRECEDENCE_PRODUCT, null, BinaryLed) },
                { TokenRegistry.TOKEN_OPR_DIV.RegName, new(PRECEDENCE_PRODUCT, null, BinaryLed) },
                { TokenRegistry.TOKEN_OPR_MOD.RegName, new(PRECEDENCE_PRODUCT, null, BinaryLed) },
                { TokenRegistry.TOKEN_OPR_LS.RegName, new(PRECEDENCE_BIT_SHIFT, null, BinaryLed) },
                { TokenRegistry.TOKEN_OPR_RS.RegName, new(PRECEDENCE_BIT_SHIFT, null, BinaryLed) },

                { TokenRegistry.TOKEN_OPR_COND_EQ.RegName, new(PRECEDENCE_COMPARE, null, BinaryLed) },
                { TokenRegistry.TOKEN_OPR_COND_NEQ.RegName, new(PRECEDENCE_COMPARE, null, BinaryLed) },
                { TokenRegistry.TOKEN_OPR_COND_LT.RegName, new(PRECEDENCE_COMPARE, null, BinaryLed) },
                { TokenRegistry.TOKEN_OPR_COND_LTE.RegName, new(PRECEDENCE_COMPARE, null, BinaryLed) },
                { TokenRegistry.TOKEN_OPR_COND_GT.RegName, new(PRECEDENCE_COMPARE, null, BinaryLed) },
                { TokenRegistry.TOKEN_OPR_COND_GTE.RegName, new(PRECEDENCE_COMPARE, null, BinaryLed) },

                { TokenRegistry.TOKEN_OPR_XOR.RegName, new(45, null, BinaryLed) },
                { TokenRegistry.TOKEN_OPR_AND.RegName, new(46, null, BinaryLed) },
                { TokenRegistry.TOKEN_OPR_OR.RegName, new(44, null, BinaryLed) },
                { TokenRegistry.TOKEN_OPR_COND_AND.RegName, new(PRECEDENCE_COND_AND, null, BinaryLed) },
                { TokenRegistry.TOKEN_OPR_COND_OR.RegName, new(PRECEDENCE_COND_OR, null, BinaryLed) },

                { TokenRegistry.TOKEN_SW.RegName, new(0, null, null) },
                { TokenRegistry.TOKEN_SW_COND.RegName, new(0, null, null) },

                { TokenRegistry.TOKEN_SHARP.RegName, new(0, null, null) }
            };
            foreach (var item in prattParserRules) Register(item.Key, item.Value);
        }

        #region PrattParserRules

        #region Precedence
        private const int PRECEDENCE_ASSIGN = 10;
        private const int PRECEDENCE_COND_OR = 20;
        private const int PRECEDENCE_COND_AND = 30;
        private const int PRECEDENCE_COMPARE = 40;
        private const int PRECEDENCE_BIT_SHIFT = 50;
        private const int PRECEDENCE_SUM = 60;
        private const int PRECEDENCE_PRODUCT = 70;
        private const int PRECEDENCE_UNARY = 80;
        private const int PRECEDENCE_MEMBER = 90;
        private const int PRECEDENCE_MAX = 100;
        #endregion

        #region helper methods
        private int GetRBP(IToken token) => Items[token.RegName].RBP;
        private static T ConvertTo<T>(object? value)
        {
            if (value is T t) return t;
            T? result = (T?)Convert.ChangeType(value, typeof(T));
            return result ?? throw new InvalidCastException($"Cannot convert value({value?.GetType()} to type {typeof(T)})");
        }
        private static bool TryConvertTo<T>(object? value, out T? result)
        {
            if (value is T t)
            {
                result = (T?)t;
                return true;
            }
            try
            {
                result = (T?)Convert.ChangeType(value, typeof(T));
                return true;
            }
            catch
            {
                result = default;
                return false;
            }
        }
        #endregion

        #region nud & led methods
        private static UnaryExpressionNode UnaryNud(PrattParser prattParser, ValueStream<IToken> tokens, IToken token)
        {
            var right = prattParser.ParseExpression(tokens, PRECEDENCE_UNARY);
            UnaryCalculator? calculator = token.RegName switch
            {
                nameof(TokenRegistry.TOKEN_OPR_SUB) => para => -ConvertTo<double>(para),
                nameof(TokenRegistry.TOKEN_OPR_ADD) => para => para,
                nameof(TokenRegistry.TOKEN_OPR_NOT) => para => !ConvertTo<bool>(para),
                _ => null
            };
            var unary = new UnaryExpressionNode(token, calculator!);
            unary.Append(right);
            return unary;
        }
        private BinaryExpressionNode BinaryLed(PrattParser prattParser, ValueStream<IToken> tokens, IToken token, ExpressionNode left)
        {
            var rbp = GetRBP(token);
            var right = prattParser.ParseExpression(tokens, rbp);
            BinaryCalculator? calculator = token.RegName switch
            {
                nameof(TokenRegistry.TOKEN_OPR_SUB) => (para1, para2) => ConvertTo<double>(para1) - ConvertTo<double>(para2),
                nameof(TokenRegistry.TOKEN_OPR_ADD) => (para1, para2) => ConvertTo<double>(para1) + ConvertTo<double>(para2),
                nameof(TokenRegistry.TOKEN_OPR_MUL) => (para1, para2) => ConvertTo<double>(para1) * ConvertTo<double>(para2),
                nameof(TokenRegistry.TOKEN_OPR_DIV) => (para1, para2) => ConvertTo<double>(para1) / ConvertTo<double>(para2),
                _ => null,
            };
            var binary = new BinaryExpressionNode(token, calculator!);
            binary.Append(left!);
            binary.Append(right);
            return binary;
        }

        // 3. Assignment Led (赋值操作符: target = value)
        private AssignmentExpressionNode AssignmentLed(PrattParser prattParser, ValueStream<IToken> tokens, IToken token, ExpressionNode left)
        {
            // 赋值操作符通常是右结合，所以解析右侧时使用 rbp - 1 或 PRECEDENCE_ASSIGN
            // 为了实现右结合性，我们需要 `a = b = c` -> `a = (b = c)`
            // 这里使用 rbp - 1 是最标准的 Pratt 实现，但由于赋值优先级已经很低，使用 PRECEDENCE_ASSIGN 也可以
            var rbp = GetRBP(token);
            var right = prattParser.ParseExpression(tokens, rbp - 1); // 确保右侧的赋值操作符拥有更高的优先级
            var assignment = new AssignmentExpressionNode(token);
            assignment.Append(left!);
            assignment.Append(right);
            return assignment;
        }
        private static ConstantExpressionNode LiteralNud(PrattParser prattParser, ValueStream<IToken> tokens, IToken token)
            => new(token);
        private static VariableExpressionNode VariableNud(PrattParser prattParser, ValueStream<IToken> tokens, IToken token)
            => new(token);
        private static GroupExpressionNode GroupNud(PrattParser prattParser, ValueStream<IToken> tokens, IToken _)
        {
            var group = new GroupExpressionNode();
            if (tokens.Peek() == TokenRegistry.TOKEN_RB)
            {
                tokens.Consume();
                return group;
            }
            while (true)
            {
                var comp = prattParser.ParseExpression(tokens, 0);
                group.Append(comp);

                var nextT = tokens.Peek();
                if (nextT.Equals(TokenRegistry.TOKEN_RB))
                {
                    tokens.Consume();
                    break;
                }
                if (nextT.Equals(TokenRegistry.TOKEN_COMMA))
                {
                    tokens.Consume();
                    continue;
                }
                throw new ExpressionParseException($"Expected ')' or ',' after group expression, but found '{nextT}'");
            }
            return group;
        }
        private MemberCallExpressionNode MemberLed(PrattParser prattParser, ValueStream<IToken> tokens, IToken token, ExpressionNode left)
        {
            var memberIdToken = tokens.Consume();
            var memberCall = new MemberCallExpressionNode(memberIdToken);
            memberCall.Append(left!);
            // 不需要 AddChild(memberIdNode)，因为 memberIdToken 已经作为节点的一部分存储了
            return memberCall;
        }
        private FunctionCallExpressionNode CallLed(PrattParser prattParser, ValueStream<IToken> tokens, IToken token, ExpressionNode left)
        {
            if (left is not VariableExpressionNode varNode) throw new ExpressionParseException("Function call expression with a invaild id.");
            var functionCall = new FunctionCallExpressionNode(varNode.Token);
            if (tokens.Peek() == TokenRegistry.TOKEN_RB)
            {
                tokens.Consume();
                return functionCall;
            }
            while (true)
            {
                var arg = prattParser.ParseExpression(tokens, 0);
                functionCall.Append(arg);

                var nextT = tokens.Peek();
                if (nextT.Equals(TokenRegistry.TOKEN_RB))
                {
                    tokens.Consume();
                    break;
                }

                if (nextT.Equals(TokenRegistry.TOKEN_COMMA))
                {
                    tokens.Consume();
                    continue;
                }
                throw new ExpressionParseException($"Expected ')' or ',' after function argument, but found '{nextT}'");
            }
            return functionCall;
        }

        #endregion

        #endregion

        #endregion
    }
}
