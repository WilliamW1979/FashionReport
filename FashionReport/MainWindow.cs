using Dalamud.Interface.Textures;
using Dalamud.Interface.Textures.TextureWraps;
using Dalamud.Interface.Windowing;
using FFXIVClientStructs.FFXIV.Client.Game;
using ImGuiNET;
using System;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Item = Lumina.Excel.Sheets.Item;
using Dalamud.Memory;
using FFXIVClientStructs.FFXIV.Component.GUI;
using static FFXIVClientStructs.FFXIV.Client.Game.UI.MapMarkerData.Delegates;
using System.Net.Security;
using System.Data;
using Lumina.Data.Parsing.Tex.Buffers;
#pragma warning disable IDE1006
#pragma warning disable CS8602
namespace FashionReport;

public class MAINWINDOW : Window, IDisposable
{
    private readonly FASHIONREPORT FashionReport;
    private DATAMANAGEMENT DataManagement = new DATAMANAGEMENT();
    private DateTime LastChecked = new DateTime(2000, 1, 1, 0, 0, 0);
    private DateTime LastDyeChecked = new DateTime(2000, 1, 1, 0, 0, 0);
    private SortedList<string, InventoryItem> EquippedGear = new SortedList<string, InventoryItem>();
    private readonly SortedList<string, uint> SlotMax = new SortedList<string, uint>()
    {
        { "Weapon", 10 },
        { "Head", 10 },
        { "Body", 10 },
        { "Gloves", 10 },
        { "Legs", 10 },
        { "Boots", 10 },
        { "Earrings", 8 },
        { "Necklace", 8 },
        { "Bracelet", 8 },
        { "RightRing", 8 },
        { "LeftRing", 8 }
    };
    public readonly List<Item> AllItems = SERVICES.DataManager.GetExcelSheet<Item>()?.ToList() ?? new List<Item>();
    private string sState = "Main";

    public MAINWINDOW(FASHIONREPORT fashionReport) : base("Fashion Report", ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse)
    {
        this.FashionReport = fashionReport;

        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(1110, 440),
            MaximumSize = new Vector2(1110, 440)
        };
        
        foreach (string slot in DataManagement.Slots)
            EquippedGear[slot] = GEARMANAGER.GetEquipped(GEARMANAGER.GetEquipSlotCategory(slot));

