using Dalamud.Game;
using Dalamud.Game.ClientState.Objects;
using Dalamud.Game.Command;
using Dalamud.Interface.Windowing;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using FashionReport.Windows;
using System.IO;

namespace FashionReport;

public sealed class FASHIONREPORT : IDalamudPlugin
{
    [PluginService] internal static IDalamudPluginInterface Interface { get; private set; } = null!;
    //    [PluginService] internal IChatGui ChatGui { get; init; }
    //    [PluginService] internal IClientState ClientState { get; init; }
    [PluginService] internal ICommandManager CommandManager { get; init; }
    //    [PluginService] internal ICondition Condition { get; init; }
    //    [PluginService] internal IDataManager DataManager { get; init; }
    //    [PluginService] internal IFramework Framework { get; init; }
    [PluginService] internal IGameGui? GameGui { get; init; }
    //    [PluginService] internal IKeyState KeyState { get; init; }
    //    [PluginService] internal IObjectTable ObjectTable { get; init; }
    //    [PluginService] internal IPartyList PartyList { get; init; }
    //    [PluginService] internal ISigScanner SigScanner { get; init; }
    //    [PluginService] internal ITargetManager TargetManager { get; init; }

    public CONFIGURATION Configuration { get; init; }

    public readonly WindowSystem WindowSystem = new("Fashion Report");
//    private CONFIGWINDOW? ConfigWindow { get; init; }
    private MAINWINDOW MainWindow { get; init; }

    public FASHIONREPORT()
    {
        Configuration = Interface.GetPluginConfig() as CONFIGURATION ?? new CONFIGURATION();
        MainWindow = new MAINWINDOW(this);

//        WindowSystem.AddWindow(ConfigWindow);
        WindowSystem.AddWindow(MainWindow);

#pragma warning disable CS8602
        CommandManager.AddHandler("/fr", new CommandInfo(OnCommand) { HelpMessage = "Fashion Report calculator!" });
        CommandManager.AddHandler("/fashionreport", new CommandInfo(OnCommand) { HelpMessage = "Fashion Report calculator!" });
#pragma warning restore CS8602

        Interface.UiBuilder.Draw += DrawUI;

        // This adds a button to the plugin installer entry of this plugin which allows
        // to toggle the display status of the configuration ui
//        Interface.UiBuilder.OpenConfigUi += ToggleConfigUI;

        // Adds another button that is doing the same but for the main ui of the plugin
        Interface.UiBuilder.OpenMainUi += ToggleMainUI;
    }

    public void Dispose()
    {
        WindowSystem.RemoveAllWindows();

//        ConfigWindow.Dispose();
        MainWindow.Dispose();

        CommandManager.RemoveHandler("/fr");
        CommandManager.RemoveHandler("/fashionreport");
    }

    private void OnCommand(string command, string args)
    {
        ToggleMainUI();
    }

    private void DrawUI() => WindowSystem.Draw();

    public void ToggleMainUI() => MainWindow.Toggle();
}
