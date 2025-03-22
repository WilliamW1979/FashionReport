using FFXIVClientStructs.FFXIV.Client.Game;
using ImGuiNET;
using Lumina.Excel.Sheets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FashionReport
{
    public static class DYES
    {
        public struct DYEINFO
        {
            public uint DyeId;
            public string DyeName;
            public uint DyeColor;
            public uint DyeShade;

            public DYEINFO(uint Id = 0, string Name = "", uint Color = 0, uint Shade = 0)
            {
                DyeId = Id;
                DyeName = Name;
                DyeColor = Color;
                DyeShade = Shade;
            }
        }

        public static void PrintDye(InventoryItem item, int i)
        {
            if (item.ItemId > 0)
            {
                DYEINFO Dye = GetDyeInfo(item.GetStain(i));
                if (Dye.DyeId != 0) IMGUIFORMAT.TextCenteredColumn($"{Dye.DyeName}");
                else ImGui.NewLine();
                return;
            }
            ImGui.NewLine();
        }

        public static uint CompareColors(DYEINFO Dye, DYEINFO Slot1, DYEINFO Slot2)
        {
            if (Dye.DyeId == 0 || Dye.DyeColor == 0)
                return 0;
            if ((Dye.DyeColor == Slot1.DyeColor || Dye.DyeColor == Slot2.DyeColor))
                return 2;
            uint dDye = Dye.DyeShade, dSlot1 = Slot1.DyeShade, dSlot2 = Slot2.DyeShade;
            if (dDye == 10 || dDye == 2) dDye = Convert10(Dye.DyeName);
            if (dSlot1 == 10 || dSlot1 == 2) dSlot1 = Convert10(Slot1.DyeName);
            if (dSlot2 == 10 || dSlot2 == 2) dSlot2 = Convert10(Slot2.DyeName);
            if (dDye == dSlot1 || dDye == dSlot2)
                return 1;
            return 0;
        }

        public static DYEINFO GetDyeInfo(uint DyeId)
        {
            Lumina.Excel.ExcelSheet<Stain> stainSheet = SERVICES.DataManager.GetExcelSheet<Stain>();
            Stain Dye = stainSheet.GetRow(DyeId);

            if (Dye.RowId != 0)
                return new DYEINFO(DyeId, Dye.Name.ExtractText(), Dye.Color, Dye.Shade);
            return new DYEINFO();
        }

        public static DYEINFO GetDyeInfo(string DyeName)
        {
            if (string.IsNullOrEmpty(DyeName)) return new DYEINFO();
            DyeName = DyeName.Replace("Dye", "");
            DyeName = DyeName.Trim();
            Lumina.Excel.ExcelSheet<Stain> stainSheet = SERVICES.DataManager.GetExcelSheet<Stain>();
            IEnumerable<Stain> Dyes = stainSheet.Where<Stain>(stain => stain.Name == DyeName);
            Stain Dye = new Stain();
            if (Dyes.Count() >= 1) Dye = Dyes.First();
            else return new DYEINFO();

            if (Dye.RowId != 0)
                return new DYEINFO(Dye.RowId, Dye.Name.ExtractText(), Dye.Color, Dye.Shade);
            return new DYEINFO();
        }

        private static uint Convert10(string Dye)
        {
            switch (Dye)
            {
                case "Pearl White":
                case "Pure White":
                case "Snow White":
                case "Metallic Silver":
                    return 11;
                case "Ash Grey":
                case "Goobbue Grey":
                case "Slate Grey":
                case "Charcoal Grey":
                    return 13;
                case "Jet Black":
                case "Gunmetal Black":
                case "Soot Black":
                    return 12;
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
    }
}
