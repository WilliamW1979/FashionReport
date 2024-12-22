using Dalamud.Interface.Textures;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Item = Lumina.Excel.Sheets.Item;

namespace FashionReport
{
    public class GEARMANAGER
    {
        readonly FASHIONREPORT FashionReport;
        public List<Item> lAllItems;

        public GEARMANAGER(FASHIONREPORT fashionReport)
        {
            FashionReport = fashionReport;
            lAllItems = new List<Item>();
            lAllItems = SERVICES.DataManager.GetExcelSheet<Item>()!.Where(item => item.EquipSlotCategory.RowId != 0 && item.EquipSlotCategory.Value!.SoulCrystal == 0 && item.EquipSlotCategory.Value!.OffHand == 0).ToList();
        }

        public void DrawItem(Item item)
        {
            if(item.Icon != 0)
            {
                Vector2 IconSize = new(ImGui.GetTextLineHeight() * 2f);
                Vector2 SlotWidthSize = new(ImGui.CalcTextSize("W").X * 30f, (IconSize.Y + ImGui.GetStyle().ItemSpacing.Y) * 6f);

                if (SERVICES.TextureProvider.GetFromGameIcon(new GameIconLookup { IconId = item.Icon }).TryGetWrap(out var icon, out Exception? ex))
                    if (icon != null)
                    {
                        ImGui.Image(icon.ImGuiHandle, IconSize);
                        ImGui.SameLine();
                    }
                ImGui.SetCursorPosY(((ImGui.CalcTextSize(item.Name.ExtractText()).Y) * 0.5f) + ImGui.GetCursorPosY());
                ImGui.Text(item.Name.ExtractText());
            }
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
