using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using Lumina.Excel.Sheets;

namespace FashionReport
{
    internal sealed class SERVICES
    {
        [PluginService] public static IDalamudPluginInterface Interface { get; private set; } = null!;
        [PluginService] public static IChatGui ChatGui { get; private set; } = null!;
        [PluginService] public static IClientState ClientState { get; private set; } = null!;
        [PluginService] public static ICommandManager CommandManager { get; private set; } = null!;
        [PluginService] public static ICondition Condition { get; private set; } = null!;
        [PluginService] public static IDataManager DataManager { get; private set; } = null!;
        [PluginService] public static IFramework Framework { get; private set; } = null!;
        [PluginService] public static IGameGui GameGui { get; private set; } = null!;
        [PluginService] public static IKeyState KeyState { get; private set; } = null!;
        [PluginService] public static IObjectTable ObjectTable { get; private set; } = null!;
        [PluginService] public static IPartyList PartyList { get; private set; } = null!;
        [PluginService] public static ITextureProvider TextureProvider { get; private set; } = null!;
        [PluginService] public static IPluginLog Log { get; private set; } = null!;
        [PluginService] public static IGameInventory GameInventory { get; private set; } = null!;

        public static readonly string[] FRSlots = { "Weapon", "Head", "Body", "Gloves", "Legs", "Boots", "Earrings", "Necklace", "Bracelet", "RightRing", "LeftRing" };
        public static List<Item> AllItems = null!;
        public static WeeklyFashionReportData FRData  = null!;
        public static EquippedGearService Equipment = null!;
    }
}
