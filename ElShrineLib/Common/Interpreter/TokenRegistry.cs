using ElShrine.Common.DataStructure;
using System.Reflection;

namespace ElShrine.Common.Interpreter
{
    public class TokenRegistry : Registry<string, IToken>, ITokenRegistry, ICloneable<TokenRegistry>
    {
        public TokenRegistry(bool registerDefaultTokens = true)
        {
            if(registerDefaultTokens) RegisterDefaultTokens();
        }
        protected virtual void RegisterDefaultTokens()
        {
            var tokens = DefaultTokens;
            foreach (var token in tokens)
            {
                var r = (this as ITokenRegistry).Register(token);
                if (!r) throw new Exception($"Failed to register token: {token.RegName}");
            }
        }
        public override object Clone()
        {
            var clone = new TokenRegistry(false);
            foreach (var kv in Storage) clone.Storage.Add(kv.Key, kv.Value);
            return clone;
        }

        #region Default Tokens

        #region Tokens
        // None
        public static readonly TOKEN TOKEN_END = new(nameof(TOKEN_END));
        // SingleChar
        public static readonly TOKEN_MC TOKEN_LB = new(nameof(TOKEN_LB), "(");
        public static readonly TOKEN_MC TOKEN_RB = new(nameof(TOKEN_RB), ")");
        public static readonly TOKEN_MC TOKEN_LMB = new(nameof(TOKEN_LMB), "[");
        public static readonly TOKEN_MC TOKEN_RMB = new(nameof(TOKEN_RMB), "]");

        public static readonly TOKEN_MC TOKEN_LBB = new(nameof(TOKEN_LBB), "{");
        public static readonly TOKEN_MC TOKEN_RBB = new(nameof(TOKEN_RBB), "}");
        public static readonly TOKEN_BLOCK TOKEN_BLOCK = new(nameof(TOKEN_BLOCK));

        public static readonly TOKEN_MC TOKEN_COMMA = new(nameof(TOKEN_COMMA), ",");
        public static readonly TOKEN_MC TOKEN_DOT = new(nameof(TOKEN_DOT), ".");
        public static readonly TOKEN_MC TOKEN_SHARP = new(nameof(TOKEN_SHARP), "#");
        public static readonly TOKEN_MC TOKEN_SPLIT = new(nameof(TOKEN_SPLIT), ";");
        public static readonly TOKEN_MC TOKEN_ASSIGN = new(nameof(TOKEN_ASSIGN), "=");
        public static readonly TOKEN_MC TOKEN_DEDUCE = new(nameof(TOKEN_DEDUCE), "=>");
        // MultiChar

        //
        public static readonly TOKEN_OPR TOKEN_OPR_ADD = new(nameof(TOKEN_OPR_ADD), "+");
        public static readonly TOKEN_OPR TOKEN_OPR_SUB = new(nameof(TOKEN_OPR_SUB), "-");
        public static readonly TOKEN_OPR TOKEN_OPR_MUL = new(nameof(TOKEN_OPR_MUL), "*");
        public static readonly TOKEN_OPR TOKEN_OPR_DIV = new(nameof(TOKEN_OPR_DIV), "/");
        public static readonly TOKEN_OPR TOKEN_OPR_MOD = new(nameof(TOKEN_OPR_MOD), "%");
        public static readonly TOKEN_OPR TOKEN_OPR_XOR = new(nameof(TOKEN_OPR_XOR), "^");
        public static readonly TOKEN_OPR TOKEN_OPR_AND = new(nameof(TOKEN_OPR_AND), "&");
        public static readonly TOKEN_OPR TOKEN_OPR_OR = new(nameof(TOKEN_OPR_OR), "|");
        public static readonly TOKEN_OPR TOKEN_OPR_NOT = new(nameof(TOKEN_OPR_NOT), "!");
        public static readonly TOKEN_OPR TOKEN_OPR_COND_EQ = new(nameof(TOKEN_OPR_COND_EQ), "==");
        public static readonly TOKEN_OPR TOKEN_OPR_COND_NEQ = new(nameof(TOKEN_OPR_COND_NEQ), "!=");
        public static readonly TOKEN_OPR TOKEN_OPR_COND_LT = new(nameof(TOKEN_OPR_COND_LT), "<");
        public static readonly TOKEN_OPR TOKEN_OPR_COND_LTE = new(nameof(TOKEN_OPR_COND_LTE), "<=");
        public static readonly TOKEN_OPR TOKEN_OPR_COND_GT = new(nameof(TOKEN_OPR_COND_GT), ">");
        public static readonly TOKEN_OPR TOKEN_OPR_COND_GTE = new(nameof(TOKEN_OPR_COND_GTE), ">=");
        public static readonly TOKEN_OPR TOKEN_OPR_COND_AND = new(nameof(TOKEN_OPR_COND_AND), "&&");
        public static readonly TOKEN_OPR TOKEN_OPR_COND_OR = new(nameof(TOKEN_OPR_COND_OR), "||");
        public static readonly TOKEN_OPR TOKEN_OPR_LS = new(nameof(TOKEN_OPR_LS), "<<");
        public static readonly TOKEN_OPR TOKEN_OPR_RS = new(nameof(TOKEN_OPR_RS), ">>");

        public static readonly TOKEN_MC TOKEN_SW = new(nameof(TOKEN_SW), "switch");
        public static readonly TOKEN_MC TOKEN_SW_COND = new(nameof(TOKEN_SW_COND), "when");
        public static readonly TOKEN_MC TOKEN_IF = new(nameof(TOKEN_IF), "if");
        public static readonly TOKEN_MC TOKEN_IF_ELSE = new(nameof(TOKEN_IF_ELSE), "else");
        public static readonly TOKEN_MC TOKEN_DECLARE = new(nameof(TOKEN_DECLARE), "var", StringComparison.Ordinal);

        public static readonly TOKEN_STR TOKEN_STR = new(string.Empty);
        public static readonly TOKEN_NUM TOKEN_NUM = new(0);
        public static readonly TOKEN_ID TOKEN_ID = new(string.Empty);
        #endregion

        #region Access
        public static IReadOnlyList<IToken> DefaultTokens
        {
            get
            {
                if (field.Count == 0)
                {
                    var type = typeof(TokenRegistry);
                    var fields = type.GetFields(BindingFlags.Static | BindingFlags.Public).Where(fi => typeof(IToken).IsAssignableFrom(fi.FieldType) && fi.IsInitOnly);
                    field = [.. fields.Select(fi => fi.GetValue(null) as IToken).Where(i => i is not null).Select(i => i!)];
                }
                return field;
            }
        } = [];
        #endregion

        #endregion
    }
}
