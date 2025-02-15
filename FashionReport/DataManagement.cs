using Dalamud.Configuration;
using Dalamud.Memory;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using FFXIVClientStructs.FFXIV.Component.GUI;
using MySql.Data.MySqlClient;
using System;
using System.Threading;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using static FashionReport.DYES;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Asn1.Cms;
using FFXIVClientStructs.FFXIV.Client.UI;
#pragma warning disable IDE1006
#pragma warning disable CS8601
#pragma warning disable CS8600

namespace FashionReport
{
    public class DATAMANAGEMENT : IPluginConfiguration
    {
        public int Version { get; set; } = 1;
        public SortedList<string, string> SlotThemes = new SortedList<string, string>();
        public SortedList<string, string> SlotData = new SortedList<string, string>();
        public SortedList<string, DYES.DYEINFO> SlotDyes = new SortedList<string, DYEINFO>();
        public uint Week = 0;
        public string WeeklyTheme = "";
        public string[] Slots = { "Weapon", "Head", "Body", "Gloves", "Legs", "Boots", "Earrings", "Necklace", "Bracelet", "RightRing", "LeftRing" };
        public string[] Dyes = { "WeaponDye", "HeadDye", "BodyDye", "GlovesDye", "LegsDye", "BootsDye" };

        public bool IsDyes()
        {
            SlotDyes.TryGetValue("Weapon", out DYEINFO Weapon);
            SlotDyes.TryGetValue("Head", out DYEINFO Head);
            SlotDyes.TryGetValue("Body", out DYEINFO Body);
            SlotDyes.TryGetValue("Gloves", out DYEINFO Gloves);
            SlotDyes.TryGetValue("Legs", out DYEINFO Legs);
            SlotDyes.TryGetValue("Boots", out DYEINFO Boots);

            if(Weapon.DyeId == 0 || Head.DyeId == 0 || Body.DyeId == 0 || Gloves.DyeId == 0 || Legs.DyeId == 0 || Boots.DyeId == 0)
                return false;
            return true;
        }

        public bool IsError()
        {
            foreach(string slot in Slots)
            {
                if (SlotThemes.TryGetValue(slot, out string? theme2) && !string.IsNullOrEmpty(theme2))
                {
                    if (SlotData.TryGetValue(slot, out string? data) && string.IsNullOrEmpty(data))
                        return true;
                }
            }
            return false;
        }

        public DateTime CalculateFriday()
        {
            DateTime Current = DateTime.Now;
            int Diff = 0;
            Console.WriteLine(Current.DayOfWeek.ToString());
            Console.WriteLine(DayOfWeek.Friday.ToString());
            switch (Current.DayOfWeek)
            {
                case DayOfWeek.Tuesday:
                    {
                        Diff = 3;
                        break;
                    }
                case DayOfWeek.Wednesday:
                    {
                        Diff = 2;
                        break;
                    }
                case DayOfWeek.Thursday:
                    {
                        Diff = 1;
                        break;
                    }
                case DayOfWeek.Saturday:
                    {
                        Diff = -1;
                        break;
                    }
                case DayOfWeek.Sunday:
                    {
                        Diff = -2;
                        break;
                    }
                case DayOfWeek.Monday:
                    {
                        Diff = -3;
                        break;
                    }
            }
            if (Current.Hour < 7)
                Diff -= 1;
            if (Diff == -4)
                Diff = 3;
            return Current.AddDays(Diff);
        }

        public DateTime CalculateTuesday()
        {
            DateTime Current = DateTime.Now;
            int Diff = 0;
            Console.WriteLine(Current.DayOfWeek.ToString());
            Console.WriteLine(DayOfWeek.Friday.ToString());
            switch (Current.DayOfWeek)
            {
                case DayOfWeek.Wednesday:
                    {
                        Diff = -1;
                        break;
                    }
                case DayOfWeek.Thursday:
                    {
                        Diff = -2;
                        break;
                    }
                case DayOfWeek.Friday:
                    {
                        Diff = -3;
                        break;
                    }
                case DayOfWeek.Saturday:
                    {
                        Diff = -4;
                        break;
                    }
                case DayOfWeek.Sunday:
                    {
                        Diff = -5;
                        break;
                    }
                case DayOfWeek.Monday:
                    {
                        Diff = -6;
                        break;
                    }
            }
            if (Current.Hour < 7 && Current.DayOfWeek != DayOfWeek.Tuesday)
                Diff -= 1;
            else if (Current.Hour < 7 && Current.DayOfWeek == DayOfWeek.Tuesday)
                Diff -= 7;
            return Current.AddDays(Diff);
        }

