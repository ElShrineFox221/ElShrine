using ElShrine.Modules.StateListener;
using ElShrine.Modules.UITheme;
using System.Runtime.CompilerServices;

namespace ElShrine.Modules;

public static class WpfModuleAccessor
{
    public static ImageSourceManager ImageSource => ResolveLocal<ImageSourceManager>();
    public static IStateListenerManager StateListener => ResolveLocal<IStateListenerManager>();
    public static TransitionsManager Transition => ResolveLocal<TransitionsManager>();
    public static IUIThemeManager UITheme => ResolveLocal<IUIThemeManager>();
    
    static WpfModuleAccessor() { }
    private static bool _initialized;
    private static TService ResolveLocal<TService>()
        where TService : class
    {
        if (!_initialized)
        {
            _initialized = true;
            Bootstrapper.Initialize(builder =>
            {
                RegisterWpfModules(builder);
                CoreModuleAccessor.RegisterCoreModules(builder);
            });
        }
        return Bootstrapper.Resolve<TService>();
    }
    public static void RegisterWpfModules(this IModuleRegister register)
    {
        register.RegisterModule<IStateListenerManager, StateListenerManager>(overrides: false);
        register.RegisterModule<IUIThemeManager, UIThemeManager>(overrides: false);

        register.RegisterModule<ImageSourceManager>(overrides: false);
        register.RegisterModule<TransitionsManager>(overrides: false);
    }
}
