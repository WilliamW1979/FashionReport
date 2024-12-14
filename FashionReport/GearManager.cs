using Lumina.Excel.Sheets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Numerics;
using System.Threading.Tasks;
using Dalamud.Interface.Textures;
using static FFXIVClientStructs.FFXIV.Client.Game.UI.NpcTrade;
using Item = Lumina.Excel.Sheets.Item;
using Dalamud.Interface.Utility;
using ImGuiNET;
using Dalamud.Interface.Textures.TextureWraps;
using static System.Net.Mime.MediaTypeNames;
using Dalamud.Interface;

namespace FashionReport
{
    public class GEARMANAGER
    {
        FASHIONREPORT FashionReport;
        public List<Item> lAllItems;

        public GEARMANAGER(FASHIONREPORT fashionReport)
        {
            FashionReport = fashionReport;
            lAllItems = new List<Item>();
            lAllItems = FASHIONREPORT.DataManager.GetExcelSheet<Item>()!.Where(item => item.EquipSlotCategory.RowId != 0 && item.EquipSlotCategory.Value!.SoulCrystal == 0 && item.EquipSlotCategory.Value!.OffHand == 0).ToList();
            FashionReport = fashionReport;
            foreach (Item item in lAllItems)
            {
                string Data = "";
                string sep = " | ";
                Data += item.RowId.ToString();
                Data += sep;
                Data += item.Name.ToString();
                Data += sep;
                Data += item.EquipSlotCategory.RowId.ToString();
            }
        }

        public void DrawItem(Item item)
        {
            if(item.Icon != 0)
            {
                Vector2 IconSize = new(ImGui.GetTextLineHeight() * 2f);
                Vector2 SlotWidthSize = new(ImGui.CalcTextSize("W").X * 30f, (IconSize.Y + ImGui.GetStyle().ItemSpacing.Y) * 6f);

                if (FASHIONREPORT.TextureProvider.GetFromGameIcon(new GameIconLookup { IconId = item.Icon }).TryGetWrap(out var icon, out Exception? ex))
                    if (icon != null)
                    {
                        ImGui.Image(icon.ImGuiHandle, IconSize);
                        ImGui.SameLine();
                    }
                ImGui.SetCursorPosY(((ImGui.CalcTextSize(item.Name.ExtractText()).Y) * 0.5f) + ImGui.GetCursorPosY());
                ImGui.Text(item.Name.ExtractText());
            }
        }

        private IDalamudTextureWrap? GetIcon(Item item)
        {
            FASHIONREPORT.TextureProvider.GetFromGameIcon(new GameIconLookup { IconId = item.Icon }).TryGetWrap(out IDalamudTextureWrap? icon, out _);
            return icon;
        }

        public void DrawIcon(Item item, Vector2 vSize = default, string? sTooltip = null, bool bSmall = false)
        {

        }

        public void DrawIcon(FontAwesomeIcon aIcon, Vector2 vSize = default, string? sTooltip = null, bool bSmall = false)
        {
            ImGui.PushFont(UiBuilder.IconFont);
            bool result = bSmall ? ImGui.SmallButton(aIcon.ToIconString()) : ImGui.Button(aIcon.ToIconString(), vSize);
            ImGui.PopFont();
            if(ImGui.IsItemHovered() && sTooltip != null)
                ImGui.SetTooltip(sTooltip);
        }
        
        public Item GetGearItem(uint ItemID)
        {
            List<Item> itItem = lAllItems.Where(item => item.RowId == ItemID).ToList();
            if(itItem.Count > 0)
                return (Item)itItem[0];
            return new Item();
        }
    }
}
