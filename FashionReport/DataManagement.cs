using Dalamud.Configuration;
using Dalamud.Memory;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using FFXIVClientStructs.FFXIV.Component.GUI;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using static FashionReport.DYES;

namespace FashionReport
{
    public class DATAMANAGEMENT : IPluginConfiguration
    {
        public int Version { get; set; } = 1;
        public SortedList<string, string> SlotThemes { get; private set; } = new SortedList<string, string>();
        public SortedList<string, string> SlotData { get; private set; } = new SortedList<string, string>();
        public SortedList<string, DYES.DYEINFO> SlotDyes { get; private set; } = new SortedList<string, DYEINFO>();
        public uint Week { get; private set; } = 0;
        public string WeeklyTheme { get; private set; } = string.Empty;
        public readonly string[] Slots = { "Weapon", "Head", "Body", "Gloves", "Legs", "Boots", "Earrings", "Necklace", "Bracelet", "RightRing", "LeftRing" };
        public readonly string[] Dyes = { "WeaponDye", "HeadDye", "BodyDye", "GlovesDye", "LegsDye", "BootsDye" };

        public bool IsDyes() => SlotDyes.Values.All(dye => dye.DyeId != 0);

        public bool IsError()
        {
            return Slots.Any(slot => SlotThemes.TryGetValue(slot, out string? theme) && !string.IsNullOrEmpty(theme) && SlotData.TryGetValue(slot, out string? data) && string.IsNullOrEmpty(data));
        }

        public DateTime CalculateTuesday()
        {
            DateTime utcNow = DateTime.UtcNow;
            DateTime nextTuesday = utcNow.Date.AddDays(((int)DayOfWeek.Tuesday - (int)utcNow.DayOfWeek + 7) % 7);
            DateTime target = nextTuesday.AddHours(8);
            if (utcNow >= target)
                target = target.AddDays(7);
            return target;
        }

        public DateTime CalculateFriday()
        {
            DateTime utcNow = DateTime.UtcNow;
            DateTime weekStart = GetWeekStartUtc(utcNow);
            return weekStart.AddDays(3);
        }

        private DateTime GetWeekStartUtc(DateTime utcNow)
        {
            int daysSinceTuesday = ((int)utcNow.DayOfWeek - (int)DayOfWeek.Tuesday + 7) % 7;
            DateTime lastTuesday = utcNow.Date.AddDays(-daysSinceTuesday).AddHours(8);
            return utcNow >= lastTuesday ? lastTuesday : lastTuesday.AddDays(-7);
        }

        public int GetCurrentWeek()
        {
            DateTime beginning = new DateTime(2018, 1, 23, 7, 0, 0);
            return (int)((DateTime.Now - beginning).TotalDays / 7);
        }

        private string GetConnectionString()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            string resourceName = "FashionReport.FashionReport.config";
            using (Stream? stream = assembly.GetManifestResourceStream(resourceName))
            {
                if (stream == null)
                    throw new FileNotFoundException(resourceName);

                XDocument configFile = XDocument.Load(stream);
                XElement? settings = configFile.Element("configuration")?.Element("databaseSettings");
                if (settings == null)
                    throw new Exception("Invalid configuration file format.");

                string sServer = settings.Elements("add").FirstOrDefault(data => (string?)data.Attribute("key") == "Server")?.Attribute("value")?.Value ?? string.Empty;
                string sDatabase = settings.Elements("add").FirstOrDefault(data => (string?)data.Attribute("key") == "Database")?.Attribute("value")?.Value ?? string.Empty;
                string sUser = settings.Elements("add").FirstOrDefault(data => (string?)data.Attribute("key") == "User")?.Attribute("value")?.Value ?? string.Empty;
                string sPassword = settings.Elements("add").FirstOrDefault(data => (string?)data.Attribute("key") == "Password")?.Attribute("value")?.Value ?? string.Empty;
                string sPort = settings.Elements("add").FirstOrDefault(data => (string?)data.Attribute("key") == "Port")?.Attribute("value")?.Value ?? string.Empty;

                return $"Server={sServer};Database={sDatabase};User={sUser};Password={sPassword};Port={sPort};";
            }
        }

