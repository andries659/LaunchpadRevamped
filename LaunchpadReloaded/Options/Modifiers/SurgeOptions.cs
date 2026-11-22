using System;
using LaunchpadRevamped.Modifiers.Game.Impostor;
using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.Utilities;

namespace LaunchpadRevamped.Options.Modifiers;

public class SurgeOptions : AbstractOptionGroup<SurgeModifier>
{
    public override string GroupName => "launchpad.options.surge";

    public override Func<bool> GroupVisible =>
        () => OptionGroupSingleton<ImpostorModifierOptions>.Instance.SurgeChance > 0;

    [ModdedNumberOption("launchpad.options.surge.speedBoost", 1.1f, 2f, 0.1f, MiraNumberSuffixes.Multiplier)]
    public float SpeedBoost { get; set; } = 1.5f;

    [ModdedNumberOption("launchpad.options.surge.duration", 2, 15, 1, MiraNumberSuffixes.Seconds)]
    public float Duration { get; set; } = 5;
}
