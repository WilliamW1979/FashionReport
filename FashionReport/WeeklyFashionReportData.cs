using System;
using System.Collections.Generic;
using Dalamud.Configuration;
using Dalamud.Plugin;

namespace FashionReport
{
    public class WeeklyFashionReportData : IPluginConfiguration
    {
        public int Version { get; set; } = 1;
        public string WeeklyTheme { get; set; } = string.Empty;
        public DateTime LastServerCheck { get; set; } = DateTime.MinValue;
        public uint StoredWeek { get; set; } = 0;
        public uint CurrentWeek
        {
            get
            {
                DateTime weekZeroUtc = new DateTime(2018, 1, 23, 8, 0, 0, DateTimeKind.Utc);
                return (uint)((DateTime.UtcNow - weekZeroUtc).TotalDays / 7);
            }
        }
        public Dictionary<string, List<uint>?>? Weapon { get; set; } = null;
        public Dictionary<string, List<uint>?>? Head { get; set; } = null;
        public Dictionary<string, List<uint>?>? Body { get; set; } = null;
        public Dictionary<string, List<uint>?>? Gloves { get; set; } = null;
        public Dictionary<string, List<uint>?>? Legs { get; set; } = null;
        public Dictionary<string, List<uint>?>? Boots { get; set; } = null;
        public Dictionary<string, List<uint>?>? Earrings { get; set; } = null;
        public Dictionary<string, List<uint>?>? Necklace { get; set; } = null;
        public Dictionary<string, List<uint>?>? Bracelet { get; set; } = null;
        public Dictionary<string, List<uint>?>? RightRing { get; set; } = null;
        public Dictionary<string, List<uint>?>? LeftRing { get; set; } = null;
        public string? WeaponDye { get; set; } = null;
        public string? HeadDye { get; set; } = null;
        public string? BodyDye { get; set; } = null;
        public string? GlovesDye { get; set; } = null;
        public string? LegsDye { get; set; } = null;
        public string? BootsDye { get; set; } = null;
        public void Save() => SERVICES.Interface.SavePluginConfig(this);
    }
}