        private string BuildInsertCommand(string weeklyTheme2, SortedList<string, string> slotThemes2)
        {
            string cmdstr = $"INSERT INTO FashionReport VALUES({GetCurrentWeek()}, '{weeklyTheme2.Replace("'", "\\'")}', " + string.Join(", ", Slots.Select(slot => slotThemes2.TryGetValue(slot, out string? theme) && !string.IsNullOrEmpty(theme) ? $"'{theme.Replace("'", "\\'")}'" : "null")) + $", {CalculateFriday().ToBinary()}, null, null, null, null, null, null)";
            return cmdstr.Replace("'null'", "null");
        }

        private void ExecuteInsertCommand(MySqlConnection connection, string cmdstr)
        {
            using (MySqlCommand cmd = new MySqlCommand(cmdstr, connection))
                cmd.ExecuteNonQuery();
        }

        public unsafe bool ReadDataFromGame()
        {
            if (Week == (uint)GetCurrentWeek()) return false;
            AtkUnitBase* addon = (AtkUnitBase*)SERVICES.GameGui.GetAddonByName("FashionCheck");
            if ((nint)addon == IntPtr.Zero || !addon->RootNode->IsVisible()) return false;
            SortedList<string, string> slotThemes2 = new SortedList<string, string>();
            string weeklyTheme2 = MemoryHelper.ReadSeStringNullTerminated((nint)(void*)addon->AtkValues[0].String).ToString() ?? string.Empty;
            if (weeklyTheme2 == WeeklyTheme)
            {
                Week = (uint)GetCurrentWeek();
                return false;
            }

            for (int count = 0; count < Slots.Length; count++)
                slotThemes2[Slots[count]] = MemoryHelper.ReadSeStringNullTerminated((nint)(void*)addon->AtkValues[2 + (11 * count)].String).TextValue ?? string.Empty;

            try
            {
                using (MySqlConnection connection = new MySqlConnection(GetConnectionString()))
                {
                    connection.Open();
                    string cmdstr = BuildInsertCommand(weeklyTheme2, slotThemes2);
                    ExecuteInsertCommand(connection, cmdstr);

                    foreach (string slot in Slots)
                    {
                        if (slotThemes2.TryGetValue(slot, out string? theme) && !string.IsNullOrEmpty(theme))
                        {
                            MySqlCommand cmd = new MySqlCommand("SELECT Gears FROM SlotThemes WHERE Theme=@Theme", connection);
                            cmd.Parameters.AddWithValue("@Theme", theme);
                            using (MySqlDataReader reader = cmd.ExecuteReader())
                                if (reader.Read())
                                    SlotData[slot] = reader["Gears"]?.ToString() ?? string.Empty;
                        }
                    }

                    Week = (uint)GetCurrentWeek();
                    WeeklyTheme = weeklyTheme2;
                    SlotThemes = slotThemes2;
                    SlotDyes.Clear();
                    Save();
                }
            }
            catch (Exception ex)
            {
                SERVICES.Log.Info(ex.ToString());
            }

            return true;
        }

