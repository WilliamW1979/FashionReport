using Dalamud.Plugin.Services;
using Dalamud.Logging;
using FFXIVClientStructs.FFXIV.Component.GUI;
using System;
using System.Linq;
using System.Threading.Tasks;
using Dalamud.Memory;
using System.IO;
using System.Collections.Generic;
using System.Reflection;
using System.Xml.Linq;
using MySql.Data.MySqlClient;
using System.Threading;

namespace FashionReport
{
    public class FashionCheckDetector : IDisposable
    {
        private bool hasCheckedThisWindow = false;
        private Task? weeklyResetTask;
        private bool _StopTask = false;

        public FashionCheckDetector()
        {
            SERVICES.Framework.Update += OnFrameworkUpdate;
            StartWeeklyResetCheck();
        }

        private void StartWeeklyResetCheck()
        {
            weeklyResetTask = Task.Run(async () =>
            {
                while (!_StopTask)
                {
                    DateTime nowUtc = DateTime.UtcNow;
                    DateTime nextResetUtc = GetNextResetTimeUtc();
                    if (nowUtc >= nextResetUtc)
                    {
                        SERVICES.Framework.Update += OnFrameworkUpdate;
                        await Task.Delay(TimeSpan.FromMinutes(5));
                        nextResetUtc = GetNextResetTimeUtc().AddDays(7);
                    }
                    TimeSpan delay = nextResetUtc - nowUtc;
                    if (delay < TimeSpan.Zero)
                        delay = TimeSpan.FromHours(1);
                    await Task.Delay(delay > TimeSpan.FromMinutes(1) ? delay : TimeSpan.FromMinutes(1));
                }
            });
        }

        private DateTime GetNextResetTimeUtc()
        {
            DateTime nowUtc = DateTime.UtcNow;
            DayOfWeek targetDay = DayOfWeek.Tuesday;
            int targetHour = 8;
            int daysUntilTuesday = ((int)targetDay - (int)nowUtc.DayOfWeek + 7) % 7;
            DateTime nextTuesday = nowUtc.AddDays(daysUntilTuesday).Date.AddHours(targetHour);
            if (nextTuesday <= nowUtc)
                nextTuesday = nextTuesday.AddDays(7);
            return nextTuesday;
        }

        private unsafe void OnFrameworkUpdate(IFramework framework)
        {
            nint addonPtr = SERVICES.GameGui.GetAddonByName("FashionCheck");
            if (addonPtr == IntPtr.Zero)
            {
                hasCheckedThisWindow = false;
                return;
            }
            if (hasCheckedThisWindow) return;
            AtkUnitBase* addon = (AtkUnitBase*)addonPtr;
            if (addon == null) return;
            string WeeklyTheme = MemoryHelper.ReadSeStringNullTerminated((nint)(void*)addon->AtkValues[0].String).ToString() ?? string.Empty;
            SortedList<string, string> SlotThemes = new();
            for (int c = 0; c < SERVICES.FRSlots.Length; c++)
                SlotThemes[SERVICES.FRSlots[c]] = MemoryHelper.ReadSeStringNullTerminated((nint)(void*)addon->AtkValues[2 + (11 * c)].String).ToString() ?? string.Empty;
            CheckServer(WeeklyTheme, SlotThemes);
            hasCheckedThisWindow = true;
            SERVICES.Framework.Update -= OnFrameworkUpdate;
        }

