using Il2CppSystem;
using LaunchpadRevamped.Features;
using LaunchpadRevamped.Options.Roles.Crewmate;
using LaunchpadRevamped.Roles.Crewmate;
using LaunchpadRevamped.Utilities;
using MiraAPI.GameOptions;
using MiraAPI.Utilities;
using MiraAPI.Utilities.Assets;
using UnityEngine;

namespace LaunchpadRevamped.Buttons.Crewmate;

public class VisitButton : BaseLaunchpadButton<PlayerControl>
{
    public override string Name => "launchpad.button.visit";
    public override float Cooldown => OptionGroupSingleton<VisitorOptions>.Instance.VisitCooldown;
    public override int MaxUses => (int)OptionGroupSingleton<VisitorOptions>.Instance.VisitUses;
    public override LoadableAsset<Sprite> Sprite => LaunchpadAssets.InvestigateButton;
    public override bool TimerAffectedByPlayer => true;
    public override bool AffectedByHack => true;

    public override bool Enabled(RoleBehaviour? role) => role is VisitorRole;

    public override PlayerControl? GetTarget()
    {
        return PlayerControl.LocalPlayer.GetClosestPlayer(true, 1.1f);
    }

    public override bool CanUse()
    {
        return base.CanUse() && Target != null && !VisitorUtilities.IsVisiting(Target);
    }

    public override void SetOutline(bool active)
    {
        Target?.cosmetics.SetOutline(active, new Nullable<Color>(LaunchpadPalette.VisitorColor));
    }

    protected override void FixedUpdate(PlayerControl playerControl)
    {
        base.FixedUpdate(playerControl);
        VisitorUtilities.Tick();
    }

    protected override void OnClick()
    {
        if (Target == null)
        {
            return;
        }

        VisitorUtilities.StartVisit(Target);
        ResetTarget();
    }
}
