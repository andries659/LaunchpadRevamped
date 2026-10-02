using LaunchpadRevamped.Features;
using LaunchpadRevamped.Networking.Roles;
using LaunchpadRevamped.Options.Roles.Impostor;
using LaunchpadRevamped.Roles.Impostor;
using LaunchpadRevamped.Utilities;
using MiraAPI.GameOptions;
using MiraAPI.Utilities.Assets;
using System.Linq;
using UnityEngine;

namespace LaunchpadRevamped.Buttons.Impostor;

public class DigButton : BaseLaunchpadButton
{
    public override string Name => "launchpad.button.dig";
    public override float Cooldown => OptionGroupSingleton<BurrowerOptions>.Instance.VentDigCooldown;
    public override float EffectDuration => 0;
    public override int MaxUses => (int)OptionGroupSingleton<BurrowerOptions>.Instance.VentDigUses;
    public override LoadableAsset<Sprite> Sprite => LaunchpadAssets.DigVentButton;
    public override bool TimerAffectedByPlayer => true;
    public override bool AffectedByHack => false;

    private float _ventDist = OptionGroupSingleton<BurrowerOptions>.Instance.VentDist;

    public override bool Enabled(RoleBehaviour? role)
    {
        return role is BurrowerRole;
    }

    public override bool CanUse()
    {
        return base.CanUse() && !ShipStatus.Instance.AllVents.Where(vent => !vent.IsSealed()).Any(vent =>
            Vector2.Distance(PlayerControl.LocalPlayer.GetTruePosition(), vent.transform.position) < _ventDist);
    }


    protected override void OnClick()
    {
        SoundManager.Instance.PlaySound(LaunchpadAssets.DigSound.LoadAsset(), false, volume: 2);
        PlayerControl.LocalPlayer.RpcDigVent();
    }
}