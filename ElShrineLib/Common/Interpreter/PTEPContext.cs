using System.Reflection;

namespace ElShrine.Common.Interpreter
{
    public class PTEPContext : IASTContext
    {
        public PTEPContext()
        {
            
        }

        public object? GetConstantValue(string key)
        {
            throw new NotImplementedException();
        }

        public MethodInfo GetMethodInfo(string name)
        {
            throw new NotImplementedException();
        }

        public object? GetVariableValue(string key)
        {
            throw new NotImplementedException();
        }

        public object? SetConstantValue(string key, object? value)
        {
            throw new NotImplementedException();
        }

        public object? SetVariableValue(string key, object? value)
        {
            throw new NotImplementedException();
        }
    }
}
