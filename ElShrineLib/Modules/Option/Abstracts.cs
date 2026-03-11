using ElShrine.Common;
using ElShrine.Common.DataStructure;

namespace ElShrine.Modules.Option;

public interface IOptionManager
{
    #region Access
    public IEnumerable<Cata> Get(bool onlyChanged = false);
    public IEnumerable<OptionItem> Get(string virtualCata, bool onlyChanged = false);
    public IReadOnlyDictionary<Cata, IReadOnlyList<OptionItem>> GetAll(bool onlyChanged = false);
    TOption GetOption<TOption>() where TOption : OptionBase;
    #endregion

    #region Edit
    public void Set(OptionItem item, object? value);
    public void Reset(OptionItem item);
    public void Reset(Cata cata)
    {
        var items = Get(cata.CataName);
        foreach (var item in items)
            Reset(item);
    }
    public void Reset()
    {
        var items = GetAll().SelectMany(i => i.Value);
        foreach (var item in items)
            Reset(item);
    }
    #endregion

    #region Data persistence
    void Save();
    void Load();
    #endregion

    event ValueChangedHandler<object?>? OptionChanged;
}
