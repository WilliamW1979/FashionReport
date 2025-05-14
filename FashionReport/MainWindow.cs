using Dalamud.Interface.Windowing;
using System;
using System.Numerics;
using System.Collections.Generic;
using Lumina.Excel.Sheets;
using FFXIVClientStructs.FFXIV.Client.Game;
using ImGuiNET;
using System.Linq;
using FFXIVClientStructs.FFXIV.Client.Graphics.Scene;
using Dalamud.Interface.Textures.TextureWraps;
using Dalamud.Interface.Textures;

namespace FashionReport
{
    public class MAINWINDOW : Window, IDisposable
    {
        private string weeklyTheme = string.Empty;
        private Dictionary<string, string> slotThemes = new Dictionary<string, string>();
        private Dictionary<string, List<uint>> slotItemIds = new Dictionary<string, List<uint>>();
        private Dictionary<string, InventoryItem> equippedGear = new Dictionary<string, InventoryItem>();
        private Dictionary<string, (uint DyeId1, uint DyeId2)> slotDyes = new Dictionary<string, (uint, uint)>();
        private DateTime lastServerCheck = DateTime.MinValue;
        private string sState = "Main";

        private readonly string[] equipmentSlots = { "Weapon", "Head", "Body", "Gloves", "Legs", "Boots", "Earrings", "Necklace", "Bracelet", "RightRing", "LeftRing" };
        private readonly Dictionary<string, uint> SlotMax = new Dictionary<string, uint>()
        {
            { "Weapon", 10 }, { "Head", 10 }, { "Body", 10 }, { "Gloves", 10 }, { "Legs", 10 }, { "Boots", 10 },
            { "Earrings", 8 }, { "Necklace", 8 }, { "Bracelet", 8 }, { "RightRing", 8 }, { "LeftRing", 8 }
        };

        public MAINWINDOW() : base("Fashion Report Helper", ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse)
        {
            SizeConstraints = new WindowSizeConstraints
            {
                MinimumSize = new Vector2(1110, 440),
                MaximumSize = new Vector2(1110, 440)
            };
        }

        public void Dispose()
        {
        }

        public override void Draw()
        {
            if (sState == "Main")
                MainDraw();
            else
                SlotDraw();
        }

