using ElShrine.Common.DataStructure;

namespace ElShrine.Modules.Command;

public interface IParamParserManager
{
    bool TryConvert(string input, Type targetType, out object? result);
}

public interface ICommandManager
{
    #region view operations
    IReadOnlyList<CommandItem> Get(string virtualCata);
    IReadOnlyList<CommandItem> Get(string virtualCata, string virtualItemName, int paramsCount = -1);
    IReadOnlyDictionary<Cata, IReadOnlyList<CommandItem>> GetAll();
    #endregion
}
