global using static Reactor.Utilities.Logger<LaunchpadRevamped.LaunchpadRevampedPlugin>;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using LaunchpadRevamped.Patches;
using MiraAPI;
using MiraAPI.PluginLoading;
using MiraAPI.Utilities;
using MiraAPI.Translation;
using Reactor;
using Reactor.Networking;
using Reactor.Networking.Attributes;
using Reactor.Utilities;

namespace LaunchpadRevamped;

[BepInAutoPlugin("dev.andriess.launchpad", "LaunchpadRevamped")]
[BepInProcess("Among Us.exe")]
[BepInDependency(ReactorPlugin.Id)]
[BepInDependency(MiraApiPlugin.Id)]
[BepInDependency(CrowdedModPatch.CrowdedId, BepInDependency.DependencyFlags.SoftDependency)]
[ReactorModFlags(ModFlags.RequireOnAllClients)]
public partial class LaunchpadRevampedPlugin : BasePlugin, IMiraPlugin
{
    private Harmony Harmony { get; } = new(Id);
    public ConfigFile GetConfigFile()
    {
        return Config;
    }

    public LaunchpadRevampedPlugin()
    {
        MiraLocaleManager.Register(Id);
    }

    public string OptionsTitleText => "Launchpad";

    public override void Load()
    {
        Harmony.PatchAll();

        ReactorCredits.Register("Launchpad", Version, true, ReactorCredits.AlwaysShow);

        Config.Save();
    }
}