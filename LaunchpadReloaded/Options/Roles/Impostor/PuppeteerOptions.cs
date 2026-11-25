using LaunchpadRevamped.Roles.Impostor;
using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.Utilities;

namespace LaunchpadRevamped.Options.Roles.Impostor;

public class PuppeteerOptions : AbstractOptionGroup<PuppeteerRole>
{
    public override string GroupName => "launchpad.options.puppeteer";

    [ModdedNumberOption("launchpad.options.puppeteer.puppetCooldown", 0, 120, 2.5f, MiraNumberSuffixes.Seconds)]
    public float PuppetCooldown { get; set; } = 25;

    [ModdedToggleOption("launchpad.options.puppeteer.doubleKills")]
    public bool DoubleKills { get; set; } = false;
}
