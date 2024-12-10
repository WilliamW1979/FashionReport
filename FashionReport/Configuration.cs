using Dalamud.Configuration;
using System;

namespace FashionReport;

[Serializable]
public class Configuration : IPluginConfiguration
{
    public int Version { get; set; } = 0;
    public bool chat { get; set; } = true;

    public bool IsConfigWindowMovable { get; set; } = true;
    public bool SomePropertyToBeSavedAndWithADefault { get; set; } = true;

    // the below exist just to make saving less cumbersome
    public void Save()
    {
        FASHIONREPORT.Interface.SavePluginConfig(this);
    }
}
