using System;
using LaunchpadRevamped.Modifiers.Game.Crewmate;
using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;

namespace LaunchpadRevamped.Options.Modifiers.Crewmate;

public class TorchOptions : AbstractOptionGroup<TorchModifier>
{
    public override string GroupName => "launchpad.options.torch";

    public override Func<bool> GroupVisible =>
        () => OptionGroupSingleton<CrewmateModifierOptions>.Instance.TorchChance > 0;

    [ModdedToggleOption("launchpad.options.torch.useHideNSeekFlashlight")]
    public bool UseFlashlight { get; set; } = true;

    public ModdedNumberOption TorchFlashlightSize { get; } = new("launchpad.options.torch.flashlightSize", .25f, 0.1f, .5f, 0.05f, MiraNumberSuffixes.Multiplier)
    {
        Visible = () => OptionGroupSingleton<TorchOptions>.Instance.UseFlashlight,
    };
}