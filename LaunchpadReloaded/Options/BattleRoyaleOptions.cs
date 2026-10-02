using System;
using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.GameOptions.OptionTypes;

namespace LaunchpadReloaded.Options;

public class BattleRoyaleOptions : AbstractOptionGroup
{
    public override string GroupName => "launchpad.options.battleRoyaleOptions";

    public override Func<bool> GroupVisible => () => false; //CustomGameModeManager.ActiveMode?.GetType() == typeof(BattleRoyale);

    [ModdedToggleOption("launchpad.options.battleRoyaleOptions.useSeekerCharacter")] public bool SeekerCharacter { get; set; } = true;
    
    public ModdedToggleOption ShowKnife { get; } = new("launchpad.options.battleRoyaleOptions.showKnife", true)
    {
        Visible = () => OptionGroupSingleton<BattleRoyaleOptions>.Instance.SeekerCharacter
    };
}