using System.Reflection;
using static ElShrine.CommonHelper;
using static ElShrine.EConsole.ConsoleManager;

namespace ElShrine.ECommand
{
    [StartupClass]
    public static class CommandManager
    {
        private record class CommandInfo : ICommandInfo
        {
            public CommandInfo(MethodInfo methodInfo, CommandAttribute? commandAttr, CommandCarrierInfo carrierInfo)
            {
                MethodInfo = methodInfo;
                CarrierInfo = carrierInfo;
                
                Description = commandAttr?.Description ?? string.Empty;
                OverrideName = commandAttr?.Name;
                //
                var sec0pre = CarrierInfo.OverrideName ?? CarrierInfo.Type.Name;
                var sec0 = sec0pre.IsEmpty() ? string.Empty : sec0pre + DOT;
                var sec1 = Name;
                var sec3pre = methodInfo.GetParameters().BuildString(pi => $"<{pi.ParameterType.Name}:{pi.Name}>", " ");
                var sec3 = methodInfo.GetParameters().Length == 0 ? string.Empty : " " + sec3pre;
                UsageTooltip = $"[{sec0}{sec1}{sec3}]";
            }

            #region For search and display
            //Own
            public string Name => OverrideName ?? MethodInfo.Name;
            public string? OverrideName { get; set; }
            public string Description { get; set; }
            public string UsageTooltip { get; set; }

            //For invoke
            public ICommandCarrierInfo CarrierInfo { get; set; }
            public MethodInfo MethodInfo { get; set; }
            #endregion

            public Type[] Parameters { get; private set; } = [];
            #region For tokenize cache
            private readonly List<ParamParserBase> Parsers = [];
            private readonly List<(char left, char right)> BracketSets = [];
            private bool Initialized = false;
            private void InitializeParsers()
            {
                List<ParamParserBase> parsers = [];
                var parameterInfos = MethodInfo.GetParameters();
                Parameters = [.. parameterInfos.Select(pmi => pmi.ParameterType)];
                foreach (var parameterInfo in parameterInfos)
                {
                    try
                    {
                        parsers.Add(ParamParserManager.GetParameterParser(parameterInfo.ParameterType));
                    }
                    catch
                    {
                        ListWarnInfo([GetWarningItem(), new($"Fount no parser with parse type <{parameterInfo.ParameterType}>, use type <{typeof(object).Name}> instead.")]);
                        parsers.Add(ParamParserManager.GetParameterParser(typeof(object)));
                    }
                }
                Parsers.ReplaceAll(parsers);
            }
            private void InitializeBrackSets()
            {
                List<(char left, char right)> cachedBracketSet = [];
                List<Type> cachedTypes = []; 
                foreach (var parser in Parsers)
                {
                    if (!cachedTypes.Contains(parser.ParseType))
                    {
                        cachedTypes.Add(parser.ParseType);
                        var validatedBranketSet = parser.ValidatedBraketSet();
                        if (validatedBranketSet.HasValue)
                        {
                            var set = validatedBranketSet.Value;
                            var index = cachedBracketSet.FindIndex(cachedSet => (cachedSet.left == set.left || cachedSet.left == set.right || cachedSet.right == set.left || cachedSet.right == set.right)&&!(cachedSet.left == set.left && cachedSet.right == set.right));
                            if (index != -1)
                            {
                                throw new("Multimatched parser bracket sign, please considering change the custom parser or method parameter type.");
                            }
                            cachedBracketSet.Add(set);
                        }
                    }
                }
                BracketSets.ReplaceAll(cachedBracketSet);
            }
            public void Initialize()
            {
                if (!Initialized)
                {
                    InitializeParsers();
                    InitializeBrackSets();
                    Initialized = true;
                }
            }
            #endregion

            public List<string> Tokenize(string str, char split = SPACE, bool removeOuterBracket = true)
            {
                Initialize();
                return ParamParserManager.Tokenize(str, BracketSets, split, removeOuterBracket);
            }
            public List<object?> ParseParams(List<string> tokens)
            {
                List<object?> result = [];
                for (int i = 0; i < tokens.Count; i++)
                {
                    result.Add(Parsers[i].TokenTranfer(tokens[i]));
                }
                return result;
            }
        }
        private record class CommandCarrierInfo : ICommandCarrierInfo
        {
            public CommandCarrierInfo(Type type, CommandCarrierAttribute carrierAttr)
            {
                Type = type;
                OverrideName = carrierAttr.Name;
                Mode = carrierAttr.ItemMode;
            }

