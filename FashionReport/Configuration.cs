using Dalamud.Configuration;
using Lumina.Excel.Sheets;
using MySql.Data.MySqlClient;
using MySqlX.XDevAPI;
using MySqlX.XDevAPI.Common;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using Org.BouncyCastle.Bcpg.Sig;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using static FashionReport.DYES;
using static FashionReport.MAINWINDOW;

namespace FashionReport;

[Serializable]
public class CONFIGURATION : IPluginConfiguration
{
    public int Version { get; set; } = 1;

    public List<Item> lItems = new List<Item>();

    public DateTime dtLastChecked = new DateTime(2000,1,1,0,0,0);
    public string sWeeklyTheme = "";
    public List<string> SlotThemes = new List<string>();
    public List<string> SlotData = new List<string>();

    public bool bCurrent = false;
    public DYES.DYEINFO WeaponDye = new DYEINFO();
    public DYES.DYEINFO HeadDye = new DYEINFO();
    public DYES.DYEINFO BodyDye = new DYEINFO();
    public DYES.DYEINFO GlovesDye = new DYEINFO();
    public DYES.DYEINFO LegsDye = new DYEINFO();
    public DYES.DYEINFO BootsDye = new DYEINFO();
    public SortedList<Item, bool> IsCraftable = new SortedList<Item, bool>();
    public SortedList<Item, bool> IsVendor = new SortedList<Item, bool>();
    public SortedList<Item, bool> IsPvP = new SortedList<Item, bool>();
    public SortedList<Item, bool> IsDesynthesis = new SortedList<Item, bool>();
    public SortedList<Item, bool> IsDungeon = new SortedList<Item, bool>();
    public SortedList<Item, bool> IsTrial = new SortedList<Item, bool>();
    public SortedList<Item, bool> IsQuestReward = new SortedList<Item, bool>();

    public string sChancesLeft = "";
    public string sHighScore = "";

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
