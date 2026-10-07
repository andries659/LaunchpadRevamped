using LaunchpadRevamped.Roles.Crewmate;
using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;

namespace LaunchpadRevamped.Options.Roles.Crewmate;

public class SnitchOptions : AbstractOptionGroup<SnitchRole>
{
    public override string GroupName => "launchpad.options.snitch";

    [ModdedNumberOption("launchpad.options.snitch.tasksLeftToExpose", 1, 5)]
    public float TasksLeftToExpose { get; set; } = 2;

    [ModdedToggleOption("launchpad.options.snitch.revealImpostors")]
    public bool RevealImpostors { get; set; } = true;
}
