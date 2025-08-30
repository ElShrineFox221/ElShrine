namespace ElShrine.EOption
{
    [AttributeUsage(AttributeTargets.Class)]
    public class OptionAttribute() : ModeValidatableAttribute()
    {
        public string Name = string.Empty;
        public LoadMode ItemLoadMode = LoadMode.Public | LoadMode.Instance | LoadMode.PropertyAndField;
        public override LoadMode Mode => LoadMode.AllAccessible | LoadMode.Instance | LoadMode.Class;
        public override bool Validate(object obj)
        {
            var baseValidated = base.Validate(obj);
            var localValidated = false;
            if (baseValidated)
            {
                if (obj is Type type)
                {
                    if (type.IsImplementOf(typeof(ISingleton))) localValidated = true;
                    else ValidateFaliedReason = $"<{obj}> is invalid, it implement no interface <{nameof(ISingleton)}>.";
                }
                else ValidateFaliedReason = $"<{obj}> is invalid type parameter, it can not convert to <System.Type>.";
            }
            return localValidated;
        }
    }
}

