using System.Reflection;

namespace ElShrine.EOption
{
    public interface ISingleton<T> : ISingleton
    {
        abstract static T GetInstance();
    }
    public interface ISingleton
    {
        public static object? GetInstance(Type ieiType)
        {
            MethodInfo? method = ieiType.GetMethod(nameof(GetInstance), BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
            object? o = method?.Invoke(null, null);
            return o;
        }
    }
}
