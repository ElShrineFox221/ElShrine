using ElShrine.Modules;
using ElShrine.Modules.Log;

namespace ElShrine.Debug;

public abstract class DebugConsoleProgram
{
    public async static Task Main(string[] _)
    {
        var log = CoreModuleAccessor.Log;
        log.RegisterListener(log.Main, new SystemConsoleLogger());
        while (true)
        {
            if (!log.TemporarilyNoEntriesToUpdate) await Task.Yield();
            var line = Console.ReadLine() ?? string.Empty;
            log.Main.Log(LogItem.Header("INPUT", LogItemStyle.SubInfo), LogItem.Normal(line, LogItemStyle.NoticeDarkYellow));
            var ci = CommandInvoker.Build(line);
            ci.ExecuteAsync().Wait();
        }
    }
    //public static void Initilize() => RuntimeHelpers.RunClassConstructor(typeof(ClassesManager).TypeHandle);
}
//
// MODULE COR: Restructuralize modules.
// Modules manager.
//[Completed] EListView: draggable listview
// IsDraggable property.
// ! Fix DragOrder.
//

// MODULE COR: Rebuild wpf controls.
//[Completed] Rebuild textbox to custom control: ENTER confrim; notice; auto spell; title; titleLocation;
// Rebuild listview to custom control: BUILD itemcontainer: row STYLE: selected style(thickness-selected, overrideColor)
//[Completed] Relocate and rebuild converters.
//[Completed] Rebuild WpfOption to Theme


// MODULE BUILD: Process Launcher
//[Canceled] Advance method GetProcesses 2.
//[Completed] Advance method GetProcesses.
//[Completed] Close method and Button.
//[Completed] Export method.
//[Completed] ! Fix process mismatched.
//[Canceled] TargetPath textbox notice(include arguments).
//[Completed] Transfer Save, Load, Import, Export method to carrier.
//[Completed] Export multiple files.
//[Completed] !!! Fix Icon error.
//[Canceled] ! Fix Open method: Arguments append, admin permission.
//[Completed] !! cor: Opem method -> Process.Launch, Process.Kill.
//[Completed] ! Fix Open method: null param to current directory.
//[Completed] !!! Fix console async command invoke.

//[Completed] ! Open method,
//[Completed] ! ClearDir method, DelFile method.
//[Completed] ! EOption method option.reset
//[Completed] ! Stopwatch of command chunk.
//[Completed] ! SubConsole fix. space line comp.

//[Completed] ！Rebuild global command carrier.
//[Completed] ！_logger.
//[Completed] _logger commands.
//[Completed] ！Parser priority
//↓Instance command result listing.
//↓Instance list saving.
//↓Object parsing.
//↓Instance command invoking.
//[Completed] ！Fix: Empty carrier name match.
//[Completed] ！Rebuild DataHandle.
//[Completed] ！Option rebuild.
//[Completed] cor: Option to existed items.


//[Completed] Example