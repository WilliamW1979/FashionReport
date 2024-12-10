using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.Game;
using ImGuiNET;
using System;
using System.Numerics;

namespace FashionReport.Windows;

public class MAINWINDOW : Window, IDisposable
{
//    private string? WolfPawImagePath;
    private FASHIONREPORT FashionReport;
    private Configuration configuration;
    
    private WEEK Week;

    public MAINWINDOW(FASHIONREPORT fashionReport, string WolfPawImagePath) : base("Fashion Report", ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse)
    {
        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(375, 330),
            MaximumSize = new Vector2(float.MaxValue, float.MaxValue)
        };
        this.FashionReport = fashionReport;
        configuration = FashionReport.Configuration;
        Week = new WEEK(FashionReport);
    }

    public void Dispose() { }
    
    public unsafe class Equip
    {
        public InventoryManager* inventoryManager;
        public InventoryContainer* equipmentContainer;
        public InventoryItem* equipmentInventoryItem;

        public readonly uint[] uiid;
        public readonly short[] shSlot;

        public Equip()
        {
            inventoryManager = InventoryManager.Instance();
            equipmentContainer = inventoryManager->GetInventoryContainer(InventoryType.EquippedItems);
            equipmentInventoryItem = equipmentContainer->GetInventorySlot(0);
            uiid = new uint[14];
            shSlot = new short[14];
        }

        public void Check()
        { 
            for(int i = 0;i<14;i++)
            {
                equipmentInventoryItem = equipmentContainer->GetInventorySlot(i);
                uiid[i] = equipmentInventoryItem->GetItemId();
                shSlot[i] = equipmentInventoryItem->Slot;
            }
        }

        public string ToString(int i)
        {
            if (i >= 14 || i < 0) return "";
            return ("ID: " + (uiid[i] % 100000).ToString() + " | Slot: " + shSlot[i].ToString());
        }
    }
    public unsafe uint GetEquipped(int x)
    {
        InventoryItem* equipmentInventoryItem;
        if (x < 0 || x > 13) return 0;
        equipmentInventoryItem = InventoryManager.Instance()->GetInventorySlot(InventoryType.EquippedItems, x);
        return (equipmentInventoryItem->GetItemId() % 100000);
    }

    private uint CheckSlot(string Slot, uint item)
    {
        if (Week.Themes[Slot] == "")
        {
            if(item != 0)
                return Week.GetMax(Slot);
            else
                return 0;
        }
        else
        {
            if (item == 0)
                return 0;
            else if (Week.IsGold(item, Week.Themes[Slot]))
                return 10;
            return 2;
        }
    }

    public unsafe override void Draw()
    {
        Week.Check();
        if (!Week.Current)
        {
            TextCentered("Fashion Report Data not current!");
            TextCentered("Please update data by talking to Masked Rose at Gold Saucer.");
            return;
        }

        uint[] items = new uint[11];
        uint[] points = new uint[11];
        for (int i = 0; i < points.Length; i++)
        {
            int t = i;
            if (t >= 1 && t < 4) t++;
            else if (t >= 4) t += 2;
            items[i] = GetEquipped(t);
        }

        uint Points = 0;
        foreach (ItemSlot slot in Enum.GetValues<ItemSlot>())
            points[(int)slot] = CheckSlot(slot.ToString(), items[(int)slot]);
        foreach (uint p in points)
            Points += p;

        ImGui.SetWindowFontScale(2);
        TextCentered(Week.WeeklyTheme);
        ImGui.SetWindowFontScale(1);
        ImGui.Separator();
        ImGui.Columns(4);
        ImGui.SetColumnWidth(0, 80);
        ImGui.SetColumnWidth(1, 200);
        ImGui.SetColumnWidth(2, 30);
        ImGui.SetColumnWidth(3, 40);
        ImGui.Text("Weapon"); // Weapon
        ImGui.Text("Head"); // Head
        ImGui.Text("Body"); // Body
        ImGui.Text("Gloves"); // Gloves
        ImGui.Text("Legs"); // Legs
        ImGui.Text("Boots"); // Boots
        ImGui.Text("Earrings"); // Earrings
        ImGui.Text("Necklace"); // Necklace
        ImGui.Text("Bracelet");// Bracelet
        ImGui.Text("Right Ring");// R Ring
        ImGui.Text("Left Ring");// L Ring
        ImGui.NextColumn();
        TextCentered(Week.Themes["Weapon"].Trim());
        TextCentered(Week.Themes["Head"].Trim());
        TextCentered(Week.Themes["Body"]);
        TextCentered(Week.Themes["Gloves"]);
        TextCentered(Week.Themes["Legs"]);
        TextCentered(Week.Themes["Boots"]);
        TextCentered(Week.Themes["Earrings"]);
        TextCentered(Week.Themes["Necklace"]);
        TextCentered(Week.Themes["Bracelet"]);
        TextCentered(Week.Themes["RightRing"]);
        TextCentered(Week.Themes["LeftRing"]);
        ImGui.NextColumn();
        foreach (int p in points)
            TextCentered(p.ToString());
        ImGui.NextColumn();
        for (int p = 0; p < 6; p++)
            ImGui.Text(" / 10");
        for (int p = 0; p < 5; p++)
            ImGui.Text(" /  8");
        ImGui.Columns(1);
        ImGui.Separator();
        ImGui.SetWindowFontScale(4);
        TextCentered(Points.ToString() + " points");
        ImGui.SetWindowFontScale(1);
    }

    void TextCentered(string text)
    {
        ImGui.SetCursorPosX(((ImGui.GetColumnWidth() - ImGui.CalcTextSize(text).X) * 0.5f) + ImGui.GetColumnOffset());
        ImGui.Text(text);
    }
}
