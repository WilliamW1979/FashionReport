using Dalamud.Game.Command;
using Dalamud.Interface.Windowing;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using FashionReport.Windows;
using System.Runtime.Serialization.DataContracts;
using System.Security.Permissions;

namespace FashionReport;

public sealed class FASHIONREPORT : IDalamudPlugin
{
    [PluginService] public static IDalamudPluginInterface Interface { get; private set; } = null!;
    [PluginService] public static IChatGui ChatGui { get; private set;} = null!;
    [PluginService] public static IClientState ClientState { get; private set;} = null!;
    [PluginService] public static ICommandManager CommandManager { get; private set; } = null!;
    [PluginService] public static ICondition Condition { get; private set; } = null!;
    [PluginService] public static IDataManager DataManager { get; private set; } = null!;
    [PluginService] public static IFramework Framework { get; private set; } = null!;
    [PluginService] public static IGameGui? GameGui { get; private set; } = null!;
    [PluginService] public static IKeyState KeyState { get; private set;} = null!;
    [PluginService] public static IObjectTable ObjectTable { get; private set;} = null!;
    [PluginService] public static IPartyList PartyList { get; private set;} = null!;
//    [PluginService] public static ISigScanner SigScanner { get; private set;} = null!;
//    [PluginService] public static ITargetManager TargetManager { get; private set;} = null!;
    [PluginService] public static ITextureProvider TextureProvider { get; private set;} = null!;

    public CONFIGURATION Configuration { get; init; }

    public readonly WindowSystem WindowSystem = new("Fashion Report");
    private MAINWINDOW MainWindow { get; init; }
    public GEARMANAGER GearManager { get; init; }

    public FASHIONREPORT()
    {
        Configuration = Interface.GetPluginConfig() as CONFIGURATION ?? new CONFIGURATION();
        MainWindow = new MAINWINDOW(this);
        GearManager = new GEARMANAGER(this);

        WindowSystem.AddWindow(MainWindow);

#pragma warning disable CS8602
        CommandManager.AddHandler("/fr", new CommandInfo(OnCommand) { HelpMessage = "Fashion Report calculator!" });
        CommandManager.AddHandler("/fashionreport", new CommandInfo(OnCommand) { HelpMessage = "Fashion Report calculator!" });
#pragma warning restore CS8602

        Interface.UiBuilder.Draw += DrawUI;
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
