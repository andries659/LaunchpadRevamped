using LaunchpadRevamped.Features;
using LaunchpadRevamped.Networking.Roles;
using LaunchpadRevamped.Options.Roles.Impostor;
using LaunchpadRevamped.Roles.Impostor;
using MiraAPI.GameOptions;
using MiraAPI.Utilities.Assets;
using UnityEngine;

namespace LaunchpadRevamped.Buttons.Impostor;

public class PlaceBombButton : BaseLaunchpadButton
{
    public override string Name => "launchpad.button.placeBomb";
    public override float Cooldown => OptionGroupSingleton<BomberOptions>.Instance.BombCooldown;
    public override float EffectDuration => 0;
    public override int MaxUses => (int)OptionGroupSingleton<BomberOptions>.Instance.BombUses;
    public override LoadableAsset<Sprite> Sprite => LaunchpadAssets.BombButton;
    public override bool TimerAffectedByPlayer => true;
    public override bool AffectedByHack => true;

    public override bool Enabled(RoleBehaviour? role)
    {
        return role is BomberRole;
    }

    protected override void OnClick()
    {
        var position = PlayerControl.LocalPlayer.GetTruePosition();
        PlayerControl.LocalPlayer.RpcPlaceBomb(position.x, position.y);
    }
}
