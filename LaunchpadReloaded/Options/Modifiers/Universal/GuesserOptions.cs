using System;
using LaunchpadRevamped.Modifiers.Game.Universal;
using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;

namespace LaunchpadRevamped.Options.Modifiers.Universal;

public class GuesserOptions : AbstractOptionGroup<GuesserModifier>
{
    public override string GroupName => "launchpad.options.guesser";

    public override Func<bool> GroupVisible =>
        () => OptionGroupSingleton<UniversalModifierOptions>.Instance.GuesserChance > 0;

    [ModdedNumberOption("launchpad.options.guesser.maxGuessesPerGame", 1, 15)]
    public float MaxGuessesPerGame { get; set; } = 3;

    [ModdedNumberOption("launchpad.options.guesser.maxGuessesPerMeeting", 1, 5)]
    public float MaxGuessesPerMeeting { get; set; } = 1;

    [ModdedToggleOption("launchpad.options.guesser.canGuessBasicRoles")]
    public bool CanGuessBasicRoles { get; set; } = false;
}