        public int GetCurrentWeek()
        {
            DateTime Beginning = new DateTime(2018, 1, 23, 7, 0, 0);
            return (int)((DateTime.Now - Beginning).TotalDays / 7);
        }

        public unsafe bool ReadDataFromGame()
        {
            if (Week == (uint)GetCurrentWeek())
                return false;

#pragma warning disable CS8602
            AtkUnitBase* addon = (AtkUnitBase*)SERVICES.GameGui.GetAddonByName("FashionCheck");
#pragma warning restore CS8602
            if ((nint)addon != IntPtr.Zero)
            {
                if ((((AtkUnitBase*)addon)->RootNode != null && ((AtkUnitBase*)addon)->RootNode->IsVisible()))
                {
                    SortedList<string, string> SlotThemes2 = new SortedList<string, string>();
                    SortedList<string, string> SlotData2 = new SortedList<string, string>();
                    string WeeklyTheme2 = MemoryHelper.ReadSeStringNullTerminated((nint)addon->AtkValues[0].String).TextValue;
                    if(WeeklyTheme2 == WeeklyTheme)
                    {
                        Week = (uint)GetCurrentWeek();
                        return false;
                    }
                    for (int count = 0; count < Slots.Length; count++)
                    {
                        SlotThemes2[Slots[count]] = MemoryHelper.ReadSeStringNullTerminated((nint)addon->AtkValues[2 + (11 * count)].String).TextValue;
                        SERVICES.Log.Info($"SlotThemes2[{Slots[count]}]: {SlotThemes2[Slots[count]]}");
                    }
                    try
                    {
                        Assembly assembly = Assembly.GetExecutingAssembly();
                        string resourceName = "FashionReport.FashionReport.config";
                        using (Stream stream = assembly.GetManifestResourceStream(resourceName))
                        {
                            if (stream == null)
                                throw new FileNotFoundException(resourceName);

                            XDocument configFile = XDocument.Load(stream);
                            XElement settings = configFile.Element("configuration")?.Element("databaseSettings");
                            if (settings == null)
                                throw new Exception("Invalid configuration file format.");

                            string? sServer = settings.Elements("add").FirstOrDefault(data => (string)data.Attribute("key") == "Server")?.Attribute("value")?.Value;
                            string? sDatabase = settings.Elements("add").FirstOrDefault(data => (string)data.Attribute("key") == "Database")?.Attribute("value")?.Value;
                            string? sUser = settings.Elements("add").FirstOrDefault(data => (string)data.Attribute("key") == "User")?.Attribute("value")?.Value;
                            string? sPassword = settings.Elements("add").FirstOrDefault(data => (string)data.Attribute("key") == "Password")?.Attribute("value")?.Value;
                            string? sPort = settings.Elements("add").FirstOrDefault(data => (string)data.Attribute("key") == "Port")?.Attribute("value")?.Value;

                            string connectionString = $"Server={sServer};Database={sDatabase};User={sUser};Password={sPassword};Port={sPort};";
                            MySqlConnection connection = new MySqlConnection(connectionString);
                            connection.Open();
                            SERVICES.Log.Info($"Week: {GetCurrentWeek().ToString()}");
                            string cmdstr = "INSERT INTO FashionReport VALUES(" + GetCurrentWeek().ToString() + ", '" + WeeklyTheme2 + "', '@Weapon', '@Head', '@Body', '@Gloves', '@Legs', '@Boots', '@Earrings', '@Necklace', '@Bracelet', '@RightRing', '@LeftRing', " + CalculateFriday().ToBinary() + ", null, null, null, null, null, null)";
                            string? Theme = "";
                            if (!SlotThemes2.TryGetValue("Weapon", out Theme) || string.IsNullOrEmpty(Theme)) Theme = "null";
                            if (Theme.Contains('\'')) Theme = Theme.Replace("'", "\\'");
                            cmdstr = cmdstr.Replace("@Weapon", Theme);
                            if (!SlotThemes2.TryGetValue("Head", out Theme) || string.IsNullOrEmpty(Theme)) Theme = "null";
                            if (Theme.Contains('\'')) Theme = Theme.Replace("'", "\\'");
                            cmdstr = cmdstr.Replace("@Head", Theme);
                            if (!SlotThemes2.TryGetValue("Body", out Theme) || string.IsNullOrEmpty(Theme)) Theme = "null";
                            if (Theme.Contains('\'')) Theme = Theme.Replace("'", "\\'");
                            cmdstr = cmdstr.Replace("@Body", Theme);
                            if (!SlotThemes2.TryGetValue("Gloves", out Theme) || string.IsNullOrEmpty(Theme)) Theme = "null";
                            if (Theme.Contains('\'')) Theme = Theme.Replace("'", "\\'");
                            cmdstr = cmdstr.Replace("@Gloves", Theme);
                            if (!SlotThemes2.TryGetValue("Legs", out Theme) || string.IsNullOrEmpty(Theme)) Theme = "null";
                            if (Theme.Contains('\'')) Theme = Theme.Replace("'", "\\'");
                            cmdstr = cmdstr.Replace("@Legs", Theme);
                            if (!SlotThemes2.TryGetValue("Boots", out Theme) || string.IsNullOrEmpty(Theme)) Theme = "null";
                            if (Theme.Contains('\'')) Theme = Theme.Replace("'", "\\'");
                            cmdstr = cmdstr.Replace("@Boots", Theme);
                            if (!SlotThemes2.TryGetValue("Earrings", out Theme) || string.IsNullOrEmpty(Theme)) Theme = "null";
                            if (Theme.Contains('\'')) Theme = Theme.Replace("'", "\\'");
                            cmdstr = cmdstr.Replace("@Earrings", Theme);
                            if (!SlotThemes2.TryGetValue("Necklace", out Theme) || string.IsNullOrEmpty(Theme)) Theme = "null";
                            if (Theme.Contains('\'')) Theme = Theme.Replace("'", "\\'");
                            cmdstr = cmdstr.Replace("@Necklace", Theme);
                            if (!SlotThemes2.TryGetValue("Bracelet", out Theme) || string.IsNullOrEmpty(Theme)) Theme = "null";
                            if (Theme.Contains('\'')) Theme = Theme.Replace("'", "\\'");
                            cmdstr = cmdstr.Replace("@Bracelet", Theme);
                            if (!SlotThemes2.TryGetValue("RightRing", out Theme) || string.IsNullOrEmpty(Theme)) Theme = "null";
                            if (Theme.Contains('\'')) Theme = Theme.Replace("'", "\\'");
                            cmdstr = cmdstr.Replace("@RightRing", Theme);
                            if (!SlotThemes2.TryGetValue("LeftRing", out Theme) || string.IsNullOrEmpty(Theme)) Theme = "null";
                            if (Theme.Contains('\'')) Theme = Theme.Replace("'", "\\'");
                            cmdstr = cmdstr.Replace("@LeftRing", Theme);
                            cmdstr = cmdstr.Replace("'null'", "null");
                            SERVICES.Log.Info($"cmdstr: {cmdstr}");
                            MySqlCommand cmd = new MySqlCommand(cmdstr, connection);
                            cmd.ExecuteNonQuery();
                                
                            foreach (string slot in Slots)
                            {
                                if (SlotThemes2.TryGetValue(slot, out string? Theme2) && !string.IsNullOrEmpty(Theme2))
                                {
                                    MySqlCommand cmd2 = new MySqlCommand();
                                    cmd2.Connection = connection;
                                    cmd2.CommandText = "SELECT Gears FROM SlotThemes WHERE Theme=@Theme";
                                    cmd2.Parameters.AddWithValue("@Theme", Theme2);
                                    MySqlDataReader reader = cmd2.ExecuteReader();
                                    if (reader.Read())
                                        SlotData2[slot] = reader["Gears"].ToString();
                                    reader.Close();
                                    Save();
                                }
                            }
                            Week = (uint)GetCurrentWeek();
                            WeeklyTheme = WeeklyTheme2;
                            SlotThemes = SlotThemes2;
                            SlotData = SlotData2;
                            SlotDyes.Clear();
                            Save();
                            connection.Close();
                        }
                    }
                    catch (Exception ex) { SERVICES.Log.Info(ex.ToString()); }
                    this.Save();
                    return true;
                }
            }
            return false;
        }

