using Dalamud.Game.Command;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin;
#pragma warning disable IDE1006

namespace FashionReport;

public sealed class FASHIONREPORT : IDalamudPlugin
{
    public readonly WindowSystem WindowSystem = new("Fashion Report");
    public DATAMANAGEMENT DataManagement { get; set; } = null!;
    private MAINWINDOW MainWindow { get; init; }

    public FASHIONREPORT(IDalamudPluginInterface pluginInterface)
    {
        pluginInterface.Create<SERVICES>();
        DATAMANAGEMENT.Load();
        MainWindow = new MAINWINDOW(this);

        WindowSystem.AddWindow(MainWindow);

#pragma warning disable CS8602
        SERVICES.CommandManager.AddHandler("/fr", new CommandInfo(OnCommand) { HelpMessage = "Fashion Report calculator!" });
        SERVICES.CommandManager.AddHandler("/fashionreport", new CommandInfo(OnCommand) { HelpMessage = "Fashion Report calculator!" });
        SERVICES.Interface.UiBuilder.Draw += DrawUI;
        SERVICES.Interface.UiBuilder.OpenMainUi += ToggleMainUI;
#pragma warning restore CS8602

        GEARMANAGER.Generate();
        DATAMANAGEMENT.Load();
    }

    public void Dispose()
    {
        WindowSystem.RemoveAllWindows();
        MainWindow.Dispose();
        SERVICES.CommandManager.RemoveHandler("/fr");
        SERVICES.CommandManager.RemoveHandler("/fashionreport");
    }

    private void OnCommand(string command, string args) => ToggleMainUI();
    private void DrawUI() => WindowSystem.Draw();
    public void ToggleMainUI() => MainWindow.Toggle();
}
