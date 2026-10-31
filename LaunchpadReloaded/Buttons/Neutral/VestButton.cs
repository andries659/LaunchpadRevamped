using LaunchpadRevamped.Features;
using LaunchpadRevamped.Modifiers;
using LaunchpadRevamped.Options.Roles.Neutral;
using LaunchpadRevamped.Roles.Neutral;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Networking;
using MiraAPI.Utilities.Assets;
using UnityEngine;

namespace LaunchpadRevamped.Buttons.Neutral;

public class VestButton : BaseLaunchpadButton
{
    public override string Name => "launchpad.button.vest";
    public override float Cooldown => OptionGroupSingleton<SurvivorOptions>.Instance.VestCooldown;
    public override float EffectDuration => OptionGroupSingleton<SurvivorOptions>.Instance.VestDuration;
    public override int MaxUses => (int)OptionGroupSingleton<SurvivorOptions>.Instance.MaxVests;
    public override LoadableAsset<Sprite> Sprite => LaunchpadAssets.VestButton;
    public override bool TimerAffectedByPlayer => true;
    public override bool AffectedByHack => true;

    public override bool Enabled(RoleBehaviour? role)
    {
        return role is SurvivorRole;
    }

    protected override void OnClick()
    {
        PlayerControl.LocalPlayer.RpcAddModifier<VestedModifier>();
    }
}
