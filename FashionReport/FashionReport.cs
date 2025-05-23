using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dalamud.Game.Command;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin;
using Lumina.Excel.Sheets;

namespace FashionReport;

public sealed class FASHIONREPORT : IDalamudPlugin
{
    public readonly WindowSystem WindowSystem = new("Fashion Report");
    private MAINWINDOW MainWindow { get; init; } = new();
    private FashionCheckDetector? FashionCheckDetector;

    public FASHIONREPORT(IDalamudPluginInterface pluginInterface)
    {
        pluginInterface.Create<SERVICES>();
        WindowSystem.AddWindow(MainWindow);
        SERVICES.FRData = new();
        SERVICES.Equipment = new();
        SERVICES.CommandManager.AddHandler("/fr", new CommandInfo(OnCommand) { HelpMessage = "Fashion Report calculator!" });
        SERVICES.CommandManager.AddHandler("/fashionreport", new CommandInfo(OnCommand) { HelpMessage = "Fashion Report calculator!" });
        SERVICES.Interface.UiBuilder.Draw += DrawUI;
        SERVICES.Interface.UiBuilder.OpenMainUi += ToggleMainUI;
        SERVICES.AllItems = SERVICES.DataManager.GetExcelSheet<Item>()?.ToList() ?? new List<Item>();
        FashionCheckDetector = new();
        if (SERVICES.FRData.StoredWeek == 0)
        {;
            WeeklyFashionReportData? data = FashionCheckDetector.GetServerData();
            if (data != null)
                SERVICES.FRData = data;
        }

    }

    public void Dispose()
    {
        WindowSystem.RemoveAllWindows();
        MainWindow.Dispose();
        SERVICES.CommandManager.RemoveHandler("/fr");
        SERVICES.CommandManager.RemoveHandler("/fashionreport");
        FashionCheckDetector?.Dispose();
    }

    private void OnCommand(string command, string args) => ToggleMainUI();
    private void DrawUI() => WindowSystem.Draw();
    public void ToggleMainUI() => MainWindow.Toggle();
}
