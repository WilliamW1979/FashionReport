using Dalamud.Interface.Windowing;
using Dalamud.Memory;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Component.GUI;
using ImGuiNET;
using Lumina.Excel.Sheets;
using System;
using System.Numerics;

namespace FashionReport.Windows;

public class MAINWINDOW : Window, IDisposable
{
    private readonly FASHIONREPORT FashionReport;
    private CONFIGURATION Configuration;
    private bool bToggle;
    private STATE eState;
    private uint iSlot;
    private string sCurrentTheme;
    private string sCurrentThemeData;

    public MAINWINDOW(FASHIONREPORT fashionReport) : base("Fashion Report", ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse)
    {
        this.FashionReport = fashionReport;
        Configuration = FashionReport.Configuration;
        bToggle = false;
        sCurrentTheme = "";
        sCurrentThemeData = "";
        eState = STATE.Main;
        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(375, 330),
            MaximumSize = new Vector2(float.MaxValue, float.MaxValue)
        };
    }

    public void Dispose() {}

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
        else if (eState == STATE.Main && Configuration.bCurrent)
            MainDraw();
        else if (eState == STATE.Slot && Configuration.bCurrent)
            SlotDraw();
    }

    public void SlotDraw()
    {
        if (ImGui.ArrowButton("Back", ImGuiDir.Left))
        {
            iSlot = 0;
            sCurrentTheme = "";
            sCurrentThemeData = "";
            eState = STATE.Main;
        }
        ImGui.SameLine();
        ImGui.Text("Go Back");
        ImGui.Separator();
        ImGui.SetWindowFontScale(2);
        TextCentered(sCurrentTheme);
        ImGui.SetWindowFontScale(1);
        ImGui.Separator();
        string[] items = sCurrentThemeData.Split('|');
        foreach (string item in items)
        {
            Item iItem = FashionReport.GearManager.GetGearItem(uint.Parse(item));
            if (iItem.RowId > 0 && iItem.EquipSlotCategory.RowId == iSlot) // TextCentered(iItem.Name.ToString());
                FashionReport.GearManager.DrawItem(iItem);
        }
    }

    public void MainDraw()
    { 
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

        if (Configuration.sWeapon == "") ImGui.NewLine(); else ButtonTheme(Configuration.sWeapon, Configuration.sWeaponData, 1);
        if (Configuration.sHead == "") ImGui.NewLine(); else ButtonTheme(Configuration.sHead, Configuration.sHeadData, 3);
        if (Configuration.sBody == "") ImGui.NewLine(); else ButtonTheme(Configuration.sBody, Configuration.sBodyData, 4);
        if (Configuration.sGloves == "") ImGui.NewLine(); else ButtonTheme(Configuration.sGloves, Configuration.sGlovesData, 5);
        if (Configuration.sLegs == "") ImGui.NewLine(); else ButtonTheme(Configuration.sLegs, Configuration.sLegsData, 7);
        if (Configuration.sBoots == "") ImGui.NewLine(); else ButtonTheme(Configuration.sBoots, Configuration.sBootsData, 8);
        if (Configuration.sEarrings == "") ImGui.NewLine(); else ButtonTheme(Configuration.sEarrings, Configuration.sEarringsData, 9);
        if (Configuration.sNecklace == "") ImGui.NewLine(); else ButtonTheme(Configuration.sNecklace, Configuration.sNecklaceData, 10);
        if (Configuration.sBracelet == "") ImGui.NewLine(); else ButtonTheme(Configuration.sBracelet, Configuration.sBraceletData, 11);
        if (Configuration.sRightRing == "") ImGui.NewLine(); else ButtonTheme(Configuration.sRightRing, Configuration.sRightRingData, 12);
        if (Configuration.sLeftRing == "") ImGui.NewLine(); else ButtonTheme(Configuration.sLeftRing, Configuration.sLeftRingData, 13);

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

    void ButtonTheme(string sTheme, string sThemeData, uint uiSlot)
    {
        Vector2 vTextSize = ImGui.CalcTextSize(sTheme);
        float fButtonWidth = vTextSize.X + ImGui.GetStyle().FramePadding.X * 2.0f;
        float fColunWidth = ImGui.GetContentRegionAvail().X;
        float fOffsetX = (fColunWidth - fButtonWidth) / 2.0f;
        ImGui.SetCursorPosX(ImGui.GetCursorPosX() + fOffsetX);
        if (ImGui.InvisibleButton("##" + sTheme, vTextSize))
        {
            eState = STATE.Slot;
            sCurrentTheme = sTheme;
            sCurrentThemeData = sThemeData;
            iSlot = uiSlot;
        }   
        Vector2 vButtonPos = ImGui.GetItemRectMin();
        ImDrawListPtr drawlist = ImGui.GetWindowDrawList();
        uint iBlueColor = ImGui.ColorConvertFloat4ToU32(new Vector4(0.0f, 0.5f, 1.0f, 1.0f));

        Vector2 vUnderlineStart = new Vector2(vButtonPos.X, vButtonPos.Y + vTextSize.Y + 1); // Slight offset below the text
        Vector2 vUnderlineEnd = new Vector2(vButtonPos.X + vTextSize.X, vButtonPos.Y + vTextSize.Y + 1);
        drawlist.AddLine(vUnderlineStart, vUnderlineEnd, iBlueColor, 1.0f); // Line thickness of 1.0f
        ImGui.GetWindowDrawList().AddText(vButtonPos, ImGui.ColorConvertFloat4ToU32(new Vector4(0.0f, 0.5f, 1.0f, 1.0f)), sTheme);
    }

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
        AtkUnitBase* addon = (AtkUnitBase*)FASHIONREPORT.GameGui.GetAddonByName("FashionCheck");
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

    private enum STATE { Main, Slot };
}
