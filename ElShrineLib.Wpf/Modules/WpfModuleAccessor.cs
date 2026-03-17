using ElShrine.Modules.StateListener;
using ElShrine.Modules.UITheme;
using System.Runtime.CompilerServices;

namespace ElShrine.Modules;

public static class WpfModuleAccessor
{
    public static ImageSourceManager ImageSource => Bootstrapper.Resolve<ImageSourceManager>();
    public static IStateListenerManager StateListener => Bootstrapper.Resolve<IStateListenerManager>();
    public static TransitionsManager Transition => Bootstrapper.Resolve<TransitionsManager>();
    public static IUIThemeManager UITheme => Bootstrapper.Resolve<IUIThemeManager>();
    
    static WpfModuleAccessor()
    {
        RuntimeHelpers.RunClassConstructor(typeof(CoreModuleAccessor).TypeHandle);
        Bootstrapper.Initialize(RegisterWpfModules);
    }
    private static void RegisterWpfModules(this IModuleRegister register)
    {
        register.RegisterModule<IStateListenerManager, StateListenerManager>(overrides: false);
        register.RegisterModule<IUIThemeManager, UIThemeManager>(overrides: false);

        register.RegisterModule<ImageSourceManager>(overrides: false);
        register.RegisterModule<TransitionsManager>(overrides: false);
    }
}
