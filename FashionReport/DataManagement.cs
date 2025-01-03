using Dalamud.Configuration;
using Dalamud.Memory;
using FFXIVClientStructs.FFXIV.Component.GUI;
using MySql.Data.MySqlClient;
using OpenQA.Selenium.DevTools;
using OpenQA.Selenium.DevTools.V129.Debugger;
using OpenQA.Selenium.DevTools.V129.ServiceWorker;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata.Ecma335;
using System.Xml.Linq;
using static FashionReport.DYES;
#pragma warning disable IDE1006
#pragma warning disable CS8601

namespace FashionReport
{
    public class DATAMANAGEMENT : IPluginConfiguration
    {
        public int Version { get; set; } = 1;

        public SortedList<string, string> SlotThemes = new SortedList<string, string>();
        public SortedList<string, string> SlotData = new SortedList<string, string>();
        public SortedList<string, DYES.DYEINFO> SlotDyes = new SortedList<string, DYEINFO>();
        public string WeeklyTheme = "";
        public DateTime LastChecked = new DateTime(2000,1, 1, 0, 0, 0);
        public bool CheckedRecently = false;
        public string ChancesLeft = "";
        public string HighScore = "";
        public string[] Slots = { "Weapon", "Head", "Body", "Gloves", "Legs", "Boots", "Earrings", "Necklace", "Bracelet", "RightRing", "LeftRing" };
        public string[] Dyes = { "WeaponDye", "HeadDye", "BodyDye", "GlovesDye", "LegsDye", "BootsDye" };

        public unsafe bool GetDataFromGame(MAINWINDOW? Main = null)
        {
#pragma warning disable CS8602
            AtkUnitBase* addon = (AtkUnitBase*)SERVICES.GameGui.GetAddonByName("FashionCheck");
#pragma warning restore CS8602
            if ((nint)addon != IntPtr.Zero)
            {
                if ((((AtkUnitBase*)addon)->RootNode != null && ((AtkUnitBase*)addon)->RootNode->IsVisible()) && CheckedRecently == false)
                {
                    CheckedRecently = true;
                    if(Main != null) Main.Reset();
                    if (MemoryHelper.ReadSeStringNullTerminated((nint)addon->AtkValues[0].String).TextValue != WeeklyTheme)
                    {
                        WeeklyTheme = MemoryHelper.ReadSeStringNullTerminated((nint)addon->AtkValues[0].String).TextValue;
                        for (int count = 0; count < Slots.Length; count++)
                            SlotThemes[Slots[count]] = MemoryHelper.ReadSeStringNullTerminated((nint)addon->AtkValues[2 + (11 * count)].String).TextValue;
                        ChancesLeft = MemoryHelper.ReadSeStringNullTerminated((nint)addon->AtkValues[122].String).TextValue;
                        HighScore = MemoryHelper.ReadSeStringNullTerminated((nint)addon->AtkValues[123].String).TextValue;
                        SlotDyes.Clear();
                        SlotData.Clear();
                        AccessServerData();
                        this.Save();
                        return true;
                    }
                }
                else if (!(((AtkUnitBase*)addon)->RootNode != null && ((AtkUnitBase*)addon)->RootNode->IsVisible()))
                {
                    CheckedRecently = false;
                    return false;
                }
            }
            return false;
        }

        private string GetWeek(MySqlConnection Connector, string? Week = null)
        {
            string? Weekly = null;
            MySqlCommand cmd = new MySqlCommand();
            cmd.Connection = Connector;
            if (Week == null)
                cmd.CommandText = "SELECT * FROM FashionReport ORDER BY Week DESC LIMIT 1";
            else
            {
                cmd.CommandText = "SELECT * FROM FashionReport WHERE WeeklyTheme=@Weekly";
                cmd.Parameters.AddWithValue("@Weekly", Week);
            }
            MySqlDataReader reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                if (reader["WeeklyTheme"].ToString() != null && reader["WeeklyTheme"].ToString() != "")
                    Weekly = reader["WeeklyTheme"].ToString();
            }
            reader.Close();
            if (Weekly == null)
                return "";
            return Weekly;
        }

        private bool WeekExists(MySqlConnection Connector, string Weekly)
        {
            string? Temp = GetWeek(Connector, Weekly);
            if (Temp != null && Temp != "")
                return true;
            return false;
        }

        private void GetThemeData(MySqlConnection Connector)
        {
            SlotData.Clear();
            foreach (string slot in Slots)
            {
                if (SlotThemes.TryGetValue(slot, out string? Theme) && !string.IsNullOrEmpty(Theme))
                {
                    SERVICES.Log.Info($"Theme is {Theme}");
                    MySqlCommand cmd = new MySqlCommand();
                    cmd.Connection = Connector;
                    cmd.CommandText = "SELECT Gears FROM SlotThemes WHERE Theme=@Theme";
                    cmd.Parameters.AddWithValue("@Theme", Theme);
                    MySqlDataReader reader = cmd.ExecuteReader();
                    if (reader.Read())
                        SlotData[slot] = reader["Gears"].ToString();
                    SERVICES.Log.Info($"SlotData[slot] is {SlotData[slot].ToString()}");
                    reader.Close();
                    Save();
                }
            }
        }

        private void GetSlotThemesAndDyes(MySqlConnection Connector, string Weekly)
        {
            MySqlCommand cmd = new MySqlCommand();
            cmd.Connection = Connector;
            cmd.CommandText = "SELECT * FROM FashionReport WHERE WeeklyTheme=@Weekly";
            cmd.Parameters.AddWithValue("@Weekly", Weekly);
            MySqlDataReader reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                if (!string.IsNullOrEmpty(reader["WeeklyTheme"].ToString()))
                    WeeklyTheme = reader["WeeklyTheme"].ToString();
                SlotThemes = new SortedList<string, string>();
                foreach (string slot in Slots)
                {
                    try
                    {
                        string? Data = reader[slot].ToString();
                        if (!string.IsNullOrEmpty(Data))
                            SlotThemes[slot] = Data;
                    }
                    catch { }
                }
                SERVICES.Log.Info($"Clearing SlotDyes!");
                SlotDyes.Clear();
                foreach (string dye in Dyes)
                {
                    try
                    {
                        string? Data = reader[dye].ToString();
                        if (!string.IsNullOrEmpty(Data))
                            SlotDyes[dye.Replace("Dye", "")] = GetDyeInfo(Data);
                    }
                    catch { }
                }
            }
            reader.Close();
            Save();
        }

        public void AccessServerData()
        {
            try
            {
                Assembly assembly = Assembly.GetExecutingAssembly();
                string resourceName = "FashionReport.FashionReport.config";
#pragma warning disable CS8600
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
                    using (MySqlConnection connection = new MySqlConnection(connectionString))
                    {
                        connection.Open();
                        string? FinalWeek = GetWeek(connection);
                        if ((string.IsNullOrEmpty(WeeklyTheme)) || (!string.IsNullOrEmpty(WeeklyTheme) && WeekExists(connection, WeeklyTheme)))
                        {
                            GetSlotThemesAndDyes(connection, FinalWeek);
                            GetThemeData(connection);
                            Save();
                        }
                        connection.Close();
                    }
                }
            }
            catch {}
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
