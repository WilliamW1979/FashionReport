using Dalamud.Interface.Textures;
using Dalamud.Interface.Textures.TextureWraps;
using Dalamud.Interface.Windowing;
using FFXIVClientStructs.FFXIV.Client.Game;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Item = Lumina.Excel.Sheets.Item;
#pragma warning disable IDE1006

namespace FashionReport;

public class DYEWINDOW : Window, IDisposable
{
    private readonly FASHIONREPORT FashionReport;

    public DYEWINDOW(FASHIONREPORT fashionReport) : base("Dye List", ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse)
    {
        this.FashionReport = fashionReport;

        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(1110, 440),
            MaximumSize = new Vector2(1110, 440)
        };
    }

    public void Dispose() { }

    public override void Draw()
    {

    }
}
