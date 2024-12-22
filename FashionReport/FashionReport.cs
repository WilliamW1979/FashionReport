using Dalamud.Game.Command;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin;
using FashionReport.Windows;

namespace FashionReport;

public sealed class FASHIONREPORT : IDalamudPlugin
{
    public CONFIGURATION Configuration { get; init; }

    public readonly WindowSystem WindowSystem = new("Fashion Report");
    private MAINWINDOW MainWindow { get; init; }
    public GEARMANAGER GearManager { get; init; }

    public FASHIONREPORT(IDalamudPluginInterface pluginInterface)
    {
        pluginInterface.Create<SERVICES>();
        Configuration = SERVICES.Interface.GetPluginConfig() as CONFIGURATION ?? new CONFIGURATION();
        MainWindow = new MAINWINDOW(this);
        GearManager = new GEARMANAGER(this);

        WindowSystem.AddWindow(MainWindow);

#pragma warning disable CS8602
        SERVICES.CommandManager.AddHandler("/fr", new CommandInfo(OnCommand) { HelpMessage = "Fashion Report calculator!" });
        SERVICES.CommandManager.AddHandler("/fashionreport", new CommandInfo(OnCommand) { HelpMessage = "Fashion Report calculator!" });
#pragma warning restore CS8602

        SERVICES.Interface.UiBuilder.Draw += DrawUI;
        SERVICES.Interface.UiBuilder.OpenMainUi += ToggleMainUI;
    }

    public void Dispose()
    {
        WindowSystem.RemoveAllWindows();
        MainWindow.Dispose();

        SERVICES.CommandManager.RemoveHandler("/fr");
        SERVICES.CommandManager.RemoveHandler("/fashionreport");
    }

    private void OnCommand(string command, string args)
    {
        ToggleMainUI();
    }

    private void DrawUI() => WindowSystem.Draw();

    public void ToggleMainUI() => MainWindow.Toggle();
}
