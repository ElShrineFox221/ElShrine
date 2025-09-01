using System.Text;
using static ElShrine.CommonHelper;
using static ElShrine.EConsole.ConsoleManager;
using ElShrine.EConsole;
using System.Reflection;

namespace ElShrine.ECommand
{
    [StartupClass]
    public static class ParamParserManager
    {
        private readonly static Dictionary<Type, ParamParserBase> ParserDictionary = [];
        private readonly static List<Type> cachedTypes = [];
        private readonly static List<Type> cachedParserTypes = [];
        private readonly static List<char> cachedChars = [];
        static ParamParserManager() => ClassesManager.AssembliesLoaded += LoadParamParsers;

        private static void LoadParamParsers(Assembly[] assemblies)
        {
            var parserClasses = typeof(ParamParserBase).GetImplements(assemblies);
            foreach (var parserClass in parserClasses)
            {
                var parserInstance = Activator.CreateInstance(parserClass) as ParamParserBase;
                if (parserInstance is not null)
                {
                    var cachedTypeIndex = cachedTypes.IndexOf(parserInstance.ParseType);
                    if (cachedTypeIndex == -1)
                    {
                        ParserDictionary.Add(parserInstance.ParseType, parserInstance);
                        cachedTypes.Add(parserInstance.ParseType);
                        cachedParserTypes.Add(parserClass);
                        var bracketSet = parserInstance.ValidatedBraketSet();
                        if (bracketSet.HasValue)
                        {
                            var (left, right) = bracketSet.Value;
                            if (cachedChars.Contains(left) || cachedChars.Contains(right)) ListWarnInfo([GetWarningItem(true), new($" There are repeat chars in bracksets <\'{left}\',\'{right}\'>, it may causes error when parsing command.")], true);
                            else if (left == right) cachedChars.Add(left);
                            else cachedChars.AddRange([left, right]);
                        }
                    }
                    else
                    {
                        var cachedParser = ParserDictionary[cachedTypes[cachedTypeIndex]];
                        var containedItem = new InformationItem($"The parser are already contained the <{parserInstance.ParseType.Name}> parser <{cachedParser}>, ", InformationPaintStyle.Sub);
                        InformationItem extraItem;
                        if (cachedParser.Priority > parserInstance.Priority) extraItem = new($"the parser <{parserInstance}> would be ignored.", InformationPaintStyle.Sub);
                        else
                        {
                            ParserDictionary[cachedTypes[cachedTypeIndex]] = parserInstance;
                            extraItem = new($"the parser <{cachedParser}> would be replaced to <{parserInstance}>.", InformationPaintStyle.Sub);
                        }

                        ListWarnInfo([GetWarningItem(true), containedItem, extraItem], true);
                    }
                }
            }
            ListContentInfo($"Loaded {cachedTypes.Count} parsers: {cachedTypes.BuildString(t => $"<{t.Name}>")}");
        }

        public static ParamParserBase GetParameterParser(Type type)
            => ParserDictionary[type];

        public static List<string> Tokenize(string str, List<(char left, char right)> bracketSets, char split, bool removeOuterBracket)
        {
            List<string> tokens = [];
            StringBuilder? tokenBuilder = null;
            bool splitEqualSpace = split == SPACE, lastInToken = false;
            char? currentLeft = null, currentRight = null;

            for (int index = 0, cachedSpaceCount = 0, braketDegree = 0; index < str.Length; index++)
            {
                char currentChar = str[index];
                if (braketDegree == 0)
                {
                    
                    if (splitEqualSpace)
                    {
                        if (currentChar == SPACE)
                        {
                            if (lastInToken) finishBuildToken();
                            else continue;
                        }
                        else
                        {
                            var bracketSetIndexByLeft = bracketSets.FindIndex(cs => cs.left == currentChar);
                            if (bracketSetIndexByLeft != -1)
                            {
                                braketDegree++;
                                currentLeft = bracketSets[bracketSetIndexByLeft].left;
                                currentRight = bracketSets[bracketSetIndexByLeft].right;
                            }
                            else if (bracketSets.FindIndex(cs => cs.right == currentChar) != -1) throw new("Meet a right bracket <> but no matched left bracket.");
                            if (!removeOuterBracket || bracketSetIndexByLeft == -1) appendChar(currentChar);
                            lastInToken = true;
                        }
                    }
                    else
                    {
                        //not space split
                        if (currentChar == SPACE)
                        {
                            if (lastInToken) cachedSpaceCount++;
                            else cachedSpaceCount = 0;
                        }
                        else if (currentChar == split) finishBuildToken();
                        else
                        {
                            while (cachedSpaceCount > 0)
                            {
                                appendChar(SPACE);
                                cachedSpaceCount--;
                            }
                            var bracketSetIndexByLeft = bracketSets.FindIndex(cs => cs.left == currentChar);
                            if (bracketSetIndexByLeft != -1)
                            {
                                braketDegree++;
                                currentLeft = bracketSets[bracketSetIndexByLeft].left;
                                currentRight = bracketSets[bracketSetIndexByLeft].right;
                            }
                            else if (bracketSets.FindIndex(cs => cs.right == currentChar) != -1) throw new("Meet a right bracket <> but no matched left bracket.");
                            if (!removeOuterBracket || bracketSetIndexByLeft == -1) appendChar(currentChar);
                            lastInToken = true;
                        }
                    }
                }
                else
                {
                    tokenBuilder ??= new();
                    if (currentChar == currentRight) braketDegree--;
                    else if (currentChar == currentLeft) braketDegree++;
                    if (!removeOuterBracket || currentChar != currentLeft && currentChar != currentRight) appendChar(currentChar);
                    lastInToken = true;
                }
            }
            
            finishBuildToken();
            return tokens;
            void appendChar(char c) => (tokenBuilder ??= new()).Append(c);
            void finishBuildToken()
            {
                if (tokenBuilder is not null)
                {
                    tokens.Add(tokenBuilder.ToString());
                    tokenBuilder = new();
                    lastInToken = false;
                }
                else tokenBuilder = new();
            }
        }
    }
}
