using LaunchpadReloaded.Roles.Neutral;
using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;

namespace LaunchpadReloaded.Options.Roles.Neutral;

public class ExecutionerOptions : AbstractOptionGroup<ExecutionerRole>
{
    public override string GroupName => "launchpad.options.executioner";

    [ModdedToggleOption("launchpad.options.executioner.canCallMeeting")]
    public bool CanCallMeeting { get; set; } = false;

    [ModdedEnumOption("launchpad.options.executioner.onTargetDeathExecutionerBecomes", typeof(ExecutionerBecomes))]
    public ExecutionerBecomes TargetDeathNewRole { get; set; } = ExecutionerBecomes.Jester;

    public enum ExecutionerBecomes
    {
        Crewmate,
        Jester,
    }
}