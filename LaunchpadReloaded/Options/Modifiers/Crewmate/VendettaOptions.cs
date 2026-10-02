using System;
using LaunchpadRevamped.Modifiers.Game.Crewmate;
using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.PluginLoading;
using MiraAPI.Utilities;

namespace LaunchpadRevamped.Options.Modifiers.Crewmate;

[MiraIgnore]
public class VendettaOptions : AbstractOptionGroup<VendettaModifier>
{
    public override string GroupName => "launchpad.options.vendetta";

    public override Func<bool> GroupVisible =>
        () => OptionGroupSingleton<CrewmateModifierOptions>.Instance.VendettaChance > 0;

    [ModdedNumberOption("launchpad.options.vendetta.markCooldown", 5, 40, 2.5f, MiraNumberSuffixes.Seconds)]
    public float MarkCooldown { get; set; } = 15;
    
    [ModdedNumberOption("launchpad.options.vendetta.marksPerRound", 1, 3, 1f)]
    public float MarkUses { get; set; } = 1;
}