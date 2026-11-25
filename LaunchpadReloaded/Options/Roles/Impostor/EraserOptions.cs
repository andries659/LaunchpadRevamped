using LaunchpadRevamped.Roles.Impostor;
using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;

namespace LaunchpadRevamped.Options.Roles.Impostor;

public class EraserOptions : AbstractOptionGroup<EraserRole>
{
    public override string GroupName => "launchpad.options.eraser";

    [ModdedNumberOption("launchpad.options.eraser.eraseUses", 1, 15)]
    public float EraseUses { get; set; } = 2;
}
