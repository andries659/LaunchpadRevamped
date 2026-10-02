using System;
using LaunchpadReloaded.Modifiers.Game.Crewmate;
using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;

namespace LaunchpadReloaded.Options.Modifiers.Crewmate;

public class MayorOptions : AbstractOptionGroup<MayorModifier>
{
    public override string GroupName => "launchpad.options.mayor";

    public override Func<bool> GroupVisible =>
        () => OptionGroupSingleton<CrewmateModifierOptions>.Instance.MayorChance > 0;

    [ModdedNumberOption("launchpad.options.mayor.extraVotes", 1, 3)]
    public float ExtraVotes { get; set; } = 1;

    [ModdedToggleOption("launchpad.options.mayor.allowMultipleVotesOnSamePlayer")]
    public bool AllowVotingTwice { get; set; } = true;
}