        public void MainDraw()
        {
            ImGui.SetWindowFontScale(3);
            TextCenteredColumn(SERVICES.FRData.WeeklyTheme);
            ImGui.SetWindowFontScale(1);
            ImGui.Separator();

            ImGui.Columns(6, "Menu", false);
            ImGui.SetColumnWidth(0, 180);
            ImGui.SetColumnWidth(1, 150);
            ImGui.SetColumnWidth(2, 400);
            ImGui.SetColumnWidth(3, 150);
            ImGui.SetColumnWidth(4, 150);
            ImGui.SetColumnWidth(5, 80);
            TextCenteredColumn("Slot");
            ImGui.NextColumn();
            TextCenteredColumn("Theme");
            ImGui.NextColumn();
            TextCenteredColumn("Equipped Item");
            ImGui.NextColumn();
            TextCenteredColumn("Dye 1");
            ImGui.NextColumn();
            TextCenteredColumn("Dye 2");
            ImGui.NextColumn();
            TextCenteredColumn("Points");

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

            foreach (string slot in SERVICES.FRSlots)
            {
                if (slot == "RightRing")
                    ImGui.Text("Right Ring");
                else if (slot == "LeftRing")
                    ImGui.Text("Left Ring");
                else
                    ImGui.Text(slot);
                string? Dye = slot switch
                {
                    "Weapon" => SERVICES.FRData.WeaponDye,
                    "Head" => SERVICES.FRData.HeadDye,
                    "Body" => SERVICES.FRData.BodyDye,
                    "Gloves" => SERVICES.FRData.GlovesDye,
                    "Legs" => SERVICES.FRData.LegsDye,
                    "Boots" => SERVICES.FRData.BootsDye,
                    _ => null
                };
                if (Dye != null)
                {
                    ImGui.SameLine();
                    ImGui.Text($" ({Dye})");
                }
            }
            ImGui.NextColumn();

            foreach (string slot in SERVICES.FRSlots)
            {
                string? theme = slot switch
                {
                    "Weapon" => SERVICES.FRData.Weapon?.FirstOrDefault().Key,
                    "Head" => SERVICES.FRData.Head?.FirstOrDefault().Key,
                    "Body" => SERVICES.FRData.Body?.FirstOrDefault().Key,
                    "Gloves" => SERVICES.FRData.Gloves?.FirstOrDefault().Key,
                    "Legs" => SERVICES.FRData.Legs?.FirstOrDefault().Key,
                    "Boots" => SERVICES.FRData.Boots?.FirstOrDefault().Key,
                    "Earrings" => SERVICES.FRData.Earrings?.FirstOrDefault().Key,
                    "Necklace" => SERVICES.FRData.Necklace?.FirstOrDefault().Key,
                    "Bracelet" => SERVICES.FRData.Bracelet?.FirstOrDefault().Key,
                    "RightRing" => SERVICES.FRData.RightRing?.FirstOrDefault().Key,
                    "LeftRing" => SERVICES.FRData.LeftRing?.FirstOrDefault().Key,
                    _ => null
                };

            if (!string.IsNullOrEmpty(theme))
                    ButtonTheme(slot);
                else
                    ImGui.NewLine();
            }
            ImGui.NextColumn();

            foreach (string slot in SERVICES.FRSlots)
            {
                InventoryItem item = slot switch
                {
                    "Weapon" => SERVICES.Equipment.Weapon,
                    "Head" => SERVICES.Equipment.Head,
                    "Body" => SERVICES.Equipment.Body,
                    "Gloves" => SERVICES.Equipment.Gloves,
                    "Legs" => SERVICES.Equipment.Legs,
                    "Boots" => SERVICES.Equipment.Boots,
                    "Earrings" => SERVICES.Equipment.Earrings,
                    "Necklace" => SERVICES.Equipment.Necklace,
                    "Bracelet" => SERVICES.Equipment.Bracelet,
                    "RightRing" => SERVICES.Equipment.RightRing,
                    "LeftRing" => SERVICES.Equipment.LeftRing,
                    _ => new InventoryItem { ItemId = 0 }
                };

                if (item.ItemId != 0)
                {
                    uint itemIdToLookup = item.GlamourId > 0 ? item.GlamourId : item.ItemId;
                    string itemName = SERVICES.AllItems.FirstOrDefault(i => i.RowId == itemIdToLookup).Name.ToString() ?? string.Empty;
                    TextCenteredColumn(itemName);
                }
                else
                {
                    ImGui.NewLine();
                }
            }
            ImGui.NextColumn();

            DYES.PrintDye(SERVICES.Equipment.Weapon, 0);
            DYES.PrintDye(SERVICES.Equipment.Head, 0);
            DYES.PrintDye(SERVICES.Equipment.Body, 0);
            DYES.PrintDye(SERVICES.Equipment.Gloves, 0);
            DYES.PrintDye(SERVICES.Equipment.Legs, 0);
            DYES.PrintDye(SERVICES.Equipment.Boots, 0);
            ImGui.NextColumn();

            DYES.PrintDye(SERVICES.Equipment.Weapon, 1);
            DYES.PrintDye(SERVICES.Equipment.Head, 1);
            DYES.PrintDye(SERVICES.Equipment.Body, 1);
            DYES.PrintDye(SERVICES.Equipment.Gloves, 1);
            DYES.PrintDye(SERVICES.Equipment.Legs, 1);
            DYES.PrintDye(SERVICES.Equipment.Boots, 1);
            ImGui.NextColumn();

            uint TotalPoints = 0;
            foreach (string slot in SERVICES.FRSlots)
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
                TextCenteredColumn(p.ToString());
                ImGui.PopStyleColor();
            }
            ImGui.NextColumn();

            foreach (string slot in SERVICES.FRSlots)
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
            TextCenteredColumn(TotalPoints.ToString() + " points");
            ImGui.PopStyleColor();

            ImGui.SetWindowFontScale(1);
            TextCenteredColumn("Dyes are factored in as bonus points IF next to the Weapon or Armor category it is listed! (Updates after Friday when Historical Data is updated by Scarlet)");
            TextCenteredColumn("Black and White dyes may be off due to how the shading system works. Future testing will weed out these problems in a future update.");
            ImGui.Separator();
        }

