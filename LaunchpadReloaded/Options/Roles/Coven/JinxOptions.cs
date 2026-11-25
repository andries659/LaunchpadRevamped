using LaunchpadRevamped.Roles.Coven;
using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.Utilities;

namespace LaunchpadRevamped.Options.Roles.Coven;

public class JinxOptions : AbstractOptionGroup<JinxRole>
{
    public override string GroupName => "launchpad.options.jinx";

    [ModdedNumberOption("launchpad.options.jinx.jinxCooldown", 0, 120, 2.5f, MiraNumberSuffixes.Seconds)]
    public float JinxCooldown { get; set; } = 20;

    [ModdedNumberOption("launchpad.options.jinx.maxJinxes", 1, 100)]
    public float MaxJinxes { get; set; } = 10;

    [ModdedToggleOption("launchpad.options.jinx.covenCanDieToJinx")]
    public bool CovenCanDieToJinx { get; set; } = true;
}
