using ElShrine.Modules;

namespace ElShrine.Common.Interpreter
{
    #region Abstract
    public abstract class ExpressionNodeWithToken(IToken token, int childrenLimit) : ExpressionNode(childrenLimit)
    {
        public readonly IToken Token = token;
    }
    #endregion

    #region Calculation
    public abstract class CalculationExpressionNode(IToken token, int childrenLimit) : ExpressionNodeWithToken(token, childrenLimit);
    public class UnaryExpressionNode(IToken token, UnaryCalculator calculator) : CalculationExpressionNode(token, 1)
    {
        public readonly UnaryCalculator Calculator = calculator;
        public ExpressionNode Child => this[0];
        public override object? Access(IASTContext context) => Calculator(Child.Access(context));
    }
    public class BinaryExpressionNode(IToken token, BinaryCalculator calculator) : CalculationExpressionNode(token, 2)
    {
        public readonly BinaryCalculator Calculator = calculator;
        public ExpressionNode Left => this[0];
        public ExpressionNode Right => this[1];
        public override object? Access(IASTContext context) => Calculator(Left.Access(context), Right.Access(context));
    }
    public class TernaryExpressionNode(IToken token, TernaryCalculator calculator) : CalculationExpressionNode(token, 3)
    {
        public readonly TernaryCalculator Calculator = calculator;
        public ExpressionNode Left => this[0];
        public ExpressionNode Middle => this[1];
        public ExpressionNode Right => this[2];
        public override object? Access(IASTContext context) => Calculator(Left.Access(context), Middle.Access(context), Right.Access(context));
    }
    #endregion

    #region Structure
    public class AssignmentExpressionNode(IToken assignToken) : ExpressionNodeWithToken(assignToken, 2)
    {
        public override object? Access(IASTContext context)
        {
            var right = this[1].Access(context);
            var result = this[0].Desinate(context, right);
            return result;
        }
    }
    public class GroupExpressionNode() : ExpressionNode(-1)
    {
        public override object? Access(IASTContext context)
        {
            var length = Children.Count;
            var items = new object?[length];
            for (int i = 0; i < length; i++)
            {
                items[i] = this[i].Access(context);
            }
            return items;
        }
    }

    public class FunctionCallExpressionNode(IToken idToken) : ExpressionNodeWithToken(idToken, -1)
    {
        public override object? Access(IASTContext context)
        {
            var func = context.GetMethodInfo(Token.ToString()!);
            var result = func.Invoke(null, [.. Children.Select(c => (c as ExpressionNode)?.Access(context))]);
            return result;
        }
    }
    public class MemberCallExpressionNode(IToken memberIdToken) : ExpressionNodeWithToken(memberIdToken, 1)
    {
        public override object? Access(IASTContext context)
        {
            var memberOwner = this[0].Access(context);
            var member = memberOwner?.GetType().GetMember(Token.ToString()!)[0];
            var value = member?.GetMemberValue(memberOwner);
            return value;
        }
        public override object? Desinate(IASTContext context, object? value)
        {
            var memberOwner = this[0].Access(context);
            var member = memberOwner?.GetType().GetMember(Token.ToString()!)[0];
            member?.SetMemberValue(memberOwner, value);
            return value;
        }
    }
    public class IndexerCallExpressionNode() : ExpressionNode(2)
    {
        public override object? Access(IASTContext context)
        {
            var indexerOwner = this[0].Access(context);
            var index = Children.TakeLast(Children.Count - 1).Select(c => (c as ExpressionNode)?.Access(context)).ToArray();
            var indexerParams = index;
            var indexer = indexerOwner?.GetType().GetProperties().Where(p => p.GetIndexParameters().SequenceEqual(index)).FirstOrDefault();
            var value = indexer?.GetValue(indexerOwner, [.. indexerParams]);
            return value;
        }
        public override object? Desinate(IASTContext context, object? value)
        {
            var indexerOwner = this[0].Access(context);
            var index = Children.TakeLast(Children.Count - 1).Select(c => (c as ExpressionNode)?.Access(context)).ToArray();
            var indexerParams = index;
            var indexer = indexerOwner?.GetType().GetProperties().Where(p => p.GetIndexParameters().SequenceEqual(index)).FirstOrDefault();
            indexer?.SetValue(indexerOwner, value, [.. indexerParams]);
            return value;
        }
    }

    public class ConstantExpressionNode(IToken token) : ExpressionNodeWithToken(token, 0)
    {
        private readonly static Dictionary<string, object?> constantKeywordValues = new(StringComparer.OrdinalIgnoreCase)
        {
            ["true"] = true,
            ["false"] = false,
            ["null"] = null
        };
        public override object? Access(IASTContext context)
        {
            if(Token is not TOKEN_ID id || constantKeywordValues.ContainsKey(id.Name))
            {
                var r = Token switch
                {
                    TOKEN_NUM t_number => t_number.Number,
                    TOKEN_STR t_str => t_str.Content,
                    TOKEN_ID t_id => constantKeywordValues[t_id.Name],
                    _ => null
                };
                return r;
            }
            return context.GetConstantValue(id.Name);
        }
    }
    public class VariableExpressionNode(IToken token) : ExpressionNodeWithToken(token, 0)
    {
        public override object? Access(IASTContext context) 
            => context.GetVariableValue(Token.ToString()!);
        public override object? Desinate(IASTContext context, object? value) 
            => context.SetVariableValue(Token.ToString()!, value); 
    }
    #endregion
}
