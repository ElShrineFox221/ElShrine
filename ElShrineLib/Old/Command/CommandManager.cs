using System.Reflection;

namespace ElShrine.Old.Command
{
    [Obsolete(ObsoleteMsg.OldNamespaceMsg)]
    public static class CommandManager
    {
        #region Command infos
        private readonly static List<CommandInfo> commandInfoCollection = [];
        private static void RefreshCommandInfoCollection()
        {
            commandInfoCollection.Clear();
            var attributedTypes = ClassesManager.GetClassesByAttribute<CommandCarrierAttribute>();
            foreach (var at in attributedTypes)
            {
                List<CommandInfo> commandInfos = [];
                List<MethodInfo> ignoreList = [];
                CommandCarrierAttribute commandCarrierAttr = at.attrs.First();
                string carrierName = commandCarrierAttr.Name.IsEmpty() ? at.type.Name : commandCarrierAttr.Name;
                string? formatedCarrierName = commandCarrierAttr.IsGlobalCarrier ? null : carrierName;
                string[] carrierNames = [at.type.Name, at.attrs.First().Name];
                foreach (MethodInfo method in at.type.GetMethods())
                {
                    var commandAttrs = method.GetCustomAttributes<CommandAttribute>().ToArray();
                    if (commandAttrs.Length != 0)
                    {
                        if (!commandAttrs.Any((ca) => ca.IgnoreThis))
                        {
                            CommandInfo commandInfo = new(method, commandAttrs, carrierNames);
                            commandInfos.Add(commandInfo);
                        }
                        else ignoreList.Add(method);
                    }
                }
                // force load
                MethodInfo[] priMethods = at.type.GetMethods(BindingFlags.Static | BindingFlags.NonPublic);
                MethodInfo[] pubMethods = at.type.GetMethods(BindingFlags.Static | BindingFlags.Public);
                MethodInfo[] autoLoadedMethods = commandCarrierAttr.LoadMode switch
                {
                    CommandAutoLoadMode.All => [.. priMethods, .. pubMethods],
                    CommandAutoLoadMode.Private => priMethods,
                    CommandAutoLoadMode.Public => pubMethods,
                    _ => []
                };
                foreach (MethodInfo methodInfo in autoLoadedMethods)
                {
                    if (commandInfos.FindIndex((ci) => ci.Method == methodInfo) == -1 && ignoreList.FindIndex((mi) => mi == methodInfo) == -1) commandInfos.Add(new(methodInfo, null, carrierNames));
                }
                // auto load
                commandInfoCollection.AddRange(commandInfos);
            }
        }
        public static CommandInfo[] CommandInfoCollection
        {
            get
            {
                if (commandInfoCollection.Count == 0) RefreshCommandInfoCollection();
                return [.. commandInfoCollection];
            }
        }
        #endregion

        public static object? StrToParamDefault(string s)
        {
            object? result;
            if (s == "True" || s == "true") result = true;
            else if (s == "False" || s == "false") result = false;
            else if (s == "Null" || s == "null") result = null;
            else if (double.TryParse(s, out double d))
            {
                result = d;
                float f = (float)d;
                if ((double)f == d) result = f;
                int i = (int)Math.Round(d);
                if (i == d) result = i;
            }
            else result = s.Replace("\"", "");
            return result;
        }
    }
}
