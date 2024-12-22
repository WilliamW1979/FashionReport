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
namespace FashionReport.Windows;

using Dalamud.Interface.Utility;
using FFXIVClientStructs.FFXIV.Client.Graphics.Scene;
using ImGuiScene;
using System.Drawing;
using System.Runtime.InteropServices;

public class MAINWINDOW : Window, IDisposable
{
    private readonly FASHIONREPORT FashionReport;
    private CONFIGURATION Configuration;
    private bool bToggle;
    private STATE eState;
    private uint iSlot;
    private string sCurrentTheme;
    private string sCurrentThemeData;
    private DateTime Last;

    private List<Item> lAllItems;


    public MAINWINDOW(FASHIONREPORT fashionReport) : base("Fashion Report", ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse)
    {
        this.FashionReport = fashionReport;
        Configuration = FashionReport.Configuration;
        bToggle = false;
        sCurrentTheme = "";
        sCurrentThemeData = "";
        eState = STATE.Main;
        Last = DateTime.Now;
        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(1110, 440),
            MaximumSize = new Vector2(1110, 440)
        };
        lAllItems = SERVICES.DataManager.GetExcelSheet<Item>()?.ToList() ?? new List<Item>();
        Configuration.Populate();
        Configuration.PopulateDyes();
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
        // Title at the top, bold and larger font, centered
        ImGui.SetWindowFontScale(2.5f);
        float textWidth = ImGui.CalcTextSize(sCurrentTheme).X;
        ImGui.SetCursorPosX((ImGui.GetWindowSize().X - textWidth) / 2); // Center the title
        ImGui.TextUnformatted(sCurrentTheme);
        ImGui.SetWindowFontScale(1);

        // Separator line after the title
        ImGui.Separator();

        // Set the border color to transparent
        ImGui.GetStyle().Colors[(int)ImGuiCol.Border] = new Vector4(0, 0, 0, 0); // Transparent border

        // Scrollable item list with vertical scrollbar
        // BeginChild with specific size limits to allow scrolling
        float childHeight = ImGui.GetWindowSize().Y - 150; // Adjust the height of the scrollable area (leave space for title and button)

        // Use ImGuiWindowFlags for horizontal scrollbar and transparent border
        ImGui.BeginChild("ItemList", new Vector2(0, childHeight), true, ImGuiWindowFlags.HorizontalScrollbar);

        string[] items = sCurrentThemeData.Split('|');
        if (items.Length == 0)
        {
            ImGui.Text("There is either no known data");
            ImGui.Text("Or a connection problem with the server.");
        }

        foreach (string item in items)
        {
            Item iItem = FashionReport.GearManager.GetGearItem(uint.Parse(item));
            if (iItem.RowId > 0 && iItem.EquipSlotCategory.RowId == iSlot)
            {
                // Draw the item in the current row (this calls the separate DrawItem function)
                DrawItem(iItem);

                // Move to the next row in the columns layout
                ImGui.NextColumn(); // Move to next column for the next item
            }
        }

        // End the child (scrollable area)
        ImGui.EndChild();

        // Define the back button text with the left arrow symbol
        string buttonText = "◀ Back";

        // Calculate the size of the button
        Vector2 textSize = ImGui.CalcTextSize(buttonText);
        Vector2 buttonSize = textSize + new Vector2(20f, 10f); // Padding around the button

        // Get window size to calculate position for the button
        Vector2 windowSize = ImGui.GetWindowSize();
        float buttonX = windowSize.X - buttonSize.X - 10f; // 10px padding from the right side
        float buttonY = windowSize.Y - buttonSize.Y - 10f; // 10px padding from the bottom

        // Set cursor position for the button
        ImGui.SetCursorPosX(buttonX);
        ImGui.SetCursorPosY(buttonY);

        // Create the Back button with left arrow and text
        if (ImGui.ArrowButton("Back", ImGuiDir.Left))
        {
            // Action when button is pressed (Back)
            iSlot = 0;
            sCurrentTheme = "";
            sCurrentThemeData = "";
            eState = STATE.Main;
        }
        ImGui.SameLine();
        ImGui.SetWindowFontScale(1);
        ImGui.Text("Back");

