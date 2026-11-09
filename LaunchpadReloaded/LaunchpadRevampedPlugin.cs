global using static Reactor.Utilities.Logger<LaunchpadRevamped.LaunchpadRevampedPlugin>;
using LaunchpadRevamped.Features;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using LaunchpadRevamped.Patches;
using LaunchpadReloaded.Wiki;
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

    /// <summary>
    /// Turns Launchpad's rearranged main menu on or off.
    /// </summary>
    public static ConfigEntry<bool> CustomMenuLayout { get; private set; } = null!;

    /// <summary>
    /// The rearranged menu is laid out for 16:9. When on, other window shapes get the normal menu.
    /// </summary>
    public static ConfigEntry<bool> MenuLayoutOnly16By9 { get; private set; } = null!;

    public override void Load()
    {
        CustomMenuLayout = Config.Bind("Menu", "CustomMenuLayout", true,
            "Use Launchpad's rearranged main menu. Turn this off if a button is missing or cannot be clicked.");
        MenuLayoutOnly16By9 = Config.Bind("Menu", "OnlyIn16By9", true,
            "The rearranged menu is built for 16:9 screens. When the window is a different shape, the normal menu is used instead.");

        Harmony.PatchAll();
        WikiBootstrap.Init();

        ReactorCredits.Register("Launchpad", Version, true, ReactorCredits.AlwaysShow);

        LprData.EnsureCreated();

        Config.Save();
    }
}
