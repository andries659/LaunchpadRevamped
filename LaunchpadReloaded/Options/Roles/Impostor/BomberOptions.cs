using LaunchpadRevamped.Roles.Impostor;
using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.Utilities;

namespace LaunchpadRevamped.Options.Roles.Impostor;

public class BomberOptions : AbstractOptionGroup<BomberRole>
{
    public override string GroupName => "launchpad.options.bomber";

    [ModdedNumberOption("launchpad.options.bomber.bombCooldown", 0, 120, 5, MiraNumberSuffixes.Seconds)]
    public float BombCooldown { get; set; } = 30;

    [ModdedNumberOption("launchpad.options.bomber.bombFuse", 1, 15, 1, MiraNumberSuffixes.Seconds)]
    public float BombFuse { get; set; } = 5;

    [ModdedNumberOption("launchpad.options.bomber.bombRadius", 0.5f, 10f, 0.5f, suffixType: MiraNumberSuffixes.None)]
    public float BombRadius { get; set; } = 3f;

    [ModdedNumberOption("launchpad.options.bomber.bombUses", 0, 10, zeroInfinity: true)]
    public float BombUses { get; set; } = 3;

    [ModdedToggleOption("launchpad.options.bomber.bombKillsImpostors")]
    public bool BombKillsImpostors { get; set; } = false;
}
