using Dalamud.Interface.Windowing;
using Dalamud.Memory;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Component.GUI;
using ImGuiNET;
using System;
using System.Numerics;

namespace FashionReport.Windows;

public class SLOTWINDOW : Window, IDisposable
{
    private FASHIONREPORT FashionReport;
    private CONFIGURATION Configuration;
        
    public SLOTWINDOW(FASHIONREPORT fashionReport) : base("Fashion Report", ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse)
    {
        this.FashionReport = fashionReport;
        Configuration = FashionReport.Configuration;

        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(375, 330),
            MaximumSize = new Vector2(float.MaxValue, float.MaxValue)
        };
    }

    public override void Draw()
    { 

    }

    public void Dispose()
    {

    }
}