        // Reset the border color to default after the child window
        ImGui.GetStyle().Colors[(int)ImGuiCol.Border] = new Vector4(0.32f, 0.32f, 0.32f, 1f); // Default border color
    }

    void DrawItem(Item iItem)
    {
        // Column 1 (Icon)
        ImGui.BeginGroup(); // Start group for column 1

        // Define the size of the icon based on text line height
        Vector2 iconSize = new(ImGui.GetTextLineHeight() * 2f);

        // Draw item icon
        if (SERVICES.TextureProvider.GetFromGameIcon(new GameIconLookup { IconId = iItem.Icon }).TryGetWrap(out var icon, out Exception? ex))
        {
            if (icon != null)
            {
                ImGui.Image(icon.ImGuiHandle, iconSize);
            }
        }

        ImGui.EndGroup(); // End group for column 1

        // Column 2 (Item Name and Level)
        ImGui.SameLine();
        ImGui.BeginGroup(); // Start group for column 2

        // Item Name (Row 1)
        Vector2 itemNameSize = ImGui.CalcTextSize(iItem.Name.ExtractText());
        ImGui.Text(iItem.Name.ExtractText());
        float itemNamePosY = ImGui.GetCursorPosY(); // Get current Y position after item name

        // Item Level (Row 2, smaller text, positioned directly below item name)
        ImGui.SetCursorPosY(itemNamePosY + ImGui.GetStyle().ItemSpacing.Y); // Move below item name

        // Adjust the Y position of the level text (raise it up)
        ImGui.SetCursorPosY(ImGui.GetCursorPosY() - 7f); // Move it slightly higher
        ImGui.SetWindowFontScale(0.75f); // Scale down the font for the item level text
        ImGui.Text("Level: " + iItem.LevelEquip.ToString());
        ImGui.SetWindowFontScale(1f); // Reset to the original font size

        ImGui.EndGroup(); // End group for column 2

        // Column 3 (Empty icon space, same size as column 1)
        ImGui.SameLine();
        ImGui.BeginGroup(); // Start group for column 3

        // Draw an empty icon (same size as column 1, but no icon)
        ImGui.Image(IntPtr.Zero, iconSize); // Empty space for now, will add functionality later

        ImGui.EndGroup(); // End group for column 3
    }

    public void MainDraw()
    {
        DateTime Now = DateTime.Now;
        TimeSpan ts = new TimeSpan(0, 1, 0);
        if (((Last + ts) > DateTime.Now) && (Configuration.WeaponDye.DyeId == 0 || Configuration.HeadDye.DyeId == 0 || Configuration.BodyDye.DyeId == 0 || Configuration.GlovesDye.DyeId == 0 || Configuration.LegsDye.DyeId == 0 || Configuration.BootsDye.DyeId == 0 ) && (Now.DayOfWeek == DayOfWeek.Friday || Now.DayOfWeek == DayOfWeek.Saturday || Now.DayOfWeek == DayOfWeek.Sunday || Now.DayOfWeek == DayOfWeek.Monday))
        {
            Configuration.PopulateDyes();
            Last = Now;
        }

        try
        {
            uint[] items = new uint[11];
            for (int i = 0; i < items.Length; i++)
            {
                int t = i;
                if (t >= 1 && t < 4) t++;
                else if (t >= 4) t += 2;
                items[i] = GetEquipped(t).ItemId;
            }

            InventoryItem Weapon = GetEquipped(0);
            InventoryItem Head = GetEquipped(2);
            InventoryItem Body = GetEquipped(3);
            InventoryItem Gloves = GetEquipped(4);
            InventoryItem Legs = GetEquipped(6);
            InventoryItem Boots = GetEquipped(7);
            InventoryItem Earrings = GetEquipped(8);
            InventoryItem Necklace = GetEquipped(9);
            InventoryItem Bracelet = GetEquipped(10);
            InventoryItem RightRing = GetEquipped(11);
            InventoryItem LeftRing = GetEquipped(12);

            ImGui.SetWindowFontScale(3);
            TextCentered(Configuration.sWeeklyTheme);
            ImGui.SetWindowFontScale(1);
            ImGui.Separator();
            ImGui.Columns(6, "Menu", false);
            ImGui.SetColumnWidth(0, 180);
            ImGui.SetColumnWidth(1, 150);
            ImGui.SetColumnWidth(2, 400);
            ImGui.SetColumnWidth(3, 150);
            ImGui.SetColumnWidth(4, 150);
            ImGui.SetColumnWidth(5, 80);
            TextCentered("Slot");
            ImGui.NextColumn();
            TextCentered("Theme");
            ImGui.NextColumn();
            TextCentered("Equipped Item");
            ImGui.NextColumn();
            TextCentered("Dye 1");
            ImGui.NextColumn();
            TextCentered("Dye 2");
            ImGui.NextColumn();
            TextCentered("Points");
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

            ImGui.Text("Weapon");
            if (Configuration.WeaponDye.DyeId != 0 )
            {
                ImGui.SameLine();
                ImGui.Text($" ({Configuration.WeaponDye.DyeName})");
            }
            ImGui.Text("Head");
            if (Configuration.HeadDye.DyeId != 0)
            {
                ImGui.SameLine();
                ImGui.Text($" ({Configuration.HeadDye.DyeName})");
            }
            ImGui.Text("Body");
            if (Configuration.BodyDye.DyeId != 0)
            {
                ImGui.SameLine();
                ImGui.Text($" ({Configuration.BodyDye.DyeName})");
            }
            ImGui.Text("Gloves");
            if (Configuration.GlovesDye.DyeId != 0)
            {
                ImGui.SameLine();
                ImGui.Text($" ({Configuration.GlovesDye.DyeName})");
            }
            ImGui.Text("Legs");
            if (Configuration.LegsDye.DyeId != 0)
            {
                ImGui.SameLine();
                ImGui.Text($" ({Configuration.LegsDye.DyeName})");
            }
            ImGui.Text("Boots");
            if (Configuration.BootsDye.DyeId != 0)
            {
                ImGui.SameLine();
                ImGui.Text($" ({Configuration.BootsDye.DyeName})");
            }
            ImGui.Text("Earrings");
            ImGui.Text("Necklace");
            ImGui.Text("Bracelet");
            ImGui.Text("Right Ring");
            ImGui.Text("Left Ring");
            ImGui.NextColumn();

            if (!string.IsNullOrEmpty(Configuration.sWeapon)) ButtonTheme(Configuration.sWeapon, Configuration.sWeaponData, 1); else ImGui.NewLine();
            if (!string.IsNullOrEmpty(Configuration.sHead)) ButtonTheme(Configuration.sHead, Configuration.sHeadData, 3); else ImGui.NewLine();
            if (!string.IsNullOrEmpty(Configuration.sBody)) ButtonTheme(Configuration.sBody, Configuration.sBodyData, 4); else ImGui.NewLine();
            if (!string.IsNullOrEmpty(Configuration.sGloves)) ButtonTheme(Configuration.sGloves, Configuration.sGlovesData, 5); else ImGui.NewLine();
            if (!string.IsNullOrEmpty(Configuration.sLegs)) ButtonTheme(Configuration.sLegs, Configuration.sLegsData, 7); else ImGui.NewLine();
            if (!string.IsNullOrEmpty(Configuration.sBoots)) ButtonTheme(Configuration.sBoots, Configuration.sBootsData, 8); else ImGui.NewLine();
            if (!string.IsNullOrEmpty(Configuration.sEarrings)) ButtonTheme(Configuration.sEarrings, Configuration.sEarringsData, 9); else ImGui.NewLine();
            if (!string.IsNullOrEmpty(Configuration.sNecklace)) ButtonTheme(Configuration.sNecklace, Configuration.sNecklaceData, 10); else ImGui.NewLine();
            if (!string.IsNullOrEmpty(Configuration.sBracelet)) ButtonTheme(Configuration.sBracelet, Configuration.sBraceletData, 11); else ImGui.NewLine();
            if (!string.IsNullOrEmpty(Configuration.sRightRing)) ButtonTheme(Configuration.sRightRing, Configuration.sRightRingData, 12); else ImGui.NewLine();
            if (!string.IsNullOrEmpty(Configuration.sLeftRing)) ButtonTheme(Configuration.sLeftRing, Configuration.sLeftRingData, 13); else ImGui.NewLine();
            ImGui.NextColumn();

            if (Weapon.GlamourId > 0) TextCentered(GetGearItem(Weapon.GlamourId).Name.ToString()); else TextCentered(GetGearItem(Weapon.ItemId).Name.ToString());
            if (Head.GlamourId> 0) TextCentered(GetGearItem(Head.GlamourId).Name.ToString()); else TextCentered(GetGearItem(Head.ItemId).Name.ToString());
            if (Body.GlamourId > 0) TextCentered(GetGearItem(Body.GlamourId).Name.ToString()); else TextCentered(GetGearItem(Body.ItemId).Name.ToString());
            if (Gloves.GlamourId > 0) TextCentered(GetGearItem(Gloves.GlamourId).Name.ToString()); else TextCentered(GetGearItem(Gloves.ItemId).Name.ToString());
            if (Legs.GlamourId > 0) TextCentered(GetGearItem(Legs.GlamourId).Name.ToString()); else TextCentered(GetGearItem(Legs.ItemId).Name.ToString());
            if (Boots.GlamourId > 0) TextCentered(GetGearItem(Boots.GlamourId).Name.ToString()); else TextCentered(GetGearItem(Boots.ItemId).Name.ToString());
            if (Earrings.GlamourId > 0) TextCentered(GetGearItem(Earrings.GlamourId).Name.ToString()); else TextCentered(GetGearItem(Earrings.ItemId).Name.ToString());
            if (Necklace.GlamourId > 0) TextCentered(GetGearItem(Necklace.GlamourId).Name.ToString()); else TextCentered(GetGearItem(Necklace.ItemId).Name.ToString());
            if (Bracelet.GlamourId > 0) TextCentered(GetGearItem(Bracelet.GlamourId).Name.ToString()); else TextCentered(GetGearItem(Bracelet.ItemId).Name.ToString());
            if (RightRing.GlamourId > 0) TextCentered(GetGearItem(RightRing.GlamourId).Name.ToString()); else TextCentered(GetGearItem(RightRing.ItemId).Name.ToString());
            if (LeftRing.GlamourId > 0) TextCentered(GetGearItem(LeftRing.GlamourId).Name.ToString()); else TextCentered(GetGearItem(LeftRing.ItemId).Name.ToString());
            ImGui.NextColumn();

            if (Weapon.ItemId > 0) PrintDye(Weapon, 0);
            if (Head.ItemId > 0) PrintDye(Head, 0);
            if (Body.ItemId > 0) PrintDye(Body, 0);
            if (Gloves.ItemId > 0) PrintDye(Gloves, 0);
            if (Legs.ItemId > 0) PrintDye(Legs, 0);
            if (Boots.ItemId > 0) PrintDye(Boots, 0);
            ImGui.NextColumn();

            if (Weapon.ItemId > 0) PrintDye(Weapon, 1);
            if (Head.ItemId > 0) PrintDye(Head, 1);
            if (Body.ItemId > 0) PrintDye(Body, 1);
            if (Gloves.ItemId > 0) PrintDye(Gloves, 1);
            if (Legs.ItemId > 0) PrintDye(Legs, 1);
            if (Boots.ItemId > 0) PrintDye(Boots, 1);
            ImGui.NextColumn();

            uint[] SlotPoints = new uint[11];
            SlotPoints[0] = GetSlotPoints(GetEquipped(0).ItemId, Configuration.sWeapon, Configuration.sWeaponData) + GetDyePoints(Weapon);
            SlotPoints[1] = GetSlotPoints(GetEquipped(2).ItemId, Configuration.sHead, Configuration.sHeadData) + GetDyePoints(Head);
            SlotPoints[2] = GetSlotPoints(GetEquipped(3).ItemId, Configuration.sBody, Configuration.sBodyData) + GetDyePoints(Body);
            SlotPoints[3] = GetSlotPoints(GetEquipped(4).ItemId, Configuration.sGloves, Configuration.sGlovesData) + GetDyePoints(Gloves);
            SlotPoints[4] = GetSlotPoints(GetEquipped(6).ItemId, Configuration.sLegs, Configuration.sLegsData) + GetDyePoints(Legs);
            SlotPoints[5] = GetSlotPoints(GetEquipped(7).ItemId, Configuration.sBoots, Configuration.sBootsData) + GetDyePoints(Boots);
            SlotPoints[6] = GetSlotPoints(GetEquipped(8).ItemId, Configuration.sEarrings, Configuration.sEarringsData);
            SlotPoints[7] = GetSlotPoints(GetEquipped(9).ItemId, Configuration.sNecklace, Configuration.sNecklaceData);
            SlotPoints[8] = GetSlotPoints(GetEquipped(10).ItemId, Configuration.sBracelet, Configuration.sBraceletData);
            SlotPoints[9] = GetSlotPoints(GetEquipped(11).ItemId, Configuration.sRightRing, Configuration.sRightRingData);
            SlotPoints[10] = GetSlotPoints(GetEquipped(12).ItemId, Configuration.sLeftRing, Configuration.sLeftRingData);

            for (uint i = 6; i < SlotPoints.Length; i++)
                if (SlotPoints[i] == 10)
                    SlotPoints[i] = 8;

            uint Points = 0;
            foreach (uint p in SlotPoints)
            {
                if (p >= 8)
                    ImGui.PushStyleColor(ImGuiCol.Text, ImGui.ColorConvertFloat4ToU32(new System.Numerics.Vector4(0.0f, 0.0f, 1.0f, 1.0f)));
                else if (p >= 2 && p <= 4)
                    ImGui.PushStyleColor(ImGuiCol.Text, ImGui.ColorConvertFloat4ToU32(new System.Numerics.Vector4(0.0f, 1.0f, 0.0f, 1.0f)));
                else if (p == 0)
                    ImGui.PushStyleColor(ImGuiCol.Text, ImGui.ColorConvertFloat4ToU32(new System.Numerics.Vector4(1.0f, 0.0f, 0.0f, 1.0f)));
                else
                    ImGui.PushStyleColor(ImGuiCol.Text, ImGui.ColorConvertFloat4ToU32(new System.Numerics.Vector4(1.0f, 1.0f, 1.0f, 1.0f)));

                TextCentered(p.ToString());
                ImGui.PopStyleColor();
                Points += p;
            }

            ImGui.NextColumn();
            for (int p = 0; p < 6; p++)
                ImGui.Text(" / 10");
            for (int p = 0; p < 5; p++)
                ImGui.Text(" /  8");
            ImGui.NextColumn();
            ImGui.Columns(1);
            ImGui.Separator();
            ImGui.SetWindowFontScale(5);

            if (Points < 80)
                ImGui.PushStyleColor(ImGuiCol.Text, ImGui.ColorConvertFloat4ToU32(new System.Numerics.Vector4(1.0f, 0.0f, 0.0f, 1.0f))); // Red
            else if (Points >= 80 && Points < 100)
                ImGui.PushStyleColor(ImGuiCol.Text, ImGui.ColorConvertFloat4ToU32(new System.Numerics.Vector4(0.0f, 1.0f, 0.0f, 1.0f))); // Green
            else
                ImGui.PushStyleColor(ImGuiCol.Text, ImGui.ColorConvertFloat4ToU32(new System.Numerics.Vector4(1.0f, 0.84f, 0.0f, 1.0f))); // Gold

            TextCentered(Points.ToString() + " points");
            ImGui.PopStyleColor();

            ImGui.SetWindowFontScale(1);
            TextCentered("Dyes are factored in as bonus points IF next to the Weapon or Armor category it is listed! (Updates after Friday when Historical Data is updated by Scarlet)");
            ImGui.Separator();
        }
        catch (Exception ex)
        {
            SERVICES.Log.Error(ex, "Error in MainDraw");
        }
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

    private uint GetSlotPoints(uint item, string slot, string data)
    {
        if (item == 0) return 0;
        if (slot == "") return 10;
        if (data == "" && slot != "")
        {
            return 2;
        }
        string[] Gears = data.Split('|');
        foreach (string Gear in Gears)
        {
            if (item == uint.Parse(Gear))
                return 10;
        }
        return 2;
    }

    private uint GetDyePoints(InventoryItem item)
    {
        switch(item.GetSlot())
        {
            case 0:
                return CompareColors(Configuration.WeaponDye, GetDyeInfo(item.GetStain(0)), GetDyeInfo(item.GetStain(1)));
            case 2:
                return CompareColors(Configuration.HeadDye, GetDyeInfo(item.GetStain(0)), GetDyeInfo(item.GetStain(1)));
            case 3:
                return CompareColors(Configuration.BodyDye, GetDyeInfo(item.GetStain(0)), GetDyeInfo(item.GetStain(1)));
            case 4:
                return CompareColors(Configuration.GlovesDye, GetDyeInfo(item.GetStain(0)), GetDyeInfo(item.GetStain(1)));
            case 6:
                return CompareColors(Configuration.LegsDye, GetDyeInfo(item.GetStain(0)), GetDyeInfo(item.GetStain(1)));
            case 7:
                return CompareColors(Configuration.BootsDye, GetDyeInfo(item.GetStain(0)), GetDyeInfo(item.GetStain(1)));
        }
        return 0;
    }

    private uint CompareColors(DyeInfo Dye, DyeInfo Slot1, DyeInfo Slot2)
    {
        if (Dye.DyeId == 0)
            return 0;
        if ((Dye.DyeColor == Slot1.DyeColor || Dye.DyeColor == Slot2.DyeColor) && Dye.DyeColor != 0)
            return 2;
        uint dDye = Dye.DyeShade, dSlot1 = Slot1.DyeShade, dSlot2 = Slot2.DyeShade;
        if (dDye == 10) dDye = Convert10(Dye.DyeName);
        if (dSlot1 == 10) dSlot1 = Convert10(Slot1.DyeName);
        if (dSlot2 == 10) dSlot2 = Convert10(Slot2.DyeName);
        if (dDye == dSlot1 || dDye == dSlot2)
            return 1;
        return 0;
    }

    private uint Convert10(string Dye)
    {
        switch (Dye)
        {
            case "Pearl White":
            case "Pure White":
            case "Jet Black":
            case "Metallic Silver":
                return 2;
            case "Pastel Pink":
            case "Dark Red":
            case "Metallic Red":
                return 4;
            case "Dark Brown":
            case "Metallic Orange":
                return 5;
            case "Metallic Brass":
            case "Metallic Yellow":
            case "Metallic Gold":
                return 6;
            case "Dark Green":
            case "Pastel Green":
            case "Metallic Green":
                return 7;
            case "Pastel Blue":
            case "Dark Blue":
            case "Metallic Sky Blue":
            case "Metallic Blue":
                return 8;
            case "Pastel Purple":
            case "Dark Purple":
            case "Metallic Purple":
                return 9;
        }
        return 10;
    }

    public unsafe void Check()
    {
        Configuration.bCurrent = (WeeklyReset(DateTime.Now) == WeeklyReset(Configuration.dtLastChecked));
#pragma warning disable CS8602
        AtkUnitBase* addon = (AtkUnitBase*)SERVICES.GameGui.GetAddonByName("FashionCheck");
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

    private unsafe InventoryItem GetEquipped(int x)
    {
        InventoryItem* equipmentInventoryItem;
        if (x < 0 || x > 13) return new InventoryItem();
        equipmentInventoryItem = InventoryManager.Instance()->GetInventorySlot(InventoryType.EquippedItems, x);
        if (equipmentInventoryItem != null)
        {
            InventoryItem inventoryItem = *equipmentInventoryItem;
            if (inventoryItem.GetItemId() > 0)
            {
                return inventoryItem;
            }
        }
        InventoryItem item = new InventoryItem();
        item.ItemId = 0;
        return item;
    }

    private Item GetGearItem(uint itemId)
    {
        List<Item> itItem = lAllItems.Where(item => item.RowId == itemId).ToList();
        return itItem.Count > 0 ? itItem[0] : new Item();
    }

    private void PrintDye(InventoryItem item, int i)
    {
        if (item.ItemId > 0)
        {
            DyeInfo? Dye = GetDyeInfo(item.GetStain(i));
            if (Dye != null) TextCentered($"{Dye.DyeName}");
            else ImGui.NewLine();
            return;
        }
        ImGui.NewLine();
    }

    public DyeInfo GetDyeInfo(uint dyeId)
    {
        Lumina.Excel.ExcelSheet<Stain> stainSheet = SERVICES.DataManager.GetExcelSheet<Stain>();
        Stain Dye = stainSheet.GetRow(dyeId);

        if (Dye.RowId != 0)
        {
            return new DyeInfo
            {
                DyeId = dyeId,
                DyeName = Dye.Name.ExtractText(), // Name of the dye
                DyeColor = Dye.Color, // The color
                DyeShade = Dye.Shade // The shade
            };
        }
        return new DyeInfo { DyeId = 0 }; // Return null if no match is found
    }

    public class DyeInfo
    {
        public uint DyeId { get; set; }
        public string DyeName { get; set; }
        public uint DyeColor { get; set; }
        public uint DyeShade { get; set; }
    }

    private enum STATE { Main, Slot };
}
