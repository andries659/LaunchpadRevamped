using LaunchpadRevamped.Roles.Impostor;
using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.Utilities;

namespace LaunchpadRevamped.Options.Roles.Impostor;

public class JanitorOptions : AbstractOptionGroup<JanitorRole>
{
    public override string GroupName => "launchpad.options.janitor";

    [ModdedNumberOption("launchpad.options.janitor.hideBodiesCooldown", 0, 120, 5, MiraNumberSuffixes.Seconds)]
    public float HideCooldown { get; set; } = 5f;

    [ModdedNumberOption("launchpad.options.janitor.dragBodySpeed", 0.5f, 2.5f, 0.25f, MiraNumberSuffixes.None)]
    public float DragSpeed { get; set; } = 1.75f;

    [ModdedNumberOption("launchpad.options.janitor.hideBodiesUses", 0, 10, zeroInfinity: true)]
    public float HideUses { get; set; } = 3;

    [ModdedToggleOption("launchpad.options.janitor.cleanInsteadOfHide")]
    public bool CleanInsteadOfHide { get; set; } = false;
}