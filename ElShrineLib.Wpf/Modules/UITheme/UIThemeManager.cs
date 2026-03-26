using ElShrine.Common;
using ElShrine.Common.Serialization;
using ElShrine.Wpf;
using ElShrine.Wpf.Controls.Extensions;
using ElShrine.Wpf.UITheme;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;

namespace ElShrine.Modules.UITheme;

public sealed class UIThemeManager : ViewModelBase, IUIThemeManager
{
    private static UIThemeManager? instance;
    public static UIThemeManager GetInstance() => instance ??= new();

    public UIThemeManager()
    {
        CurrentTheme.PropertyChanged += ThemePropChanged;
        CurrentThemeChanged?.Invoke(this, new(CurrentTheme, CurrentTheme));
        themeByName[Theme.Default.ThemeName] = Theme.Default;
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
    public IReadOnlyList<Theme> AllThemes => Themes;
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
    public event EventHandler? ThemesChanged;
    private void ThemePropChanged(object? sender, PropertyChangedEventArgs e) => CurrentThemeChanged?.Invoke(this, new(CurrentTheme, CurrentTheme));

    public bool NewTheme(string themeName)
    {
        if (themeByName.ContainsKey(themeName)) return false;
        themeByName[themeName] = new Theme(themeName);
        ThemesChanged?.Invoke(this, new());
        return true;
    }
    public bool RemoveTheme(string themeName)
    {
        if (themeName.EqualIgnoreCase(CurrentTheme.ThemeName)) return false;
        var r = themeByName.Remove(themeName);
        if(r) ThemesChanged?.Invoke(this, new());
        return r;
    }
    public void Save()
    {
        DataHandler.Write(Themes);
    }
    public void Load()
    {
        var r = DataHandler.Read(Themes);
        if (r.Success)
        {
            Themes = r.Data!;
            ThemesChanged?.Invoke(this, new());
        }
    }

    #region Registration

    private readonly List<DependencyObject> registeredDPOs = [];
    public bool RegisterCoerceThemeDPs<T>(T control) where T : DependencyObject, IThemeControlBase
    {
        if (registeredDPOs.Contains(control)) return false;
        registeredDPOs.Add(control);
        TransHelper.CoerceValue(control);
        if (control is FrameworkElement fe)
        {
            CurrentThemeChanged += control.GlobalThemeChanged;
            fe.Unloaded += (_, _) =>
            {
                registeredDPOs.Remove(control);
                CurrentThemeChanged -= control.GlobalThemeChanged;
            };
        }
        return true;
    }

    #endregion
}
