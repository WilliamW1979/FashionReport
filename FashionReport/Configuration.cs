using Dalamud.Configuration;
using Lumina.Excel.Sheets;
using MySql.Data.MySqlClient;
using MySqlX.XDevAPI;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using static FashionReport.Windows.MAINWINDOW;

namespace FashionReport;

[Serializable]
public class CONFIGURATION : IPluginConfiguration
{
    public int Version { get; set; } = 1;

    public List<Item> lItems = new List<Item>();

    public DateTime dtLastChecked = new DateTime(2000,1,1,0,0,0);
    public string sWeeklyTheme = "";
    public string sWeapon = "";
    public string sHead = "";
    public string sBody = "";
    public string sGloves = "";
    public string sLegs = "";
    public string sBoots = "";
    public string sEarrings = "";
    public string sNecklace = "";
    public string sBracelet = "";
    public string sRightRing = "";
    public string sLeftRing = "";
    public string sWeaponData = "";
    public string sHeadData = "";
    public string sBodyData = "";
    public string sGlovesData = "";
    public string sLegsData = "";
    public string sBootsData = "";
    public string sEarringsData = "";
    public string sNecklaceData = "";
    public string sBraceletData = "";
    public string sRightRingData = "";
    public string sLeftRingData = "";
    public bool bCurrent = false;
    public DyeInfo WeaponDye = new DyeInfo();
    public DyeInfo HeadDye = new DyeInfo();
    public DyeInfo BodyDye = new DyeInfo();
    public DyeInfo GlovesDye = new DyeInfo();
    public DyeInfo LegsDye = new DyeInfo();
    public DyeInfo BootsDye = new DyeInfo();


    public string sChancesLeft = "";
    public string sHighScore = "";

    public void Reset()
    {
        dtLastChecked = new DateTime(2000, 1, 1, 0, 0, 0);
        sWeeklyTheme = "";
        sWeapon = sHead = sBody = sGloves = sLegs = sBoots = sEarrings = sNecklace = sBracelet = sRightRing = sLeftRing = "";
        sWeaponData = sHeadData = sBodyData = sGlovesData = sLegsData = sBootsData = sEarringsData = sNecklaceData = sBraceletData = sRightRingData = sLeftRingData = "";
    }

    public void Populate()
    {
        if (sWeapon != "") sWeaponData = GetData(sWeapon);
        if (sHead != "") sHeadData = GetData(sHead);
        if (sBody != "") sBodyData = GetData(sBody);
        if (sGloves != "") sGlovesData = GetData(sGloves);
        if (sLegs != "") sLegsData = GetData(sLegs);
        if (sBoots != "") sBootsData = GetData(sBoots);
        if (sEarrings != "") sEarringsData = GetData(sEarrings);
        if (sNecklace != "") sNecklaceData = GetData(sNecklace);
        if (sBracelet!= "") sBraceletData = GetData(sBracelet);
        if (sRightRing != "") sRightRingData = GetData(sRightRing);
        if (sLeftRing != "") sLeftRingData = GetData(sLeftRing);
    }

    public string GetData(string sTheme)
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


                string? sServer = settings.Elements("add").FirstOrDefault(e => (string)e.Attribute("key") == "Server")?.Attribute("value")?.Value;
                string? sDatabase = settings.Elements("add").FirstOrDefault(e => (string)e.Attribute("key") == "Database")?.Attribute("value")?.Value;
                string? sUser = settings.Elements("add").FirstOrDefault(e => (string)e.Attribute("key") == "User")?.Attribute("value")?.Value;
                string? sPassword = settings.Elements("add").FirstOrDefault(e => (string)e.Attribute("key") == "Password")?.Attribute("value")?.Value;
                string? sPort = settings.Elements("add").FirstOrDefault(e => (string)e.Attribute("key") == "Port")?.Attribute("value")?.Value;

                string connectionString = $"Server={sServer};Database={sDatabase};User={sUser};Password={sPassword};Port={sPort};";

                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    MySqlCommand cmd = new MySqlCommand();
                    cmd.Connection = connection;
                    cmd.CommandText = "SELECT Gears FROM SlotThemes WHERE Theme=@theme";
                    cmd.Parameters.AddWithValue("@theme", sTheme);
                    MySqlDataReader reader = cmd.ExecuteReader();
                    string? data = "";
                    if (reader.Read())
                        data = reader["Gears"].ToString();
                    reader.Close();
                    connection.Close();
                    if (data == null)
                        return "";
                    return data;
                }
            }
