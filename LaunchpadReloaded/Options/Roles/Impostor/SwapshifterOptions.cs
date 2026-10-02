using LaunchpadRevamped.Roles.Impostor;
using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.Utilities;

namespace LaunchpadRevamped.Options.Roles.Impostor;

public class SwapshifterOptions : AbstractOptionGroup<SwapshifterRole>
{
    public override string GroupName => "launchpad.options.swapshifter";

    [ModdedNumberOption("launchpad.options.swapshifter.swapCooldown", 0, 120, 5, MiraNumberSuffixes.Seconds)]
    public float SwapCooldown { get; set; } = 30;

    [ModdedNumberOption("launchpad.options.swapshifter.swapDuration", 0, 70, 5, MiraNumberSuffixes.Seconds)]
    public float SwapDuration { get; set; } = 15;

    [ModdedNumberOption("launchpad.options.swapshifter.swapUses", 0, 8, zeroInfinity: true)]
    public float SwapUses { get; set; } = 3;

    [ModdedToggleOption("launchpad.options.swapshifter.canSwapWithImpostors")]
    public bool CanSwapImpostors { get; set; } = true;
}