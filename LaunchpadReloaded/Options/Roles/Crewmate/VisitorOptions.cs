using LaunchpadRevamped.Roles.Crewmate;
using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.Utilities;

namespace LaunchpadRevamped.Options.Roles.Crewmate;

public class VisitorOptions : AbstractOptionGroup<VisitorRole>
{
    public override string GroupName => "launchpad.options.visitor";

    [ModdedNumberOption("launchpad.options.visitor.visitUses", 1, 5, 1, MiraNumberSuffixes.None)]
    public float VisitUses { get; set; } = 1;

    [ModdedNumberOption("launchpad.options.visitor.visitCooldown", 0, 30, 1, MiraNumberSuffixes.Seconds)]
    public float VisitCooldown { get; set; } = 5;

    [ModdedToggleOption("launchpad.options.visitor.revealKiller")]
    public bool RevealKiller { get; set; } = false;
}
