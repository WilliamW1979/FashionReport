using System;
using System.Numerics;
using Dalamud.Interface.Windowing;
using ImGuiNET;

namespace FashionReport.Windows;

public class CONFIGWINDOW : Window, IDisposable
{
    private Configuration Configuration;

    // We give this window a constant ID using ###
    // This allows for labels being dynamic, like "{FPS Counter}fps###XYZ counter window",
    // and the window ID will always be "###XYZ counter window" for ImGui
    public CONFIGWINDOW(FASHIONREPORT FashionReport) : base("Configuration Window###With a constant ID")
    {
        Flags = ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoScrollbar |
                ImGuiWindowFlags.NoScrollWithMouse;

        Size = new Vector2(232, 90);
        SizeCondition = ImGuiCond.Always;

        Configuration = FashionReport.Configuration;
    }

    public void Dispose() { }

    public override void PreDraw()
    {
        Flags &= ~ImGuiWindowFlags.NoMove;
    }

    public override void Draw()
    {
        // can't ref a property, so use a local copy
        var configValue = Configuration.SomePropertyToBeSavedAndWithADefault;
        if (ImGui.Checkbox("Random Config Bool", ref configValue))
        {
            Configuration.SomePropertyToBeSavedAndWithADefault = configValue;
            // can save immediately on change, if you don't want to provide a "Save and Close" button
            Configuration.Save();
        }

        var movable = Configuration.IsConfigWindowMovable;
        if (ImGui.Checkbox("Movable Config Window", ref movable))
        {
            Configuration.IsConfigWindowMovable = movable;
            Configuration.Save();
        }
/*
        ImGui.Text("Weapon: " + GetEquipped(0)); // Weapon
        ImGui.Text("Secondard: " + GetEquipped(1)); // Secondary
        ImGui.Text("Head: " + GetEquipped(2)); // Head
        ImGui.Text("Body: " + GetEquipped(3)); // Body
        ImGui.Text("Gloves: " + GetEquipped(4)); // Gloves
        ImGui.Text("Belt: " + GetEquipped(5));
        ImGui.Text("Legs: " + GetEquipped(6)); // Legs
        ImGui.Text("Boots: " + GetEquipped(7)); // Boots
        ImGui.Text("Earrings: " + GetEquipped(8)); // Earrings
        ImGui.Text("Necklace: " + GetEquipped(9)); // Necklace
        ImGui.Text("Bracelet: " + GetEquipped(10));// Bracelet
        ImGui.Text("Right Ring: " + GetEquipped(11));// R Ring
        ImGui.Text("Left Ring: " + GetEquipped(12));// L Ring
        ImGui.Text("Soul Stone: " + GetEquipped(13));// Soul Stone
*/
    }
}