        Thread GameDataCheck = new Thread(new ThreadStart(GameDataReader));
        Thread DyeCheck = new Thread(new ThreadStart(DatabaseDyeReader));
        GameDataCheck.Start();
        DyeCheck.Start();
    }

    public void DatabaseDyeReader()
    {
        Thread.Sleep(600000);
        while (true)
        {
            if (DataManagement.IsDyes() && DataManagement.CalculateFriday().AddDays(7) > DateTime.Now)
            {
                SERVICES.Log.Info("Dye Thread sleeping until " + DataManagement.CalculateFriday().AddDays(7).ToLocalTime());
                Thread.Sleep(DataManagement.CalculateFriday().AddDays(7) - DateTime.Now);
            }
            if (DataManagement.CalculateFriday() > DateTime.Now)
            {
                SERVICES.Log.Info("Dye Thread sleeping until " + DataManagement.CalculateFriday().ToLocalTime());
                Thread.Sleep(DataManagement.CalculateFriday() - DateTime.Now);
            }
            DataManagement.ReadDataFromServer();
            Thread.Sleep(60000);
        }
    }

    /*
    // To be implimented later
    public unsafe void ThreadReader()
    {
        bool bDyes = false;
        bool bWeek = false;
        TimeSpan Dyes = DataManagement.CalculateFriday().AddDays(7) - DateTime.Now;
        TimeSpan Week = DataManagement.CalculateTuesday().AddDays(7) - DateTime.Now;
        Thread.Sleep(600000);
        DataManagement.ReadDataFromServer();
        while (true)
        {
            if (bDyes && bWeek)
            {
                if (Dyes < Week)
                    Thread.Sleep(Dyes);
                else
                    Thread.Sleep(Week);
            }

            if (!bWeek)
            {
                if (DataManagement.GetCurrentWeek() == DataManagement.Week)
                {
                    bWeek = true;
                    DateTime temp = DataManagement.CalculateTuesday().AddDays(7);
                    SERVICES.Log.Info("Game reading Thread Sleeping until " + temp.ToLocalTime());
                    Thread.Sleep(temp - DateTime.Now);
                }
                else
                {
                    AtkUnitBase* addon = (AtkUnitBase*)SERVICES.GameGui.GetAddonByName("FashionCheck");
                    if ((nint)addon != IntPtr.Zero)
                        if ((((AtkUnitBase*)addon)->RootNode != null && ((AtkUnitBase*)addon)->RootNode->IsVisible()))
                            DataManagement.ReadDataFromGame();
                    Thread.Sleep(5000);
                }
            }
        }
    }
    */

    public unsafe void GameDataReader()
    {
        DataManagement.ReadDataFromServer();
        Thread.Sleep(600000);
        while (true)
        {
            if (DataManagement.GetCurrentWeek() == DataManagement.Week)
            {
                DateTime temp = DataManagement.CalculateTuesday().AddDays(7);
                SERVICES.Log.Info("Game reading Thread Sleeping until " + temp.ToLocalTime());
                Thread.Sleep(temp - DateTime.Now);
            }
            else
            {
                AtkUnitBase* addon = (AtkUnitBase*)SERVICES.GameGui.GetAddonByName("FashionCheck");
                if ((nint)addon != IntPtr.Zero)
                    if ((((AtkUnitBase*)addon)->RootNode != null && ((AtkUnitBase*)addon)->RootNode->IsVisible()))
                        DataManagement.ReadDataFromGame();
                Thread.Sleep(5000);
            }
        }
    }

    public void Reset() => sState = "Main";

    public void Dispose() { }

    public override void Draw()
    {
        if (DataManagement.IsError())
        {
            SERVICES.Log.Info("There was a DataManagement Error, correcting it now!");
            DataManagement.ReadDataFromServer();
        }
        if (sState == "Main")
            MainDraw();
        else
            SlotDraw();
    }

    public void MainDraw()
    {
        SortedList<string, string> SlotThemes = DataManagement.SlotThemes;
        SortedList<string, string> SlotData = DataManagement.SlotData;
        SortedList<string, DYES.DYEINFO> SlotDyes = DataManagement.SlotDyes;
        string WeeklyTheme = DataManagement.WeeklyTheme;
        string[] Slots = DataManagement.Slots;

        foreach (string slot in DataManagement.Slots)
            EquippedGear[slot] = GEARMANAGER.GetEquipped(GEARMANAGER.GetEquipSlotCategory(slot));
        
        /*
        DateTime Now = DateTime.Now;
        if (Now.DayOfWeek != LastChecked.DayOfWeek || Now.Day != LastChecked.Day || LastChecked == new DateTime(2000, 1, 1, 0, 0, 0))
        {
            LastChecked = Now;
            DataManagement.ReadDataFromServer();
            return;
        }
        TimeSpan ts = new TimeSpan(0, 15, 0);
        if ((Now.DayOfWeek == DayOfWeek.Friday || Now.DayOfWeek == DayOfWeek.Saturday || Now.DayOfWeek == DayOfWeek.Sunday || Now.DayOfWeek == DayOfWeek.Monday))
            if (((LastDyeChecked + ts) > Now) && (SlotDyes["Weapon"].DyeId == 0 || SlotDyes["Head"].DyeId == 0 || DataManagement.SlotDyes["Body"].DyeId == 0 || SlotDyes["Gloves"].DyeId == 0 || SlotDyes["Legs"].DyeId == 0 || SlotDyes["Boots"].DyeId == 0))
            {
                SERVICES.Log.Info($"Checking Dyes");
                DataManagement.ReadDataFromServer();
                LastDyeChecked = Now;
                return;
            }

        if (SlotData.Count == 0)
        {
            DataManagement.ReadDataFromServer();
            return;
        }
        */

        ImGui.SetWindowFontScale(3);
        IMGUIFORMAT.TextCenteredColumn(DataManagement.WeeklyTheme);
        ImGui.SetWindowFontScale(1);
        ImGui.Separator();

        ImGui.Columns(6, "Menu", false);
        ImGui.SetColumnWidth(0, 180);
        ImGui.SetColumnWidth(1, 150);
        ImGui.SetColumnWidth(2, 400);
        ImGui.SetColumnWidth(3, 150);
        ImGui.SetColumnWidth(4, 150);
        ImGui.SetColumnWidth(5, 80);
        IMGUIFORMAT.TextCenteredColumn("Slot");
        ImGui.NextColumn();
        IMGUIFORMAT.TextCenteredColumn("Theme");
        ImGui.NextColumn();
        IMGUIFORMAT.TextCenteredColumn("Equipped Item");
        ImGui.NextColumn();
        IMGUIFORMAT.TextCenteredColumn("Dye 1");
        ImGui.NextColumn();
        IMGUIFORMAT.TextCenteredColumn("Dye 2");
        ImGui.NextColumn();
        IMGUIFORMAT.TextCenteredColumn("Points");

        ImGui.Columns(1);
        ImGui.Separator();

        ImGui.Columns(7, "Data", true);
        ImGui.SetColumnWidth(0, 180);
        ImGui.SetColumnWidth(1, 150);
        ImGui.SetColumnWidth(2, 400);
        ImGui.SetColumnWidth(3, 150);
        ImGui.SetColumnWidth(4, 150);
        ImGui.SetColumnWidth(5, 30);
        ImGui.SetColumnWidth(6, 50);

        foreach (string slot in Slots)
        {
            if (slot == "RightRing")
                ImGui.Text("Right Ring");
            else if (slot == "LeftRing")
                ImGui.Text("Left Ring");
            else
                ImGui.Text(slot);

            if (SlotDyes.TryGetValue(slot, out DYES.DYEINFO Dyes) && Dyes.DyeId != 0 && !string.IsNullOrEmpty(Dyes.DyeName))
            {
                ImGui.SameLine();
                ImGui.Text($" ({Dyes.DyeName})");
            }
        }
        ImGui.NextColumn();

        foreach (string slot in Slots)
            if (SlotThemes.TryGetValue(slot, out string? theme) && !string.IsNullOrEmpty(theme))
                ButtonTheme(slot);
            else
                ImGui.NewLine();
        ImGui.NextColumn();

        foreach (string slot in Slots)
            if (EquippedGear.TryGetValue(slot, out InventoryItem item) && item.ItemId != 0)
                if (item.GlamourId > 0)
                    IMGUIFORMAT.TextCenteredColumn(GEARMANAGER.GetGearItem(item.GlamourId).Name.ToString());
                else
                    IMGUIFORMAT.TextCenteredColumn(GEARMANAGER.GetGearItem(item.ItemId).Name.ToString());
            else
                ImGui.NewLine();
        ImGui.NextColumn();

        for (int count = 0; count < 6; count++)
            if (EquippedGear.TryGetValue(Slots[count], out InventoryItem item))
                if (item.ItemId > 0) DYES.PrintDye(item, 0);
        ImGui.NextColumn();

        for (int count = 0; count < 6; count++)
            if (EquippedGear.TryGetValue(Slots[count], out InventoryItem item))
                if (item.ItemId > 0) DYES.PrintDye(item, 1);
        ImGui.NextColumn();

        uint TotalPoints = 0;
        foreach (string slot in Slots)
        {
            uint p = GetSlotPoints(slot) + GetDyePoints(slot);
            TotalPoints += p;

            if (p >= 8)
                ImGui.PushStyleColor(ImGuiCol.Text, ImGui.ColorConvertFloat4ToU32(new System.Numerics.Vector4(0.0f, 0.0f, 1.0f, 1.0f)));
            else if (p >= 2 && p <= 4)
                ImGui.PushStyleColor(ImGuiCol.Text, ImGui.ColorConvertFloat4ToU32(new System.Numerics.Vector4(0.0f, 1.0f, 0.0f, 1.0f)));
            else if (p == 0)
                ImGui.PushStyleColor(ImGuiCol.Text, ImGui.ColorConvertFloat4ToU32(new System.Numerics.Vector4(1.0f, 0.0f, 0.0f, 1.0f)));
            else
                ImGui.PushStyleColor(ImGuiCol.Text, ImGui.ColorConvertFloat4ToU32(new System.Numerics.Vector4(1.0f, 1.0f, 1.0f, 1.0f)));
            IMGUIFORMAT.TextCenteredColumn(p.ToString());
            ImGui.PopStyleColor();
        }
        ImGui.NextColumn();

        foreach (string slot in Slots)
            ImGui.Text($" / {SlotMax[slot]}");
        ImGui.NextColumn();

        ImGui.Columns(1);
        ImGui.Separator();
        ImGui.SetWindowFontScale(4);

        if (TotalPoints > 100) TotalPoints = 100;
        if (TotalPoints < 80)
            ImGui.PushStyleColor(ImGuiCol.Text, ImGui.ColorConvertFloat4ToU32(new System.Numerics.Vector4(1.0f, 0.0f, 0.0f, 1.0f)));
        else if (TotalPoints >= 80 && TotalPoints < 100)
            ImGui.PushStyleColor(ImGuiCol.Text, ImGui.ColorConvertFloat4ToU32(new System.Numerics.Vector4(0.0f, 1.0f, 0.0f, 1.0f)));
        else
            ImGui.PushStyleColor(ImGuiCol.Text, ImGui.ColorConvertFloat4ToU32(new System.Numerics.Vector4(1.0f, 0.84f, 0.0f, 1.0f)));
        IMGUIFORMAT.TextCenteredColumn(TotalPoints.ToString() + " points");
        ImGui.PopStyleColor();

        ImGui.SetWindowFontScale(1);
        IMGUIFORMAT.TextCenteredColumn("Dyes are factored in as bonus points IF next to the Weapon or Armor category it is listed! (Updates after Friday when Historical Data is updated by Scarlet)");
        IMGUIFORMAT.TextCenteredColumn("Black and White dyes may be off due to how the shading system works. Future testing will weed out these problems in a future update.");
        ImGui.Separator();
    }

    public void SlotDraw()
    {
        DrawBackButton();
        ImGui.SetWindowFontScale(2.5f);
        float textWidth = ImGui.CalcTextSize(DataManagement.SlotThemes[sState]).X;
        ImGui.SetCursorPosX((ImGui.GetWindowSize().X - textWidth) / 2);
        ImGui.TextUnformatted(DataManagement.SlotThemes[sState]);
        ImGui.SetWindowFontScale(1);
        ImGui.Separator();

        ImGui.GetStyle().Colors[(int)ImGuiCol.Border] = new Vector4(0, 0, 0, 0);

        float childHeight = ImGui.GetWindowSize().Y - 150;

        ImGui.BeginChild("ItemList", new Vector2(0, childHeight), true, ImGuiWindowFlags.HorizontalScrollbar);

        DataManagement.SlotData.TryGetValue(sState, out string? data);

        if (string.IsNullOrEmpty(data))
        {
            ImGui.Text("There is either no known data");
            ImGui.Text("or a connection problem with the server.");
        }
        else
        {
            string[] items = data.Split('|');
            foreach (string itemname in items)
            {
                Item item = GEARMANAGER.GetGearItem(uint.Parse(itemname));
                uint iSlot = GEARMANAGER.GetEquipSlotCategory(sState) + 1;
                if (iSlot == 13) iSlot = 12;
                if (item.RowId > 0 && item.EquipSlotCategory.RowId == iSlot)
                {
                    DrawItem(item);
                    ImGui.NextColumn();
                }
            }
            ImGui.Dummy(new Vector2(0, 10));
        }
        ImGui.EndChild();
    }

    void DrawBackButton()
    {
        Vector2 Current = ImGui.GetCursorPos();
        string buttonText = "◀ Back";
        Vector2 textSize = ImGui.CalcTextSize(buttonText);
        Vector2 buttonSize = textSize + new Vector2(20f, 10f);
        Vector2 windowSize = ImGui.GetWindowSize();
        float buttonX = windowSize.X - buttonSize.X - 10f;
        float buttonY = windowSize.Y - buttonSize.Y - 10f;
        ImGui.SetCursorPosX(buttonX);
        ImGui.SetCursorPosY(buttonY);

        if (ImGui.ArrowButton("Back", ImGuiDir.Left))
            sState = "Main";
        ImGui.SameLine();
        ImGui.SetWindowFontScale(1);
        ImGui.Text("Back");
        ImGui.GetStyle().Colors[(int)ImGuiCol.Border] = new Vector4(0.32f, 0.32f, 0.32f, 1f);
        ImGui.SetCursorPos(Current);
    }

    void DrawItem(Item item)
    {
        if (item.RowId == 0) return;
        ImGui.Columns(3, "DrawItems", false);
        ImGui.SetColumnWidth(0, 80);
        ImGui.SetColumnWidth(1, 500);
        ImGui.SetColumnWidth(2, ImGui.GetWindowSize().X - 580);
        Vector2 iconSize = new(ImGui.GetTextLineHeight() * 2f);
        iconSize = iconSize * 1.5f;
        if(item.RowId > 0)
            if (SERVICES.TextureProvider.GetFromGameIcon(new GameIconLookup { IconId = item.Icon }).TryGetWrap(out IDalamudTextureWrap? icon, out Exception? ex))
                if (icon != null)
                    ImGui.Image(icon.ImGuiHandle, iconSize);
        ImGui.NextColumn();

        Vector2 itemNameSize = ImGui.CalcTextSize(item.Name.ExtractText());
        ImGui.SetWindowFontScale(2.0f);
        ImGui.Text(item.Name.ExtractText());
        ImGui.SetWindowFontScale(1f);
        ImGui.Text("Level: " + item.LevelEquip.ToString());
        ImGui.NextColumn();
        ImGui.BeginGroup();

        float X = ImGui.GetCursorPosX();
        float Y = ImGui.GetCursorPosY();

        List<(uint IconId, bool ShouldDisplay)> iconsToDisplay = new()
        {
            (61432, GEARMANAGER.ItemInfo[item.RowId].IsQuestReward),
            (60831, GEARMANAGER.ItemInfo[item.RowId].IsDuty),
            (60412, GEARMANAGER.ItemInfo[item.RowId].IsVender),
            (60434, GEARMANAGER.IsCraftable(item))
        };

        uint displayedIcons = 0;
        foreach ((uint iconId, bool shouldDisplay) in iconsToDisplay)
        {
            if (!shouldDisplay) continue;
            if (SERVICES.TextureProvider.GetFromGameIcon(new GameIconLookup { IconId = iconId }).TryGetWrap(out IDalamudTextureWrap? icon, out Exception? ex))
                if (icon != null)
                {
                    displayedIcons++;
                    ImGui.SetCursorPosX((ImGui.GetWindowWidth() - (iconSize.X * displayedIcons)) - 40);
                    ImGui.SetCursorPosY(Y);
                    ImGui.Image(icon.ImGuiHandle, iconSize);
                }
        }
        ImGui.Columns();
    }

    void ButtonTheme(string slot)
    {
        DataManagement.SlotThemes.TryGetValue(slot, out string? Theme);
        Vector2 vTextSize = ImGui.CalcTextSize(Theme);
        float fButtonWidth = vTextSize.X + ImGui.GetStyle().FramePadding.X * 2.0f;
        float fColunWidth = ImGui.GetContentRegionAvail().X;
        float fOffsetX = (fColunWidth - fButtonWidth) / 2.0f;
        ImGui.SetCursorPosX(ImGui.GetCursorPosX() + fOffsetX);
        if (ImGui.InvisibleButton("##" + Theme, vTextSize))
            sState = slot;
        Vector2 vButtonPos = ImGui.GetItemRectMin();
        ImDrawListPtr drawlist = ImGui.GetWindowDrawList();
        uint iBlueColor = ImGui.ColorConvertFloat4ToU32(new Vector4(0.0f, 0.5f, 1.0f, 1.0f));

        Vector2 vUnderlineStart = new Vector2(vButtonPos.X, vButtonPos.Y + vTextSize.Y + 1);
        Vector2 vUnderlineEnd = new Vector2(vButtonPos.X + vTextSize.X, vButtonPos.Y + vTextSize.Y + 1);
        drawlist.AddLine(vUnderlineStart, vUnderlineEnd, iBlueColor, 1.0f);
        ImGui.GetWindowDrawList().AddText(vButtonPos, ImGui.ColorConvertFloat4ToU32(new Vector4(0.0f, 0.5f, 1.0f, 1.0f)), Theme);
    }

    private uint GetSlotPoints(string slot)
    {
        EquippedGear.TryGetValue(slot, out InventoryItem item);
        DataManagement.SlotThemes.TryGetValue(slot, out string? Theme);
        DataManagement.SlotData.TryGetValue(slot, out string? Data);

        if (item.ItemId == 0) return 0;
        uint ID = item.ItemId;
        if (item.GlamourId != 0)
            ID = item.GlamourId;
        if (string.IsNullOrEmpty(Theme) && string.IsNullOrEmpty(Data))
            return SlotMax[slot];
        if ((string.IsNullOrEmpty(Theme) && !string.IsNullOrEmpty(Data)) || (!string.IsNullOrEmpty(Theme) && string.IsNullOrEmpty(Data)))
        {
            DataManagement.ReadDataFromServer();
            return 1;
        }
        if (Data != null)
        {
            string[] Gears = Data.Split('|');
            foreach (string Gear in Gears)
                if (ID == uint.Parse(Gear))
                    return SlotMax[slot];
            return 2;
        }
        return 50; // We have an error if we get here!
    }

    private uint GetDyePoints(string slot)
    {
        InventoryItem item = EquippedGear[slot];
        SortedList<string, DYES.DYEINFO> SlotDyes = DataManagement.SlotDyes;
        if (slot == "Weapons" || slot == "Head" || slot == "Body" || slot == "Gloves" || slot == "Legs" || slot == "Boots")
        {
            SlotDyes.TryGetValue(slot, out DYES.DYEINFO Dye);
            if (Dye.DyeId == 0) return 0;
            return DYES.CompareColors(SlotDyes[slot], DYES.GetDyeInfo(item.GetStain(0)), DYES.GetDyeInfo(item.GetStain(1)));
        }
        return 0;
    }
}
