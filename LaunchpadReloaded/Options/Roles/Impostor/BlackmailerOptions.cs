using LaunchpadRevamped.Roles.Impostor;
using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.Utilities;

namespace LaunchpadRevamped.Options.Roles.Impostor;

public class BlackmailerOptions : AbstractOptionGroup<BlackmailerRole>
{
    public override string GroupName => "launchpad.options.blackmailer";

    [ModdedNumberOption("launchpad.options.blackmailer.blackmailCooldown", 2.5f, 120, 2.5f, MiraNumberSuffixes.Seconds)]
    public float BlackmailCooldown { get; set; } = 20;
}