        private MySqlConnection? GetConnector()
        {
            try
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

                    MySqlConnection connection = new($"Server={sServer};Database={sDatabase};User={sUser};Password={sPassword};Port={sPort};");
                    connection.Open();
                    return connection;
                }
            }
            catch (Exception ex)
            {
                SERVICES.Log.Error($"Error creating database connection: {ex}");
                return null;
            }
        }

        public WeeklyFashionReportData? GetServerData()
        {
            WeeklyFashionReportData reportData = new();
            try
            {
                using (MySqlConnection? connection = GetConnector())
                {
                    using (connection)
                    {
                        using (MySqlCommand cmd = new("SELECT * FROM WeeklyThemes ORDER BY Week DESC LIMIT 1;", connection))
                        {
                            using (MySqlDataReader reader = cmd.ExecuteReader())
                            {
                                if (reader.Read())
                                {
                                    reportData.StoredWeek = reader.GetUInt32("Week");
                                    reportData.WeeklyTheme = reader["WeeklyTheme"] != DBNull.Value ? reader.GetString("WeeklyTheme") : string.Empty;
                                    Dictionary<string, List<uint>?>? GetSlotTheme(string columnName, string slotName)
                                    {
                                        if (reader[columnName] != DBNull.Value)
                                        {
                                            string theme = reader.GetString(columnName);
                                            return new Dictionary<string, List<uint>?> { { theme, GetThemeDataInternal(theme, slotName) } };
                                        }
                                        return null;
                                    }

                                    reportData.Weapon = GetSlotTheme("Weapon", "Weapon");
                                    reportData.Head = GetSlotTheme("Head", "Head");
                                    reportData.Body = GetSlotTheme("Body", "Body");
                                    reportData.Gloves = GetSlotTheme("Gloves", "Gloves");
                                    reportData.Legs = GetSlotTheme("Legs", "Legs");
                                    reportData.Boots = GetSlotTheme("Boots", "Boots");
                                    reportData.Earrings = GetSlotTheme("Earrings", "Earrings");
                                    reportData.Necklace = GetSlotTheme("Necklace", "Necklace");
                                    reportData.Bracelet = GetSlotTheme("Bracelet", "Bracelet");
                                    reportData.RightRing = GetSlotTheme("RightRing", "RightRing");
                                    reportData.LeftRing = GetSlotTheme("LeftRing", "LeftRing");
                                }
                            }
                        }
                        return reportData;
                    }
                }
            }
            catch (MySqlException ex)
            {
                SERVICES.Log.Error($"MySQL Error: {ex}");
                return null;
            }
            catch (FileNotFoundException ex)
            {
                SERVICES.Log.Error($"Config file not found: {ex.FileName}");
                return null;
            }
            catch (Exception ex)
            {
                SERVICES.Log.Error($"General Error: {ex}");
                return null;
            }
        }

        private List<uint>? GetThemeDataInternal(string Theme, string Slot)
        {
            List<uint> itemIds = new List<uint>();
            using (MySqlConnection? connection = GetConnector())
            {
                using (connection)
                {
                    using (MySqlCommand cmd = new("SELECT ItemID FROM ThemeItems WHERE Theme=@theme AND Slot=@Slot", connection))
                    {
                        cmd.Parameters.AddWithValue("@theme", Theme);
                        cmd.Parameters.AddWithValue("@Slot", Slot);
                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                uint itemId = reader.GetUInt32("ItemID");
                                itemIds.Add(itemId);
                            }
                        }
                    }
                }
            }
            return itemIds.Count > 0 ? itemIds : null;
        }

        private void CheckServer(string weeklyThemeFromGame, SortedList<string, string> slotThemesFromGame)
        {
            using (MySqlConnection? connection = GetConnector())
            {
                using (MySqlCommand cmd = new("SELECT * FROM WeeklyThemes ORDER BY Week DESC LIMIT 1;", connection))
                {
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        string? dbWeeklyTheme = null;
                        SortedList<string, string?> dbSlotThemes = new();
                        if (reader.Read())
                        {
                            dbWeeklyTheme = reader["WeeklyTheme"] as string;
                            dbSlotThemes = new()
                        {
                            { "Weapon", reader["Weapon"] as string },
                            { "Head", reader["Head"] as string },
                            { "Body", reader["Body"] as string },
                            { "Gloves", reader["Gloves"] as string },
                            { "Legs", reader["Legs"] as string },
                            { "Boots", reader["Boots"] as string },
                            { "Earrings", reader["Earrings"] as string },
                            { "Necklace", reader["Necklace"] as string },
                            { "Bracelet", reader["Bracelet"] as string },
                            { "RightRing", reader["RightRing"] as string },
                            { "LeftRing", reader["LeftRing"] as string }
                        };
                        }
                        reader.Dispose();
                        cmd.Dispose();
                        connection?.Close();
                        bool themesDiffer = !CompareThemes(weeklyThemeFromGame, slotThemesFromGame, dbWeeklyTheme, dbSlotThemes);
                        if (weeklyThemeFromGame != dbWeeklyTheme || themesDiffer)
                            UpdateServer(weeklyThemeFromGame, slotThemesFromGame);
                    }
                }
            }
        }

        private bool CompareThemes(string gameWeeklyTheme, SortedList<string, string> gameSlotThemes, string? dbWeeklyTheme, SortedList<string, string?> dbSlotThemes)
        {
            if (gameWeeklyTheme != dbWeeklyTheme)
                return false;
            foreach (string slot in SERVICES.FRSlots)
                if (gameSlotThemes.TryGetValue(slot, out var gameTheme) && dbSlotThemes.TryGetValue(slot, out var dbTheme))
                {
                    if (gameTheme != dbTheme)
                        return false;
                }
                else if (gameSlotThemes.ContainsKey(slot) != dbSlotThemes.ContainsKey(slot))
                    return false;
            return true;
        }

        private void UpdateServer(string weeklyTheme, SortedList<string, string> slotThemes)
        {
            MySqlConnection? connection = GetConnector();
            string insertQuery = @"INSERT INTO WeeklyThemes (WeeklyTheme, Weapon, Head, Body, Gloves, Legs, Boots, Earrings, Necklace, Bracelet, RightRing, LeftRing, Week) VALUES (@WeeklyTheme, @Weapon, @Head, @Body, @Gloves, @Legs, @Boots, @Earrings, @Necklace, @Bracelet, @RightRing, @LeftRing, @CurrentWeek) ON DUPLICATE KEY UPDATE WeeklyTheme = @WeeklyTheme, Weapon = @Weapon, Head = @Head, Body = @Body, Gloves = @Gloves, Legs = @Legs, Boots = @Boots, Earrings = @Earrings, Necklace = @Necklace, Bracelet = @Bracelet, RightRing = @RightRing, LeftRing = @LeftRing, Week = @CurrentWeek;";
            using (MySqlCommand cmd = new(insertQuery, connection))
            {
                cmd.Parameters.AddWithValue("@WeeklyTheme", weeklyTheme);
                cmd.Parameters.AddWithValue("@Weapon", slotThemes.TryGetValue("Weapon", out string? weaponTheme) ? weaponTheme : (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Head", slotThemes.TryGetValue("Head", out string? headTheme) ? headTheme : (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Body", slotThemes.TryGetValue("Body", out string? bodyTheme) ? bodyTheme : (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Gloves", slotThemes.TryGetValue("Gloves", out string? glovesTheme) ? glovesTheme : (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Legs", slotThemes.TryGetValue("Legs", out string? legsTheme) ? legsTheme : (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Boots", slotThemes.TryGetValue("Boots", out string? bootsTheme) ? bootsTheme : (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Earrings", slotThemes.TryGetValue("Earrings", out string? earringsTheme) ? earringsTheme : (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Necklace", slotThemes.TryGetValue("Necklace", out string? necklaceTheme) ? necklaceTheme : (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Bracelet", slotThemes.TryGetValue("Bracelet", out string? braceletTheme) ? braceletTheme : (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@RightRing", slotThemes.TryGetValue("RightRing", out string? rightRingTheme) ? rightRingTheme : (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@LeftRing", slotThemes.TryGetValue("LeftRing", out string? leftRingTheme) ? leftRingTheme : (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@CurrentWeek", SERVICES.FRData.CurrentWeek);
                try
                {
                    cmd.ExecuteNonQuery();
                    SERVICES.Log.Info("Weekly themes updated on server.");

                    WeeklyFashionReportData newData = new WeeklyFashionReportData { Version = SERVICES.FRData.Version };
                    newData.WeeklyTheme = weeklyTheme;
                    newData.StoredWeek = SERVICES.FRData.CurrentWeek;
                    newData.LastServerCheck = DateTime.UtcNow;

                    foreach (string slot in SERVICES.FRSlots)
                    {
                        if (slotThemes.TryGetValue(slot, out string? theme))
                        {
                            string query = "SELECT ItemId FROM ThemeItems WHERE Theme = @theme AND Slot = @slot";
                            using (MySqlCommand itemCmd = new(query, connection))
                            {
                                itemCmd.Parameters.AddWithValue("@theme", theme);
                                itemCmd.Parameters.AddWithValue("@slot", slot);
                                using (MySqlDataReader itemReader = itemCmd.ExecuteReader())
                                {
                                    List<uint> itemIds = new List<uint>();
                                    while (itemReader.Read())
                                        if (itemReader["ItemID"] != DBNull.Value && uint.TryParse(itemReader["ItemID"].ToString(), out var itemId))
                                            itemIds.Add(itemId);
                                    switch (slot)
                                    {
                                        case "Weapon":
                                            newData.Weapon = new Dictionary<string, List<uint>?> { { theme, itemIds } };
                                            break;
                                        case "Head":
                                            newData.Head = new Dictionary<string, List<uint>?> { { theme, itemIds } };
                                            break;
                                        case "Body":
                                            newData.Body = new Dictionary<string, List<uint>?> { { theme, itemIds } };
                                            break;
                                        case "Gloves":
                                            newData.Gloves = new Dictionary<string, List<uint>?> { { theme, itemIds } };
                                            break;
                                        case "Legs":
                                            newData.Legs = new Dictionary<string, List<uint>?> { { theme, itemIds } };
                                            break;
                                        case "Boots":
                                            newData.Boots = new Dictionary<string, List<uint>?> { { theme, itemIds } };
                                            break;
                                        case "Earrings":
                                            newData.Earrings = new Dictionary<string, List<uint>?> { { theme, itemIds } };
                                            break;
                                        case "Necklace":
                                            newData.Necklace = new Dictionary<string, List<uint>?> { { theme, itemIds } };
                                            break;
                                        case "Bracelet":
                                            newData.Bracelet = new Dictionary<string, List<uint>?> { { theme, itemIds } };
                                            break;
                                        case "RightRing":
                                            newData.RightRing = new Dictionary<string, List<uint>?> { { theme, itemIds } };
                                            break;
                                        case "LeftRing":
                                            newData.LeftRing = new Dictionary<string, List<uint>?> { { theme, itemIds } };
                                            break;
                                    }
                                }
                            }
                        }
                    }
                    SERVICES.FRData.StoredWeek = newData.StoredWeek;
                    SERVICES.FRData.LastServerCheck = newData.LastServerCheck;
                    SERVICES.FRData.WeeklyTheme = newData.WeeklyTheme;
                    SERVICES.FRData.Weapon = newData.Weapon;
                    SERVICES.FRData.Head = newData.Head;
                    SERVICES.FRData.Body = newData.Body;
                    SERVICES.FRData.Gloves = newData.Gloves;
                    SERVICES.FRData.Legs = newData.Legs;
                    SERVICES.FRData.Boots = newData.Boots;
                    SERVICES.FRData.Earrings = newData.Earrings;
                    SERVICES.FRData.Necklace = newData.Necklace;
                    SERVICES.FRData.Bracelet = newData.Bracelet;
                    SERVICES.FRData.RightRing = newData.RightRing;
                    SERVICES.FRData.LeftRing = newData.LeftRing;
                    SERVICES.FRData.Save();
                    SERVICES.Log.Info("Local theme data updated.");
                }
                catch (MySqlException ex)
                {
                    SERVICES.Log.Error($"MySQL Error during UpdateServer: {ex}");
                }
            }
        }

        public void Dispose()
        {
            SERVICES.Framework.Update -= OnFrameworkUpdate;
            StopWeeklyResetCheck();
        }

        public void StopWeeklyResetCheck() => _StopTask = true;
    }
}
