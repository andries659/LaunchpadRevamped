using LaunchpadReloaded.Roles.Crewmate;
using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.Utilities;

namespace LaunchpadReloaded.Options.Roles.Crewmate;

public class MedicOptions : AbstractOptionGroup<MedicRole>
{
    public override string GroupName => "launchpad.options.medic";

    [ModdedToggleOption("launchpad.options.medic.onlyAllowRevivingInMedBayLaboratory")]
    public bool OnlyAllowInMedbay { get; set; } = false;

    [ModdedToggleOption("launchpad.options.medic.canDragBodies")]
    public bool DragBodies { get; set; } = false;

    [ModdedNumberOption("launchpad.options.medic.maxRevives", 1, 9)]
    public float MaxRevives { get; set; } = 2;

    [ModdedNumberOption("launchpad.options.medic.reviveCooldown", 1, 50, 2, MiraNumberSuffixes.Seconds)]
    public float ReviveCooldown { get; set; } = 20;
}