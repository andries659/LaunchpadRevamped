using System;
using LaunchpadRevamped.Modifiers.Game.Universal;
using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.Utilities;

namespace LaunchpadRevamped.Options.Modifiers.Universal;

public class BaitOptions : AbstractOptionGroup<BaitModifier>
{
    public override string GroupName => "launchpad.options.bait";

    public override Func<bool> GroupVisible =>
        () => OptionGroupSingleton<UniversalModifierOptions>.Instance.BaitChance > 0;

    [ModdedNumberOption("launchpad.options.bait.delayMin", 0, 5, 1, MiraNumberSuffixes.Seconds)]
    public float DelayMin { get; set; } = 0;

    [ModdedNumberOption("launchpad.options.bait.delayMax", 0, 10, 1, MiraNumberSuffixes.Seconds)]
    public float DelayMax { get; set; } = 0;

    [ModdedToggleOption("launchpad.options.bait.notifyKiller")]
    public bool NotifyKiller { get; set; } = false;

    [ModdedToggleOption("launchpad.options.bait.showAliveBaits")]
    public bool ShowAliveBaits { get; set; } = false;
}