        public void ReadDataFromServer()
        {
            try
            {
                string connectionString = GetConnectionString();
                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    if (connection.State != ConnectionState.Open)
                    {
                        connection.Dispose();
                        return;
                    }
                    string sCommand = "SELECT * FROM FashionReport ORDER BY Week DESC LIMIT 1";
                    if(connection.State == ConnectionState.Open)
                        using (MySqlCommand command = new MySqlCommand(sCommand, connection))
                            using (MySqlDataReader reader = command.ExecuteReader())
                                if (reader.Read())
                                {
                                    Week = reader.GetUInt32("Week");
                                    WeeklyTheme = reader["WeeklyTheme"]?.ToString() ?? string.Empty;
                                    foreach (string slot in Slots)
                                        if (reader[slot] is string data && !string.IsNullOrEmpty(data))
                                            SlotThemes[slot] = data;
                                    foreach (string dye in Dyes)
                                        if (reader[dye] is string dyeData && !string.IsNullOrEmpty(dyeData))
                                            SlotDyes[dye.Replace("Dye", "")] = GetDyeInfo(dyeData);
                                }
                }
            }
            catch (Exception ex)
            {
                try
                {
                    SERVICES.Log.Info(ex.ToString());
                }
                catch { }
            }
        }

        private void WriteDataToServer()
        {
            try
            {
                string connectionString = GetConnectionString();
                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    SERVICES.Log.Info($"Week: {GetCurrentWeek().ToString()} ... updating to database!");
                    string cmdstr = @"INSERT INTO FashionReport (Week, WeeklyTheme, Weapon, Head, Body, Gloves, Legs, Boots, Earrings, Necklace, Bracelet, RightRing, LeftRing) VALUES (@Week, @WeeklyTheme, @Weapon, @Head, @Body, @Gloves, @Legs, @Boots, @Earrings, @Necklace, @Bracelet, @RightRing, @LeftRing) ON DUPLICATE KEY UPDATE WeeklyTheme = @WeeklyTheme, Weapon = @Weapon, Head = @Head, Body = @Body, Gloves = @Gloves, Legs = @Legs, Boots = @Boots, Earrings = @Earrings, Necklace = @Necklace, Bracelet = @Bracelet, RightRing = @RightRing, LeftRing = @LeftRing;";
                    using (MySqlCommand cmd = new MySqlCommand(cmdstr, connection))
                    {
                        if (cmd == null)
                        {
                            SERVICES.Log.Info("cmd is null! Server not updated!");
                            return;
                        }
                        cmd.Parameters.AddWithValue("@Week", GetCurrentWeek());
                        cmd.Parameters.AddWithValue("@WeeklyTheme", WeeklyTheme ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@Weapon", SlotThemes.TryGetValue("Weapon", out var weapon) ? weapon : (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@Head", SlotThemes.TryGetValue("Head", out var head) ? head : (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@Body", SlotThemes.TryGetValue("Body", out var body) ? body : (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@Gloves", SlotThemes.TryGetValue("Gloves", out var gloves) ? gloves : (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@Legs", SlotThemes.TryGetValue("Legs", out var legs) ? legs : (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@Boots", SlotThemes.TryGetValue("Boots", out var boots) ? boots : (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@Earrings", SlotThemes.TryGetValue("Earrings", out var earrings) ? earrings : (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@Necklace", SlotThemes.TryGetValue("Necklace", out var necklace) ? necklace : (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@Bracelet", SlotThemes.TryGetValue("Bracelet", out var bracelet) ? bracelet : (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@RightRing", SlotThemes.TryGetValue("RightRing", out var rightRing) ? rightRing : (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@LeftRing", SlotThemes.TryGetValue("LeftRing", out var leftRing) ? leftRing : (object)DBNull.Value);
                        cmd.ExecuteNonQuery();
                        SERVICES.Log.Info($"Sent: {cmd.CommandText} to database");
                    }
                }
            }
            catch (Exception ex)
            {
                try
                {
                    SERVICES.Log.Info(ex.ToString());
                }
                catch { }
            }
        }

        public static DATAMANAGEMENT Load()
        {
            if (SERVICES.Interface.GetPluginConfig() is DATAMANAGEMENT dataManagement)
                return dataManagement;
            dataManagement = new DATAMANAGEMENT();
            dataManagement.Save();
            return dataManagement;
        }

        private void Save() => SERVICES.Interface.SavePluginConfig(this);
    }
}