        public void ReadDataFromServer()
        {
            try
            {
                Assembly assembly = Assembly.GetExecutingAssembly();
                string resourceName = "FashionReport.FashionReport.config";
                using (Stream stream = assembly.GetManifestResourceStream(resourceName))
                {
                    if (stream == null)
                        throw new FileNotFoundException(resourceName);

                    XDocument configFile = XDocument.Load(stream);
                    XElement settings = configFile.Element("configuration")?.Element("databaseSettings");
                    if (settings == null)
                        throw new Exception("Invalid configuration file format.");

                    string? sServer = settings.Elements("add").FirstOrDefault(data => (string)data.Attribute("key") == "Server")?.Attribute("value")?.Value;
                    string? sDatabase = settings.Elements("add").FirstOrDefault(data => (string)data.Attribute("key") == "Database")?.Attribute("value")?.Value;
                    string? sUser = settings.Elements("add").FirstOrDefault(data => (string)data.Attribute("key") == "User")?.Attribute("value")?.Value;
                    string? sPassword = settings.Elements("add").FirstOrDefault(data => (string)data.Attribute("key") == "Password")?.Attribute("value")?.Value;
                    string? sPort = settings.Elements("add").FirstOrDefault(data => (string)data.Attribute("key") == "Port")?.Attribute("value")?.Value;

                    string connectionString = $"Server={sServer};Database={sDatabase};User={sUser};Password={sPassword};Port={sPort};";
                    MySqlConnection connection = new MySqlConnection(connectionString);
                    try
                    {
                        connection.Open();
                        string sCommand = "SELECT * FROM FashionReport ORDER BY Week DESC LIMIT 1";
                        MySqlCommand command = new MySqlCommand(sCommand, connection);
                        MySqlDataReader reader = command.ExecuteReader();
                        if (reader.Read())
                        {
                            SortedList<string, string> SlotThemes2 = new SortedList<string, string>();
                            SortedList<string, string> SlotData2 = new SortedList<string, string>();
                            SortedList<string, DYES.DYEINFO> SlotDyes2 = new SortedList<string, DYEINFO>();
                            
                            Week = reader.GetUInt32("Week");
                            if (!string.IsNullOrEmpty(reader["WeeklyTheme"].ToString()))
                                WeeklyTheme = reader["WeeklyTheme"].ToString();
                            foreach (string slot in Slots)
                            {
                                try
                                {
                                    string? Data = reader[slot].ToString();
                                    if (!string.IsNullOrEmpty(Data))
                                        SlotThemes2[slot] = Data;
                                }
                                catch (Exception ex) { SERVICES.Log.Info(ex.ToString()); }
                            }

                            foreach (string dye in Dyes)
                            {
                                try
                                {
                                    string? Data = reader[dye].ToString();
                                    if (!string.IsNullOrEmpty(Data))
                                        SlotDyes2[dye.Replace("Dye", "")] = GetDyeInfo(Data);
                                }
                                catch (Exception ex) { SERVICES.Log.Info(ex.ToString()); }
                            }
                            reader.Close();

                            foreach (string slot in Slots)
                            {
                                if (SlotThemes2.TryGetValue(slot, out string? Theme) && !string.IsNullOrEmpty(Theme))
                                {
                                    MySqlCommand cmd = new MySqlCommand();
                                    cmd.Connection = connection;
                                    cmd.CommandText = "SELECT Gears FROM SlotThemes WHERE Theme=@Theme";
                                    cmd.Parameters.AddWithValue("@Theme", Theme);
                                    reader = cmd.ExecuteReader();
                                    if (reader.Read())
                                        SlotData2[slot] = reader["Gears"].ToString();
                                    reader.Close();
                                }
                            }
                            SlotThemes = SlotThemes2;
                            SlotData = SlotData2;
                            SlotDyes = SlotDyes2;
                        }
                    }
                    catch (Exception  ex) { SERVICES.Log.Info(ex.ToString()); }
                    connection.Close();
                    Save();
                }
            }
            catch (Exception ex) { SERVICES.Log.Info(ex.ToString()); }
        }

        public static DATAMANAGEMENT Load()
        {
            if (SERVICES.Interface.GetPluginConfig() is DATAMANAGEMENT DataManagement)
                return DataManagement;
            DataManagement = new DATAMANAGEMENT();
            DataManagement.Save();
            return DataManagement;
        }

        private void Save() => SERVICES.Interface.SavePluginConfig(this);
    }
}
