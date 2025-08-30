using ElShrine.EOption;
using System.Runtime.CompilerServices;

namespace ElShrine
{
    [AttributeUsage(AttributeTargets.All)]
    public abstract class ValidatableAttribute() : Attribute()
    {
        public string? ValidateFaliedReason = null;
        public abstract bool Validate(object obj);
    }

    [AttributeUsage(AttributeTargets.All)]
    public class ModeValidatableAttribute() : ValidatableAttribute()
    {
        public virtual LoadMode Mode { get; } = LoadMode.None;
        public override bool Validate(object obj)
        {
            var localValidated = obj.ModeMatched(Mode);
            if (!localValidated) ValidateFaliedReason = $"<{obj}> is invalid, it failed matching mode.";
            return localValidated;
        }
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class StartupClassAttribute() : ModeValidatableAttribute()
    {
        public override LoadMode Mode => LoadMode.AllAccessible | LoadMode.AllInstiateble | LoadMode.Class;
        public override bool Validate(object obj)
        {
            var baseValidated = base.Validate(obj);
            var localValidated = false;
            if (baseValidated)
            {
                if (obj is Type type)
                {
                    //Try to instantiate
                    try
                    {
                        if (type.IsStaticClass()) RuntimeHelpers.RunClassConstructor(type.TypeHandle);
                        else if (type.IsImplementOf(typeof(ISingleton))) ISingleton.GetInstance(type);
                        else Activator.CreateInstance(type);
                        localValidated = true;
                    }
                    catch (Exception e)
                    {
                        while (e.InnerException is not null) e = e.InnerException;
                        ValidateFaliedReason = e.Message;
                    }
                }
                else ValidateFaliedReason = $"<{obj}> is invalid type parameter, it can not convert to <System.Type>.";
            }
            return localValidated;
        }
    }
}
