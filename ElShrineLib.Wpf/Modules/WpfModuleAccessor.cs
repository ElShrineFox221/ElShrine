using ElShrine.Modules.StateListener;
using ElShrine.Modules.UITheme;

namespace ElShrine.Modules;

public static class WpfModuleAccessor
{
    public static ImageSourceManager ImageSource => Bootstrapper.Resolve<ImageSourceManager>();
    public static IStateListenerManager StateListener => Bootstrapper.Resolve<IStateListenerManager>();
    public static TransitionsManager Transition => Bootstrapper.Resolve<TransitionsManager>();
    public static IUIThemeManager UITheme => Bootstrapper.Resolve<IUIThemeManager>();
    
    static WpfModuleAccessor() => Bootstrapper.Initialize(RegisterWpfModules);
    private static void RegisterWpfModules(this IModuleRegister register)
    {
        register.RegisterModule<IStateListenerManager, StateListenerManager>();
        register.RegisterModule<IUIThemeManager, UIThemeManager>();

        register.RegisterModule<ImageSourceManager>();
        register.RegisterModule<TransitionsManager>();
    }
}
