using BepInEx;
using BepInEx.Configuration;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;

namespace ServerAdd;

[BepInAutoPlugin]
[BepInProcess("Among Us.exe")]
public partial class ServerAdd : BasePlugin
{
    public Harmony Harmony { get; } = new(Id);
    public static ConfigEntry<string> Ip { get; set; }
    public static ConfigEntry<ushort> Port { get; set; }
    public static ConfigEntry<bool> isHttps { get; set; }
    public static ConfigEntry<bool> isDNS { get; set; }
    public static ConfigEntry<string> ServerName { get; set; }
    public override void Load()
    {
        Ip = Config.Bind("Custom", "Custom Server IP", "127.0.0.1");
        Port = Config.Bind("Custom", "Custom Server Port", (ushort)22000);
        isHttps = Config.Bind("Custom", "Custom Server isHttps", false);
        isHttps = Config.Bind("Custom", "Custom Server isHttps", false);
        ServerName = Config.Bind("Custom", "Custom Server Name", "Custom");

        ModManager.Instance.ShowModStamp();
        Harmony.PatchAll();
    }

}
