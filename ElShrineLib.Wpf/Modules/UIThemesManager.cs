using ElShrine.Common;
using ElShrine.Common.Serialization;
using ElShrine.Wpf;
using ElShrine.Wpf.UITheme;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;

namespace ElShrine.Modules
{
    [InitializationInfo(PreInstantiate = true)]
    public sealed class UIThemesManager : ViewModelBase, IInitializable<UIThemesManager>
    {
        #region Singleton
        private readonly static Lazy<UIThemesManager> instanceLazy = new(() => new());
        public static UIThemesManager Instance => Bootstrapper.GetInstance<UIThemesManager>();
        public static UIThemesManager GetInstance() => Instance;
        public new static UIThemesManager Initialize() => instanceLazy.Value;
        #endregion
        private UIThemesManager()
        {
            CurrentTheme.PropertyChanged += ThemePropChanged;
            CurrentThemeChanged?.Invoke(this, new(CurrentTheme, CurrentTheme));
        }

        private readonly Dictionary<string, Theme> themeByName = new(StringComparer.OrdinalIgnoreCase);
        private List<Theme> Themes
        {
            get => [.. themeByName.Values];
            set
            {
                themeByName.Clear();
                foreach (var theme in value) themeByName[theme.ThemeName] = theme;
            }
        }
        public Theme CurrentTheme
        {
            get => field;
            set
            {
                if (field != value)
                {
                    var old = field;
                    field = value;
                    old.PropertyChanged -= ThemePropChanged;
                    field.PropertyChanged += ThemePropChanged;
                    CurrentThemeChanged?.Invoke(this, new(old, value));
                }
            }
        } = Theme.Default;
        public event ValueChangedHandler<Theme>? CurrentThemeChanged;
        private void ThemePropChanged(object? sender, PropertyChangedEventArgs e) => CurrentThemeChanged?.Invoke(this, new(CurrentTheme, CurrentTheme));

        public bool NewTheme(string themeName)
        {
            if (themeByName.ContainsKey(themeName)) return false;
            else themeByName[themeName] = new Theme(themeName);
            return true;
        }
        public bool RemoveTheme(string themeName)
        {
            if (themeName.EqualIgnoreCase(CurrentTheme.ThemeName)) return false;
            return themeByName.Remove(themeName);
        }
        public void Save()
        {
            DataHandler.Write(Themes);
        }
        public void Load()
        {
            var r = DataHandler.Read(Themes);
            if (r.Success) Themes = r.Data!;
        }

        #region Registration

        private readonly static List<DependencyObject> registeredDPOs = [];
        public static bool RegisterCoerceThemeDPs<T>(T control) where T : DependencyObject, IThemeControlBase
        {
            if (registeredDPOs.Contains(control)) return false;
            registeredDPOs.Add(control);
            CoerceValue(control);
            if (control is FrameworkElement fe)
            {
                Instance.CurrentThemeChanged += control.GlobalThemeChanged;
                fe.Unloaded += (_, _) =>
                {
                    registeredDPOs.Remove(control);
                    Instance.CurrentThemeChanged -= control.GlobalThemeChanged;
                };
            }
            return true;
        }
        public static void CoerceValue<T>(T control) where T : DependencyObject, IThemeControlBase
        {
            control.CoerceValue(ThemeProperties.SecondaryBrushProperty);
            control.CoerceValue(ThemeProperties.PrimaryBrushProperty);
            control.CoerceValue(ThemeProperties.FontBrushProperty);
            control.CoerceValue(ThemeProperties.BackBrushProperty);
            control.CoerceValue(ThemeProperties.AnimaDurationInProperty);
            control.CoerceValue(ThemeProperties.AnimaDurationOutProperty);
        }

        #endregion
    }
}
