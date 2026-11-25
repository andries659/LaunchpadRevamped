using LaunchpadRevamped.Roles.Coven;
using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;

namespace LaunchpadRevamped.Options.Roles.Coven;

public class DarkFairyOptions : AbstractOptionGroup<DarkFairyRole>
{
    public override string GroupName => "launchpad.options.darkFairy";

    [ModdedNumberOption("launchpad.options.darkFairy.markedTasks", 1, 14)]
    public float MarkedTasksPerRound { get; set; } = 3;

    [ModdedEnumOption("launchpad.options.darkFairy.darkenedCountMode", typeof(DarkenedCounts))]
    public DarkenedCounts DarkenedCountMode { get; set; } = DarkenedCounts.DarkFairy;

    [ModdedToggleOption("launchpad.options.darkFairy.canDarkenNeutral")]
    public bool CanDarkenNeutral { get; set; } = false;

    /// <summary>How Darkened players are counted for the Coven's win condition (same three choices as TOHO).</summary>
    public enum DarkenedCounts
    {
        None,
        DarkFairy,
        Original,
    }
}
