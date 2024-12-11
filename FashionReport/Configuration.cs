using Dalamud.Configuration;
using FashionReport.Windows;
using ImGuiNET;
using Lumina.Excel.Sheets;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using static FFXIVClientStructs.ThisAssembly.Git;

namespace FashionReport;

[Serializable]
public class CONFIGURATION : IPluginConfiguration
{
    public int Version { get; set; } = 1;

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

    public void Log(string message)
    {
        StreamWriter sw = new StreamWriter("D:\\C\\FashionReport\\FashionReport\\bin\\x64\\Debug\\log.txt", true);
        sw.WriteLine(message);
        sw.Close();
    }

    public string GetData(string sTheme)
    {
        Log("Made it!");
        MySqlConnection conn = new MySqlConnection("PORT=19137;SERVER=srankhunter-srankhunter.f.aivencloud.com;DATABASE=FashionReport;UID=FRMod;PASSWORD=jgv90jRnioasDfioFhnbweo;");
        conn.Open();
        MySqlCommand cmd = new MySqlCommand();
        cmd.Connection = conn;
        cmd.CommandText = "SELECT Gears FROM SlotThemes WHERE Theme=@theme";
        cmd.Parameters.AddWithValue("@theme", sTheme);
        MySqlDataReader reader = cmd.ExecuteReader();
        string? data = "";
        if (reader.Read())
        {
            data = reader["Gears"].ToString();
            if (data == null) data = "";
        }
        reader.Close();
        conn.Close();
        Log(data);
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
