using Dalamud.Interface.Textures;
using Dalamud.Interface.Windowing;
using Dalamud.Memory;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Component.GUI;
using ImGuiNET;
using Lumina.Excel.Sheets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
namespace FashionReport;

using Dalamud.Interface.Utility;
using FFXIVClientStructs.FFXIV.Client.Graphics.Scene;
using ImGuiScene;
using Org.BouncyCastle.Asn1.X509;
using System.Drawing;
using System.Runtime.InteropServices;

public class HELP : Window, IDisposable
{
    FASHIONREPORT FashionReport;

    public HELP(FASHIONREPORT fashionReport) : base("Help Window", ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse)
    {
        this.FashionReport = fashionReport;
        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(1110, 440),
            MaximumSize = new Vector2(1110, 440)
        };
    }

    public void Dispose() { }

    public unsafe override void Draw()
    {

    }
}
