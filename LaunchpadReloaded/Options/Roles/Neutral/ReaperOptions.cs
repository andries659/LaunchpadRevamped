using LaunchpadReloaded.Roles.Neutral;
using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.Utilities;

namespace LaunchpadReloaded.Options.Roles.Neutral;

public class ReaperOptions : AbstractOptionGroup<ReaperRole>
{
    public override string GroupName => "launchpad.options.reaper";

    [ModdedNumberOption("launchpad.options.reaper.collectionsToWin", 2, 8)]
    public float SoulCollections { get; set; } = 3;

    [ModdedNumberOption("launchpad.options.reaper.collectCooldown", 0, 60, 5, MiraNumberSuffixes.Seconds)]
    public float CollectCooldown { get; set; } = 20;
}