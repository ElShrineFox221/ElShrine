using ElShrine.Common;
using ElShrine.Common.DataStructure;

namespace ElShrine.Modules.Option;

public interface IOptionManager
{
    #region Access
    public IEnumerable<Cata> Get(bool onlyChanged = false);
    public IEnumerable<OptionItem> Get(string virtualCata, bool onlyChanged = false);
    public IReadOnlyDictionary<Cata, IReadOnlyList<OptionItem>> GetAll(bool onlyChanged = false);
    #endregion

    #region Data persistence
    void Save();
    void Load();
    #endregion

    event ValueChangedHandler<object>? OptionChanged;
    event CollectionChangedHandler<Cata>? CataChanged;
}