        private void SlotDraw()
        {
            DrawBackButton();
            ImGui.SetWindowFontScale(2.5f);
            float textWidth = ImGui.CalcTextSize(sState switch
            {
                "Weapon" => SERVICES.FRData.Weapon?.FirstOrDefault().Key,
                "Head" => SERVICES.FRData.Head?.FirstOrDefault().Key,
                "Body" => SERVICES.FRData.Body?.FirstOrDefault().Key,
                "Gloves" => SERVICES.FRData.Gloves?.FirstOrDefault().Key,
                "Legs" => SERVICES.FRData.Legs?.FirstOrDefault().Key,
                "Boots" => SERVICES.FRData.Boots?.FirstOrDefault().Key,
                "Earrings" => SERVICES.FRData.Earrings?.FirstOrDefault().Key,
                "Necklace" => SERVICES.FRData.Necklace?.FirstOrDefault().Key,
                "Bracelet" => SERVICES.FRData.Bracelet?.FirstOrDefault().Key,
                "RightRing" => SERVICES.FRData.RightRing?.FirstOrDefault().Key,
                "LeftRing" => SERVICES.FRData.LeftRing?.FirstOrDefault().Key,
                _ => ""
            }).X;
            ImGui.SetCursorPosX((ImGui.GetWindowSize().X - textWidth) / 2);
            ImGui.TextUnformatted(sState);
            ImGui.SetWindowFontScale(1);
            ImGui.Separator();

            ImGui.GetStyle().Colors[(int)ImGuiCol.Border] = new Vector4(0, 0, 0, 0);

            float childHeight = ImGui.GetWindowSize().Y - 150;

            ImGui.BeginChild("ItemList", new Vector2(0, childHeight), true, ImGuiWindowFlags.HorizontalScrollbar);

            Dictionary<string, List<uint>?>? theme = sState switch
            {
                "Weapon" => SERVICES.FRData.Weapon,
                "Head" => SERVICES.FRData.Head,
                "Body" => SERVICES.FRData.Body,
                "Gloves" => SERVICES.FRData.Gloves,
                "Legs" => SERVICES.FRData.Legs,
                "Boots" => SERVICES.FRData.Boots,
                "Earrings" => SERVICES.FRData.Earrings,
                "Necklace" => SERVICES.FRData.Necklace,
                "Bracelet" => SERVICES.FRData.Bracelet,
                "RightRing" => SERVICES.FRData.RightRing,
                "LeftRing" => SERVICES.FRData.LeftRing,
                _ => null
            };
            
            if (theme == null || string.IsNullOrEmpty(theme.FirstOrDefault().Key))
            {
                ImGui.Text("There is either no known data");
                ImGui.Text("or a connection problem with the server.");
            }
            else
            {
                foreach (uint id in theme.FirstOrDefault().Value ?? new List<uint>())
                {
                    Item item = SERVICES.AllItems.FirstOrDefault(i => i.RowId == id);
                    uint iSlot = item.EquipSlotCategory.RowId;
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

        public static void TextCenteredColumn(string Text)
        {
            ImGui.SetCursorPosX(((ImGui.GetColumnWidth() - ImGui.CalcTextSize(Text).X) * 0.5f) + ImGui.GetColumnOffset());
            ImGui.Text(Text);
        }

        public string? GetDyeSlot(string slot)
        {
            switch (slot)
            {
                case "Weapon":
                    return SERVICES.FRData.WeaponDye;
                case "Head":
                    return SERVICES.FRData.HeadDye;
                case "Body":
                    return SERVICES.FRData.BodyDye;
                case "Gloves":
                    return SERVICES.FRData.GlovesDye;
                case "Legs":
                    return SERVICES.FRData.LegsDye;
                case "Boots":
                    return SERVICES.FRData.BootsDye;
            }
            return null;
        }

        public static void TextCenteredWindow(string Text)
        {
            ImGui.SetCursorPosX(((ImGui.GetWindowWidth() - ImGui.CalcTextSize(Text).X) * 0.5f));
            ImGui.Text(Text);
        }

        void ButtonTheme(string slot)
        {
            string? Theme = slot switch
            {
                "Weapon" => SERVICES.FRData.Weapon?.Keys.FirstOrDefault(),
                "Head" => SERVICES.FRData.Head?.Keys.FirstOrDefault(),
                "Body" => SERVICES.FRData.Body?.Keys.FirstOrDefault(),
                "Gloves" => SERVICES.FRData.Gloves?.Keys.FirstOrDefault(),
                "Legs" => SERVICES.FRData.Legs?.Keys.FirstOrDefault() ,
                "Boots" => SERVICES.FRData.Boots?.Keys.FirstOrDefault(),
                _ => null
            };
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
            InventoryItem item = slot switch
            {
                "Weapon" => SERVICES.Equipment.Weapon,
                "Head" => SERVICES.Equipment.Head,
                "Body" => SERVICES.Equipment.Body,
                "Gloves" => SERVICES.Equipment.Gloves,
                "Legs" => SERVICES.Equipment.Legs,
                "Boots" => SERVICES.Equipment.Boots,
                "Earrings" => SERVICES.Equipment.Earrings,
                "Necklace" => SERVICES.Equipment.Necklace,
                "Bracelet" => SERVICES.Equipment.Bracelet,
                "RightRing" => SERVICES.Equipment.RightRing,
                "LeftRing" => SERVICES.Equipment.LeftRing,
                _ => new InventoryItem { ItemId = 0 }
            };
            Dictionary<string, List<uint>?>? Data = slot switch
            {
                "Weapon" => SERVICES.FRData.Weapon,
                "Head" => SERVICES.FRData.Head,
                "Body" => SERVICES.FRData.Body,
                "Gloves" => SERVICES.FRData.Gloves,
                "Legs" => SERVICES.FRData.Legs,
                "Boots" => SERVICES.FRData.Boots,
                "Earrings" => SERVICES.FRData.Earrings,
                "Necklace" => SERVICES.FRData.Necklace,
                "Bracelet" => SERVICES.FRData.Bracelet,
                "RightRing" => SERVICES.FRData.RightRing,
                "LeftRing" => SERVICES.FRData.LeftRing,
                _ => null
            };
            string? Theme = Data?.FirstOrDefault().Key;

            if (item.ItemId == 0) return 0;

            uint ID = item.ItemId;
            if (item.GlamourId != 0)
                ID = item.GlamourId;

            if (string.IsNullOrEmpty(Theme) || Data == null)
                return SlotMax[slot];

            foreach (uint Gear in Data.FirstOrDefault().Value ?? new List<uint>())
                if (ID == Gear)
                    return SlotMax[slot];
            return 2;
        }

        private uint GetDyePoints(string slot)
        {
            InventoryItem item = slot switch
            {
                "Weapon" => SERVICES.Equipment.Weapon,
                "Head" => SERVICES.Equipment.Head,
                "Body" => SERVICES.Equipment.Body,
                "Gloves" => SERVICES.Equipment.Gloves,
                "Legs" => SERVICES.Equipment.Legs,
                "Boots" => SERVICES.Equipment.Boots,
                _ => new InventoryItem { ItemId = 0 }
            };
            DYES.DYEINFO SlotDye = slot switch
            {
                "Weapon" => DYES.GetDyeInfo(SERVICES.FRData.WeaponDye ?? ""),
                "Head" => DYES.GetDyeInfo(SERVICES.FRData.HeadDye ?? ""),
                "Body" => DYES.GetDyeInfo(SERVICES.FRData.BodyDye ?? ""),
                "Gloves" => DYES.GetDyeInfo(SERVICES.FRData.GlovesDye ?? ""),
                "Legs" => DYES.GetDyeInfo(SERVICES.FRData.LegsDye ?? ""),
                "Boots" => DYES.GetDyeInfo(SERVICES.FRData.BootsDye ?? ""),
                _ => new()
            };

            if (SlotDye.DyeId == 0) return 0;
            return DYES.CompareColors(SlotDye, DYES.GetDyeInfo(item.GetStain(0)), DYES.GetDyeInfo(item.GetStain(1)));
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
            if (item.RowId > 0)
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
                (61432, false /* GEARMANAGER.ItemInfo[item.RowId].IsQuestReward */),
                (60831, false /* GEARMANAGER.ItemInfo[item.RowId].IsDuty */),
                (60412, false /* GEARMANAGER.ItemInfo[item.RowId].IsVender */),
                (60434, false /* GEARMANAGER.IsCraftable(item) */)
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
    }
}
