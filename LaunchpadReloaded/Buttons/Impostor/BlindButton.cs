using LaunchpadRevamped.Features;
using LaunchpadRevamped.Networking.Roles;
using LaunchpadRevamped.Options.Roles.Impostor;
using LaunchpadRevamped.Roles.Impostor;
using MiraAPI.GameOptions;
using MiraAPI.Utilities.Assets;
using UnityEngine;

namespace LaunchpadRevamped.Buttons.Impostor;

public class BlindButton : BaseLaunchpadButton
{
    public override string Name => "launchpad.button.blind";
    public override float Cooldown => OptionGroupSingleton<GrenadierOptions>.Instance.BlindCooldown;
    public override float EffectDuration => 0;
    public override int MaxUses => (int)OptionGroupSingleton<GrenadierOptions>.Instance.BlindUses;
    public override LoadableAsset<Sprite> Sprite => LaunchpadAssets.BlindButton;
    public override bool TimerAffectedByPlayer => true;
    public override bool AffectedByHack => true;

    public override bool Enabled(RoleBehaviour? role)
    {
        return role is GrenadierRole;
    }

    protected override void OnClick()
    {
        PlayerControl.LocalPlayer.RpcBlind();
    }
}
