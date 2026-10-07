using LaunchpadRevamped.Features;
using LaunchpadRevamped.Networking.Roles;
using LaunchpadRevamped.Options.Roles.Crewmate;
using LaunchpadRevamped.Roles.Crewmate;
using LaunchpadRevamped.Utilities;
using MiraAPI.GameOptions;
using MiraAPI.Utilities.Assets;
using UnityEngine;

namespace LaunchpadRevamped.Buttons.Crewmate;

public class RewindButton : BaseLaunchpadButton
{
    public override string Name => "launchpad.button.rewind";
    public override float Cooldown => OptionGroupSingleton<TimeRewinderOptions>.Instance.RewindCooldown;
    public override float EffectDuration => 0;
    public override int MaxUses => (int)OptionGroupSingleton<TimeRewinderOptions>.Instance.RewindUses;
    public override LoadableAsset<Sprite> Sprite => LaunchpadAssets.RewindButton;
    public override bool TimerAffectedByPlayer => true;
    public override bool AffectedByHack => true;

    public override bool Enabled(RoleBehaviour? role)
    {
        return role is TimeRewinderRole;
    }

    public override bool CanUse()
    {
        return base.CanUse() && RewindHistory.HasHistory;
    }

    protected override void OnClick()
    {
        PlayerControl.LocalPlayer.RpcRewind();
    }
}
