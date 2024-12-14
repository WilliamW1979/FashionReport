using Dalamud.Configuration;
using Lumina.Excel.Sheets;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Configuration;

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
        if (sWeapon != "")
            sWeaponData = GetData(sWeapon);
        if (sHead != "")
            sHeadData = GetData(sHead);
        if (sBody != "")
            sBodyData = GetData(sBody);
        if (sGloves != "")
            sGlovesData = GetData(sGloves);
        if (sLegs != "")
            sLegsData = GetData(sLegs);
        if (sBoots != "")
            sBootsData = GetData(sBoots);
        if (sEarrings != "")
            sEarringsData = GetData(sEarrings);
        if (sNecklace != "")
            sNecklaceData = GetData(sNecklace);
        if (sBracelet!= "")
            sBraceletData = GetData(sBracelet);
        if (sRightRing != "")
            sRightRingData = GetData(sRightRing);
        if (sLeftRing != "")
            sLeftRingData = GetData(sLeftRing);
    }

    public string GetData(string sTheme)
    {
        string server = ConfigurationManager.ConnectionStrings["<MySQL String>"].ConnectionString;
        MySqlConnection conn = new MySqlConnection(server);
        conn.Open();
        MySqlCommand cmd = new MySqlCommand();
        cmd.Connection = conn;
        cmd.CommandText = "SELECT Gears FROM SlotThemes WHERE Theme=@theme";
        cmd.Parameters.AddWithValue("@theme", sTheme);
        MySqlDataReader reader = cmd.ExecuteReader();
        string? data = "";
        if (reader.Read())
            if((data = reader["Gears"].ToString()) == null)
                data = "";
        reader.Close();
        conn.Close();
        return data;
    }

    public void Save() => FASHIONREPORT.Interface.SavePluginConfig(this);

    public static CONFIGURATION Load()
    {
        if (FASHIONREPORT.Interface.GetPluginConfig() is CONFIGURATION Configuration)
            return Configuration;
        Configuration = new CONFIGURATION();
        Configuration.Save();
        return Configuration;
    }
}
