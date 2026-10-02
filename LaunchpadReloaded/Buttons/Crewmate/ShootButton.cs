using Il2CppSystem;
using LaunchpadRevamped.Features;
using LaunchpadRevamped.Options.Roles.Crewmate;
using LaunchpadRevamped.Roles.Crewmate;
using MiraAPI.GameOptions;
using MiraAPI.Networking;
using MiraAPI.Utilities;
using MiraAPI.Utilities.Assets;
using UnityEngine;

namespace LaunchpadRevamped.Buttons.Crewmate;

public class ShootButton : BaseLaunchpadButton<PlayerControl>
{
    public override string Name => "launchpad.button.shoot";
    public override float Cooldown => OptionGroupSingleton<SheriffOptions>.Instance.ShotCooldown;
    public override float EffectDuration => 0;
    public override int MaxUses => (int)OptionGroupSingleton<SheriffOptions>.Instance.ShotsPerGame;
    public override LoadableAsset<Sprite> Sprite => LaunchpadAssets.ShootButton;
    public override bool TimerAffectedByPlayer => true;
    public override bool AffectedByHack => true;

    public override bool Enabled(RoleBehaviour? role) => role is SheriffRole;

    public override PlayerControl? GetTarget()
    {
        return PlayerControl.LocalPlayer.GetClosestPlayer(true, 1.1f);
    }

    public override void SetOutline(bool active)
    {
        Target?.cosmetics.SetOutline(active, new Nullable<Color>(LaunchpadPalette.SheriffColor));
    }

    protected override void OnClick()
    {
        if (Target == null)
        {
            return;
        }

        if (Target.Data.Role.TeamType == RoleTeamTypes.Impostor || OptionGroupSingleton<SheriffOptions>.Instance.ShouldCrewmateDie && Target.Data.Role.TeamType == RoleTeamTypes.Crewmate)
        {
            PlayerControl.LocalPlayer.RpcCustomMurder(Target);
        }

        if (Target.Data.Role.TeamType == RoleTeamTypes.Crewmate)
        {
            PlayerControl.LocalPlayer.RpcCustomMurder(PlayerControl.LocalPlayer);
        }

        ResetTarget();
    }
}