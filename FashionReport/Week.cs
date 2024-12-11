using Dalamud.Memory;
using FFXIVClientStructs.FFXIV.Component.GUI;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.IO;

namespace FashionReport
{
    public enum ItemSlot
    {
        Weapon,
        Head,
        Body,
        Gloves,
        Legs,
        Boots,
        Earrings,
        Necklace,
        Bracelet,
        RightRing,
        LeftRing
    }

    internal class WEEK
    {
        FASHIONREPORT FashionReport;

        public DateTime LastChecked;
        public SortedList<string, string> Themes; // SLOT, THEME
        public SortedList<string, string> ThemeDetails; // Theme, int data to be parsed!
        public string WeeklyTheme;
        public bool Current;

        public WEEK(FASHIONREPORT fr)
        {
            Current = false;
            FashionReport = fr;
            WeeklyTheme = "";
            LastChecked = new DateTime(2000,01,01,0,0,0);
            Themes = new SortedList<string, string>();
            ThemeDetails = new SortedList<string, string>();
            foreach (FashionReport.ItemSlot slot in Enum.GetValues<ItemSlot>())
                Themes[slot.ToString()] = "";
            Load();
        }

        public bool IsGold(uint item, string theme)
        {
            string data = ThemeDetails[theme];
            if (data == "") return false;
            string[] Gears = data.Split('|');
            foreach (string Gear in Gears)
            {
                if (item == uint.Parse(Gear))
                    return true;
            }
            return false;
        }

        private DateTime WeeklyReset(DateTime time)
        {
            time = time.AddHours(-8);
            int offset = ((int)time.DayOfWeek) - ((int)DayOfWeek.Tuesday);
            if (offset < 0) offset += 7;
            time = time.AddDays(-offset);
            return new DateTime(time.Year, time.Month, time.Day, 0, 0, 0);
        }

        public void Log(string message)
        {
            StreamWriter sw = new StreamWriter("D:\\C\\FashionReportDatabaseUpdater\\FashionReportDatabaseUpdater\\bin\\Debug\\net8.0\\log.txt", true);
            sw.WriteLine(message);
            sw.Close();
        }

        public unsafe void Check()
        {
            if (WeeklyReset(DateTime.Now) == WeeklyReset(LastChecked))
            {
                Current = true;
                return;
            }
            else
            {
                Log("Lastchecked: " + LastChecked.ToLongDateString());
                Current = false;
            }
#pragma warning disable CS8602
            AtkUnitBase* addon = (AtkUnitBase*)FashionReport.GameGui.GetAddonByName("FashionCheck");
#pragma warning restore CS8602
            if ((nint)addon != IntPtr.Zero)
                if (((AtkUnitBase*)addon)->RootNode != null && ((AtkUnitBase*)addon)->RootNode->IsVisible())
                {
                    LastChecked = DateTime.Now;
                    WeeklyTheme = MemoryHelper.ReadSeStringNullTerminated((nint)addon->AtkValues[0].String).TextValue;
                    foreach (FashionReport.ItemSlot slot in Enum.GetValues<ItemSlot>())
                        Themes[slot.ToString()] = MemoryHelper.ReadSeStringNullTerminated((nint)addon->AtkValues[((int)slot * 11) + 2].String).TextValue;
                    Save();
                }
        }

/*
        private void Load()
        {
            if (!File.Exists("WeeklyData.wgw"))
                return;
            StreamReader sr = new StreamReader("WeeklyData.wgw");
            try
            {
                string? line = sr.ReadLine();
                if (line != null) LastChecked = DateTime.FromBinary(long.Parse(line));
                line = sr.ReadLine();
                if (line != null) WeeklyTheme = line;
                foreach (FashionReport.ItemSlot slot in Enum.GetValues<ItemSlot>())
                {
                    line = sr.ReadLine();
                    if (line != null) Themes[slot.ToString()] = line;
                }
            }
            catch
            {
                LastChecked = DateTime.FromBinary(0);
                WeeklyTheme = "";
                foreach (FashionReport.ItemSlot slot in Enum.GetValues<ItemSlot>())
                    Themes[slot.ToString()] = "";
            }
            sr.Close();

            MySqlConnection conn = new MySqlConnection("PORT=19137;SERVER=srankhunter-srankhunter.f.aivencloud.com;DATABASE=FashionReport;UID=FRMod;PASSWORD=jgv90jRnioasDfioFhnbweo;");
            conn.Open();
            ThemeDetails.Clear();
            foreach (ItemSlot slot in Enum.GetValues<ItemSlot>())
            {
                if (Themes[slot.ToString()] != "" || Themes[slot.ToString()] != null)
                {
                    MySqlCommand cmd = new MySqlCommand();
                    cmd.Connection = conn;
                    cmd.CommandText = "SELECT Gears FROM SlotThemes WHERE Theme=@theme";
                    cmd.Parameters.AddWithValue("@theme", Themes[slot.ToString()]);
                    MySqlDataReader reader = cmd.ExecuteReader();
                    if (reader.Read())
                    {
                        string? data = reader["Gears"].ToString();
                        if (data != null)
                        {
                            ThemeDetails[Themes[slot.ToString()]] = data;
                        }
                    }
                    reader.Close();
                }
            }
            conn.Close();
            Check();
        }

        private void Save()
        {
            StreamWriter sw = new StreamWriter("WeeklyData.wgw");
            sw.WriteLine(LastChecked.ToBinary());
            sw.WriteLine(WeeklyTheme);
            foreach(FashionReport.ItemSlot slot in Enum.GetValues<ItemSlot>())
                sw.WriteLine(Themes[slot.ToString()]);
            sw.Close();
        }

        public uint GetMax(string Slot)
        {
            switch (Slot)
            {
                case "Weapon":
                case "Head":
                case "Body":
                case "Gloves":
                case "Legs":
                case "Boots":
                    return 10;
                case "Earrings":
                case "Necklace":
                case "Bracelet":
                case "RightRing":
                case "LeftRing":
                    return 8;
                default: return 0;
            }
        }
*/
    }
}
