using ElShrine.EFile;
using ElShrine.EOption;
using ElShrine.Wpf.ViewModel;
using System.Collections.Generic;

namespace ElShrine.Wpf.UITheme
{
    [StartupClass]
    public class ThemeManager : ViewModelBase, ISingleton<ThemeManager>
    {
        public static ThemeManager? Instance { get; protected set; }
        public static ThemeManager GetInstance() => Instance ??= new();
        protected ThemeManager() { }

        public Theme CurrentTheme { get; protected set; } = Theme.Default;
        public readonly List<Theme> LoadedThemes = [Theme.Default];
        private class Themes : List<Theme> { }

        public void CreateNewTheme(string name)
        {
            var index = LoadedThemes.FindIndex(t => t.ThemeName == name);
            if (index == -1) LoadedThemes.Add(new() { ThemeName = name});
        }
        public void RemoveTheme(string name)
        {
            var index = LoadedThemes.FindIndex(t => t.ThemeName == name);
            if (index != -1)
            {
                if (!LoadedThemes[index].ThemeName.EqualIgnoreCase(name)) LoadedThemes.RemoveAt(index);
                //select
            } 
        }
        public void Save()
        {
            DataHandler.Write<Themes>([.. LoadedThemes]);
        }
        public void Load()
        {
            var r = DataHandler.Read<Themes>([.. LoadedThemes]);
            if (r.Success && r.Data is not null) LoadedThemes.ReplaceAll(r.Data);
        }
    }
}