            public Type Type { get; init; }
            public string Name => OverrideName ?? Type.Name;
            public string? OverrideName { get; init; }
            public LoadMode Mode { get; init; }
            public List<ICommandInfo> OwnCommandInfos { get; init; } = [];
        }

        private static readonly List<CommandInfo> AllCommandInfo = [];
        public static ICommandInfo[] CommandInfos => [.. AllCommandInfo];
        private static readonly List<CommandCarrierInfo> AllCommandCarrierInfo = [];
        public static ICommandCarrierInfo[] CommandCarrierInfos => [.. AllCommandCarrierInfo];

        public readonly static List<Task<ICommandResult>> RunningCommands = [];
        static CommandManager()
        {
            LoadAllCammandInfo();
            ListContentInfo($"Loaded {AllCommandInfo.Count} {"command".GetPural(AllCommandInfo.Count)} in {AllCommandCarrierInfo.Count} {"carrier".GetPural(AllCommandCarrierInfo.Count)}.", true);
        }
        private static void LoadAllCammandInfo()
        {
            var attrClasses = ClassesManager.GetClassesByAttribute<CommandCarrierAttribute>(true);
            foreach (var attrClass in attrClasses)
            {
                Type type = attrClass.type;
                CommandCarrierInfo carrierInfo = new(type, attrClass.attrs.First());
                
                //ALL METHODS
                List<CommandInfo> tempList = [];
                //
                var allMethodInfos = type.GetMethods();
                //Get auto load methods
                var carrierLoadMethods = type.GetMethods(carrierInfo.Mode.ToSearchFlags());
                tempList.AddRange(carrierLoadMethods.Select(mi =>
                    new CommandInfo(mi, mi.GetCustomAttribute<CommandAttribute>(true), carrierInfo)));
                //Get or remove specified methods
                foreach (var methodInfo in allMethodInfos)
                {
                    var commandAttr = methodInfo.GetCustomAttribute<CommandAttribute>(true);
                    if (commandAttr != null)
                    {
                        var cachedCIIndex = tempList.FindIndex(ci => ci.MethodInfo == methodInfo);
                        if (commandAttr.Ignored && cachedCIIndex != -1) tempList.RemoveAt(cachedCIIndex);
                        else if (!(commandAttr.Ignored || cachedCIIndex != -1)) tempList.Add(
                            new CommandInfo(methodInfo, commandAttr, carrierInfo));
                    }
                }
                
                foreach (var tempCommandInfo in tempList) tempCommandInfo.Initialize();
                carrierInfo.OwnCommandInfos.AddRange(tempList);
                AllCommandInfo.AddRange(tempList);
                AllCommandCarrierInfo.Add(carrierInfo);
            }
        }

        public static List<ICommandInfo> FindCommandInfo(string carrierName, string commandName)
            => [..AllCommandInfo.FindAll(ci =>
            {
                bool carrierMatched = 
                    carrierName.EqualIgnoreCase(ci.CarrierInfo.Type.FullName ?? string.Empty) ||
                    carrierName.EqualIgnoreCase(ci.CarrierInfo.Type.Name) ||
                    carrierName.EqualIgnoreCase(ci.CarrierInfo.OverrideName ?? string.Empty);
                bool commandMatched =
                    commandName.EqualIgnoreCase(ci.Name) ||
                    carrierName.EqualIgnoreCase(ci.MethodInfo.Name);
                return carrierMatched && commandMatched;
            })];
        public static List<ICommandCarrierInfo> FindCommandCarrierInfo(string carrierName)
            => [..AllCommandCarrierInfo.FindAll(cci =>
            {
                bool carrierMatched =
                    carrierName.EqualIgnoreCase(cci.Type.FullName ?? string.Empty) ||
                    carrierName.EqualIgnoreCase(cci.Type.Name) ||
                    carrierName.EqualIgnoreCase(cci.OverrideName ?? string.Empty);
                return carrierMatched;
            })];
    }
}