#pragma warning restore CS8600
        }
        catch (Exception)
        {
            return "";
        }
    }

    public void PopulateDyes()
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


                string? sServer = settings.Elements("add").FirstOrDefault(e => (string)e.Attribute("key") == "Server")?.Attribute("value")?.Value;
                string? sDatabase = settings.Elements("add").FirstOrDefault(e => (string)e.Attribute("key") == "Database")?.Attribute("value")?.Value;
                string? sUser = settings.Elements("add").FirstOrDefault(e => (string)e.Attribute("key") == "User")?.Attribute("value")?.Value;
                string? sPassword = settings.Elements("add").FirstOrDefault(e => (string)e.Attribute("key") == "Password")?.Attribute("value")?.Value;
                string? sPort = settings.Elements("add").FirstOrDefault(e => (string)e.Attribute("key") == "Port")?.Attribute("value")?.Value;

                string connectionString = $"Server={sServer};Database={sDatabase};User={sUser};Password={sPassword};Port={sPort};";

                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    MySqlCommand cmd = new MySqlCommand();
                    cmd.Connection = connection;
                    cmd.CommandText = "SELECT * FROM FashionReport ORDER BY Week DESC LIMIT 1";
                    MySqlDataReader reader = cmd.ExecuteReader();
                    if (reader.Read())
                    {
                        string? WeekTheme = reader["WeeklyTheme"].ToString();
                        if (WeekTheme == sWeeklyTheme)
                        {
                            string? dye = reader["WeaponDye"].ToString();
                            if (dye != null && dye != "") WeaponDye = GetDyeInfo(dye);
                            dye = reader["HeadDye"].ToString();
                            if (dye != null && dye != "") HeadDye = GetDyeInfo(dye);
                            dye = reader["BodyDye"].ToString();
                            if (dye != null && dye != "") BodyDye = GetDyeInfo(dye);
                            dye = reader["GlovesDye"].ToString();
                            if (dye != null && dye != "") GlovesDye = GetDyeInfo(dye);
                            dye = reader["LegsDye"].ToString();
                            if (dye != null && dye != "") LegsDye = GetDyeInfo(dye);
                            dye = reader["BootsDye"].ToString();
                            if (dye != null && dye != "") BootsDye = GetDyeInfo(dye);
                        }
                    }
                    reader.Close();
                    connection.Close();
                }
            }
#pragma warning restore CS8600
        }
        catch (Exception)
        {
            WeaponDye = new DyeInfo();
            HeadDye = new DyeInfo();
            BodyDye = new DyeInfo();
            GlovesDye = new DyeInfo();
            LegsDye = new DyeInfo();
            BootsDye = new DyeInfo();
        }

    }

    public DyeInfo GetDyeInfo(string DyeName)
    {
        DyeName = DyeName.Replace("Dye", "");
        DyeName = DyeName.Trim();
        Lumina.Excel.ExcelSheet<Stain> stainSheet = SERVICES.DataManager.GetExcelSheet<Stain>();
        IEnumerable<Stain> Dyes = stainSheet.Where<Stain>(stain => stain.Name == DyeName);
        Stain Dye = new Stain();
        if (Dyes.Count() >= 1) Dye = Dyes.First();
        else return new DyeInfo { DyeId = 0 }; // Return null if no match is found

        if (Dye.RowId != 0)
        {
            return new DyeInfo
            {
                DyeId = Dye.RowId,
                DyeName = Dye.Name.ExtractText(), // Name of the dye
                DyeColor = Dye.Color, // The color
                DyeShade = Dye.Shade // The shade
            };
        }
        return new DyeInfo { DyeId = 0 }; // Return null if no match is found
    }

    public void Save() => SERVICES.Interface.SavePluginConfig(this);

    public static CONFIGURATION Load()
    {
        if (SERVICES.Interface.GetPluginConfig() is CONFIGURATION Configuration)
            return Configuration;
        Configuration = new CONFIGURATION();
        Configuration.Save();
        return Configuration;
    }
}
