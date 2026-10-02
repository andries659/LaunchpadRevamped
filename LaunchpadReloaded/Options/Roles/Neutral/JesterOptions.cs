using LaunchpadReloaded.Roles.Neutral;
using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;

namespace LaunchpadReloaded.Options.Roles.Neutral;

public class JesterOptions : AbstractOptionGroup<JesterRole>
{
    public override string GroupName => "launchpad.options.jester";

    [ModdedToggleOption("launchpad.options.jester.canUseVents")]
    public bool CanUseVents { get; set; } = true;
}