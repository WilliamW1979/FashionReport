using Dalamud.Interface.Windowing;
using Dalamud.Memory;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Component.GUI;
using ImGuiNET;
using System;
using System.Numerics;

namespace FashionReport.Windows;

public class MAINWINDOW : Window, IDisposable
{
//    private string? WolfPawImagePath;
    private FASHIONREPORT FashionReport;
    private CONFIGURATION Configuration;
    private bool bToggle;
    
    public MAINWINDOW(FASHIONREPORT fashionReport) : base("Fashion Report", ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse)
    {
        this.FashionReport = fashionReport;
        Configuration = FashionReport.Configuration;
        bToggle = false;

        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(375, 330),
            MaximumSize = new Vector2(float.MaxValue, float.MaxValue)
        };

    }

    public void Dispose() { }

    void TextCentered(string text)
    {
        ImGui.SetCursorPosX(((ImGui.GetColumnWidth() - ImGui.CalcTextSize(text).X) * 0.5f) + ImGui.GetColumnOffset());
        ImGui.Text(text);
    }
    
    public unsafe uint GetEquipped(int x)
    {
        InventoryItem* equipmentInventoryItem;
        if (x < 0 || x > 13) return 0;
        equipmentInventoryItem = InventoryManager.Instance()->GetInventorySlot(InventoryType.EquippedItems, x);
        return (equipmentInventoryItem->GetItemId() % 100000);
    }

    public unsafe override void Draw()
    {
        Check();
        if (!Configuration.bCurrent)
        {
            TextCentered("Fashion Report Data not current!");
            TextCentered("Please update data by talking to Masked Rose at Gold Saucer.");
            TextCentered("Choose: \"Confirm this week's challenge\"");
            ImGui.NewLine();
            TextCentered("This loads the data for the week.");
            return;
        }

        uint[] items = new uint[11];
        for (int i = 0; i < items.Length; i++)
        {
            int t = i;
            if (t >= 1 && t < 4) t++;
            else if (t >= 4) t += 2;
            items[i] = GetEquipped(t);
        }

        ImGui.SetWindowFontScale(2);
        TextCentered(Configuration.sWeeklyTheme);
        ImGui.SetWindowFontScale(1);
        ImGui.Separator();
        ImGui.Columns(4);
        ImGui.SetColumnWidth(0, 80);
        ImGui.SetColumnWidth(1, 200);
        ImGui.SetColumnWidth(2, 30);
        ImGui.SetColumnWidth(3, 40);
        ImGui.Text("Weapon");
        ImGui.Text("Head");
        ImGui.Text("Body");
        ImGui.Text("Gloves");
        ImGui.Text("Legs");
        ImGui.Text("Boots");
        ImGui.Text("Earrings");
        ImGui.Text("Necklace");
        ImGui.Text("Bracelet");
        ImGui.Text("Right Ring");
        ImGui.Text("Left Ring");
        ImGui.NextColumn();
        TextCentered(Configuration.sWeapon);
        TextCentered(Configuration.sHead);
        TextCentered(Configuration.sBody);
        TextCentered(Configuration.sGloves);
        TextCentered(Configuration.sLegs);
        TextCentered(Configuration.sBoots);
        TextCentered(Configuration.sEarrings);
        TextCentered(Configuration.sNecklace);
        TextCentered(Configuration.sBracelet);
        TextCentered(Configuration.sRightRing);
        TextCentered(Configuration.sLeftRing);
        ImGui.NextColumn();

        uint[] SlotPoints = new uint[11];

        SlotPoints[0] = GetSlotPoints(GetEquipped(0), Configuration.sWeapon, Configuration.sWeaponData);
        SlotPoints[1] = GetSlotPoints(GetEquipped(2), Configuration.sHead, Configuration.sHeadData);
        SlotPoints[2] = GetSlotPoints(GetEquipped(3), Configuration.sBody, Configuration.sBodyData);
        SlotPoints[3] = GetSlotPoints(GetEquipped(4), Configuration.sGloves, Configuration.sGlovesData);
        SlotPoints[4] = GetSlotPoints(GetEquipped(6), Configuration.sLegs, Configuration.sLegsData);
        SlotPoints[5] = GetSlotPoints(GetEquipped(7), Configuration.sBoots, Configuration.sBootsData);
        SlotPoints[6] = GetSlotPoints(GetEquipped(8), Configuration.sEarrings, Configuration.sEarringsData);
        SlotPoints[7] = GetSlotPoints(GetEquipped(9), Configuration.sNecklace, Configuration.sNecklaceData);
        SlotPoints[8] = GetSlotPoints(GetEquipped(10), Configuration.sBracelet, Configuration.sBraceletData);
        SlotPoints[9] = GetSlotPoints(GetEquipped(11), Configuration.sRightRing, Configuration.sRightRingData);
        SlotPoints[10] = GetSlotPoints(GetEquipped(12), Configuration.sLeftRing, Configuration.sLeftRingData);

        for (uint i = 6 ; i < SlotPoints.Length; i++)
            if(SlotPoints[i] == 10)
                SlotPoints[i] = 8;

        uint Points = 0;
        foreach (uint p in SlotPoints)
        {
            TextCentered(p.ToString());
            Points += p;
        }
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

    private uint GetSlotPoints(uint item, string slot, string data)
    {
        if (item == 0) return 0;
        if (slot == "") return 10;
        if (data == "" && slot != "") return 5;
        string[] Gears = data.Split('|');
        foreach (string Gear in Gears)
        {
            if (item == uint.Parse(Gear))
                return 10;
        }
        return 2;
    }

    public unsafe void Check()
    {
        Configuration.bCurrent = (WeeklyReset(DateTime.Now) == WeeklyReset(Configuration.dtLastChecked));
#pragma warning disable CS8602
        AtkUnitBase* addon = (AtkUnitBase*)FashionReport.GameGui.GetAddonByName("FashionCheck");
#pragma warning restore CS8602
        if ((nint)addon != IntPtr.Zero)
        { 
            if ((((AtkUnitBase*)addon)->RootNode != null && ((AtkUnitBase*)addon)->RootNode->IsVisible()) && bToggle == false)
            {
                if (Configuration.bCurrent == false) Configuration.Reset();
                Configuration.dtLastChecked = DateTime.Now;
                Configuration.sWeeklyTheme = MemoryHelper.ReadSeStringNullTerminated((nint)addon->AtkValues[0].String).TextValue;
                Configuration.sWeapon = MemoryHelper.ReadSeStringNullTerminated((nint)addon->AtkValues[2].String).TextValue;
                Configuration.sHead = MemoryHelper.ReadSeStringNullTerminated((nint)addon->AtkValues[13].String).TextValue;
                Configuration.sBody = MemoryHelper.ReadSeStringNullTerminated((nint)addon->AtkValues[24].String).TextValue;
                Configuration.sGloves = MemoryHelper.ReadSeStringNullTerminated((nint)addon->AtkValues[35].String).TextValue;
                Configuration.sLegs = MemoryHelper.ReadSeStringNullTerminated((nint)addon->AtkValues[46].String).TextValue;
                Configuration.sBoots = MemoryHelper.ReadSeStringNullTerminated((nint)addon->AtkValues[57].String).TextValue;
                Configuration.sEarrings = MemoryHelper.ReadSeStringNullTerminated((nint)addon->AtkValues[68].String).TextValue;
                Configuration.sNecklace = MemoryHelper.ReadSeStringNullTerminated((nint)addon->AtkValues[79].String).TextValue;
                Configuration.sBracelet = MemoryHelper.ReadSeStringNullTerminated((nint)addon->AtkValues[90].String).TextValue;
                Configuration.sRightRing = MemoryHelper.ReadSeStringNullTerminated((nint)addon->AtkValues[101].String).TextValue;
                Configuration.sLeftRing = MemoryHelper.ReadSeStringNullTerminated((nint)addon->AtkValues[112].String).TextValue;
                Configuration.sChancesLeft = MemoryHelper.ReadSeStringNullTerminated((nint)addon->AtkValues[122].String).TextValue;
                Configuration.sHighScore = MemoryHelper.ReadSeStringNullTerminated((nint)addon->AtkValues[123].String).TextValue;
                Configuration.Populate();
                Configuration.Save();
                bToggle = true;
            }
            else if (!(((AtkUnitBase*)addon)->RootNode != null && ((AtkUnitBase*)addon)->RootNode->IsVisible()))
                bToggle = false;
        }
    }

    private DateTime WeeklyReset(DateTime time)
    {
        time = time.ToUniversalTime().AddHours(-8);
        int offset = ((int)time.DayOfWeek) - ((int)DayOfWeek.Tuesday);
        if (offset < 0) offset += 7;
        time = time.AddDays(-offset);
        return new DateTime(time.Year, time.Month, time.Day, 0, 0, 0);
    }
}
