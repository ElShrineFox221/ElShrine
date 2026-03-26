using System.Reflection;

namespace ElShrine.Common.Interpreter
{
    public interface IASTResult
    {
        Exception? Error { get; }
        ASTNode Invoker { get; }
        object? Value { get; }
        Type ValueType { get; }
    }
    public class ASTResult(Exception? error, ASTNode invoker, object? result) : IASTResult
    {
        public Exception? Error { get; } = error;
        public ASTNode Invoker { get; } = invoker;
        public object? Value { get; } = result;
        public Type ValueType { get; } = result?.GetType() ?? typeof(object);
    }

    public interface IASTContext
    {
        //ITokenRegistry TokenRegistry { get; }
        object? GetVariableValue(string key);
        object? SetVariableValue(string key, object? value);
        object? GetConstantValue(string key);
        object? SetConstantValue(string key, object? value);
        MethodInfo GetMethodInfo(string name);
    }
    public sealed class ASTTemporaryContext(IReadOnlyDictionary<string, object?> variables, IReadOnlyDictionary<string, object?> constants, IReadOnlyDictionary<string, MethodInfo> methods) : IASTContext
    {
        private readonly Dictionary<string, object?> variables = variables.ToDictionary(kv => kv.Key, kv => kv.Value);
        private readonly Dictionary<string, object?> constants = constants.ToDictionary(kv => kv.Key, kv => kv.Value);
        private readonly Dictionary<string, MethodInfo> methods = methods.ToDictionary(kv => kv.Key, kv => kv.Value);

        public object? GetConstantValue(string key)
        {
            if (!constants.TryGetValue(key, out var value)) constants[key] = value = null;
            return value;
        }
        public object? GetVariableValue(string key)
        {
            if (!variables.TryGetValue(key, out var value)) variables[key] = value = null;
            return value;
        }

        public object? SetConstantValue(string key, object? value) => variables[key] = value;

        public object? SetVariableValue(string key, object? value) => variables[key] = value;
        public MethodInfo GetMethodInfo(string name) => methods[name];
    }
}
