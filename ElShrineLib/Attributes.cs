using System.Reflection;

namespace ElShrine
{
    #region Validatable bases
    [AttributeUsage(AttributeTargets.All)]
    public abstract class ValidatableBaseAttribute() : Attribute()
    {
        public string? ValidateFailedReason { get; protected set; } = null;
        protected abstract bool Validate(Type attributedTargetType, object? extraInstance);
        internal bool DoValidate(Type attributedTargetType, object? extraInstance)
        {
            var suc = false;
            try
            {
                suc = Validate(attributedTargetType, extraInstance);
            }
            catch (Exception e)
            {
                while (e.InnerException is not null) e = e.InnerException;
                ValidateFailedReason = $"[{GetType().Name}] Validation error: {e.Message}";
            }
            return suc;
        }
    }
    public abstract class ValidatableBaseAttribute<T>(bool inherit, bool nullable) : ValidatableBaseAttribute where T : class
    {
        public bool Inherit = inherit;
        public bool Nullable = nullable;
        protected sealed override bool Validate(Type attributedTargetType, object? extraInstance)
        {
            //Nullable
            var nullableCheck = extraInstance is not null || Nullable;
            if (!nullableCheck)
            {
                ValidateFailedReason = $"Expected a instance of type {typeof(T).FullName} but received a null value.";
                return false;
            }
            //
            if (Inherit)
            {
                if (!typeof(T).IsBaseOrInterfaceOf(attributedTargetType))
                {
                    ValidateFailedReason = $"Expected {typeof(T).FullName}'s implement but received {attributedTargetType.FullName}.";
                    return false;
                }
                if (extraInstance is not null) 
                {
                    if (extraInstance is not T t)
                    {
                        ValidateFailedReason = $"Expected {typeof(T).FullName}'s implement but received a {extraInstance.GetType().FullName} instance.";
                        return false;
                    }
                    else return Validate(t);
                }
            }
            else
            {
                if (typeof(T) != attributedTargetType)
                {
                    ValidateFailedReason = $"Expected {typeof(T).FullName} but received {attributedTargetType.FullName}.";
                    return false;
                }
                if (extraInstance is not null)
                {
                    if (extraInstance is not T t || t.GetType() != typeof(T))
                    {
                        ValidateFailedReason = $"Expected {typeof(T).FullName} instance but received a {extraInstance.GetType().FullName} instance.";
                        return false;
                    }
                    else return Validate(t);
                }
            }
            return true;
        }
        protected abstract bool Validate(T extraInstance);
    }
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public abstract class ValidatableClassAttribute : ValidatableBaseAttribute
    {
        protected sealed override bool Validate(Type attributedTargetType, object? extraInstance)
        {
            if (extraInstance is not null)
            {
                ValidateFailedReason = $"Type validation expects null extraInstance, but got {extraInstance.GetType().FullName}.";
                return false;
            }
            return ValidateType(attributedTargetType);
        }
        protected abstract bool ValidateType(Type typeToValidate);
    }
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Method)]
    public abstract class ValidatableMemberAttribute() : ValidatableBaseAttribute<MemberInfo>(true, false);
    #endregion

    [AttributeUsage(AttributeTargets.Class)]
    public class SingletonAttribute() : ValidatableClassAttribute()
    {
        public BindingFlags ItemSearchFlags = BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance;

        protected override bool ValidateType(Type typeToValidate)
        {
            var suc = false;
            var t = typeToValidate;
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
