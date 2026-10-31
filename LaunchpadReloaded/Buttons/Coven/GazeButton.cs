using LaunchpadRevamped.Features;
using LaunchpadRevamped.Networking.Roles;
using LaunchpadRevamped.Options.Roles.Coven;
using LaunchpadRevamped.Roles.Coven;
using LaunchpadRevamped.Utilities;
using MiraAPI.GameOptions;
using MiraAPI.Utilities.Assets;
using UnityEngine;

namespace LaunchpadRevamped.Buttons.Coven;

public class GazeButton : BaseLaunchpadButton
{
    public override string Name => "launchpad.button.gaze";
    public override float Cooldown => OptionGroupSingleton<MedusaOptions>.Instance.GazeCooldown;
    public override float EffectDuration => 0;
    public override int MaxUses => 0;
    public override LoadableAsset<Sprite> Sprite => LaunchpadAssets.GazeButton;
    public override bool TimerAffectedByPlayer => true;
    public override bool AffectedByHack => true;

    public override bool Enabled(RoleBehaviour? role)
    {
        return role is MedusaRole;
    }

    protected override void OnClick()
    {
        var options = OptionGroupSingleton<MedusaOptions>.Instance;
        var local = PlayerControl.LocalPlayer;
        var origin = local.GetTruePosition();

        foreach (var player in PlayerControl.AllPlayerControls.ToArray())
        {
            if (!player || player.AmOwner || !player.Data || player.Data.IsDead || player.Data.Disconnected
                || player.inVent || CovenUtils.IsCoven(player))
            {
                continue;
            }

            var offset = player.GetTruePosition() - origin;
            if (offset.magnitude > options.GazeRange)
            {
                continue;
            }

            // Only players in line of sight are caught by the gaze.
            if (PhysicsHelpers.AnyNonTriggersBetween(origin, offset.normalized, offset.magnitude, Constants.ShipAndObjectsMask))
            {
                continue;
            }

            player.RpcFreeze(options.FreezeDuration);
        }
    }
}
