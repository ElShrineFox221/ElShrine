using ElShrine.Modules;
using System.Reflection;

namespace ElShrine
{
    #region Validatable bases
    [AttributeUsage(AttributeTargets.All)]
    public abstract class ValidatableAttribute() : Attribute()
    {
        public string? ValidateFailedReason { get; protected set; } = null;
        protected abstract bool Validate(object obj);
        public bool DoValidate(object obj)
        {
            var suc = false;
            try
            {
                suc = Validate(obj);
            }
            catch (Exception e)
            {
                while (e.InnerException is not null) e = e.InnerException;
                ValidateFailedReason = $"[{GetType().Name}] Validation error: {e.Message}";
            }
            return suc;
        }
    }
    public abstract class ValidatableBase<T> : ValidatableAttribute where T : class
    {
        protected sealed override bool Validate(object target)
        {
            if (target is T t)
            {
                return Validate(t);
            }
            ValidateFailedReason = $"Expected {typeof(T).Name} but received {target?.GetType().Name ?? "null"}.";
            return false;
        }
        protected abstract bool Validate(T target);
    }
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public abstract class ValidatableClassAttribute() : ValidatableBase<Type>();
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Method)]
    public abstract class ValidatableMemberAttribute() : ValidatableBase<MemberInfo>();
    #endregion

    [AttributeUsage(AttributeTargets.Class)]
    public class SingletonAttribute() : ValidatableClassAttribute()
    {
        public BindingFlags ItemSearchFlags = BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance;
        protected override bool Validate(Type t)
        {
            var suc = false;
            if (t.IsStaticClass() || typeof(IInitializable<>).IsBaseOrInterfaceOf(t)) suc = true;
            else ValidateFailedReason = $"Type <{t.FullName}> is not a static class or implement of interface {nameof(IInitializable<>)}<TIns>.";
            return suc;
        }
    }
    public class SingletonItemAttribute<TOwnerAttr>() : ValidatableMemberAttribute() where TOwnerAttr : SingletonAttribute
    {
        protected readonly Type RequiredOwnerAttribtue = typeof(TOwnerAttr);
        protected override bool Validate(MemberInfo target)
        {
            var t = target.DeclaringType;
            var notSuc = t?.GetCustomAttribute<SingletonAttribute>(true) is null;
            if (notSuc) ValidateFailedReason = $"Type <{t?.FullName}> is not a singleton class, the class it belong to should be a {RequiredOwnerAttribtue.Name} attributed class.";
            return !notSuc;
        }
    }
}
