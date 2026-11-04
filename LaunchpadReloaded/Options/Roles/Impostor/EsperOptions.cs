using LaunchpadRevamped.Roles.Impostor;
using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.Utilities;

namespace LaunchpadRevamped.Options.Roles.Impostor;

public class EsperOptions : AbstractOptionGroup<EsperRole>
{
    public override string GroupName => "launchpad.options.esper";

    [ModdedNumberOption("launchpad.options.esper.projectCooldown", 5, 120, 5, MiraNumberSuffixes.Seconds)]
    public float ProjectCooldown { get; set; } = 25;

    // The projection ends on its own after this long (the Esper is returned to its body, not killed).
    [ModdedNumberOption("launchpad.options.esper.maxProjectionTime", 5, 90, 5, MiraNumberSuffixes.Seconds)]
    public float MaxProjectionTime { get; set; } = 30;

    // Multiplier applied to the Esper's normal vision while projected.
    [ModdedNumberOption("launchpad.options.esper.projectionVision", 0.25f, 1.5f, 0.25f)]
    public float ProjectionVision { get; set; } = 0.75f;
}
