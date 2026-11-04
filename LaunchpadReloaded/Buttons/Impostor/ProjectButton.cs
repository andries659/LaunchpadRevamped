using LaunchpadRevamped.Features;
using LaunchpadRevamped.Networking.Roles;
using LaunchpadRevamped.Options.Roles.Impostor;
using LaunchpadRevamped.Roles.Impostor;
using MiraAPI.GameOptions;
using MiraAPI.Translation;
using MiraAPI.Utilities.Assets;
using UnityEngine;

namespace LaunchpadRevamped.Buttons.Impostor;

/// <summary>
/// One button for the whole ability. Clicking starts the projection (the "effect"); clicking again, or the effect
/// timer running out, ends it. Everything that undoes the projection lives in OnEffectEnd.
/// </summary>
public class ProjectButton : BaseLaunchpadButton
{
    public override string Name => "launchpad.button.esperProject";
    public override float Cooldown => OptionGroupSingleton<EsperOptions>.Instance.ProjectCooldown;
    public override float EffectDuration => OptionGroupSingleton<EsperOptions>.Instance.MaxProjectionTime;

    // TODO: replace with dedicated Project/Return art.
    public override LoadableAsset<Sprite> Sprite => LaunchpadAssets.HideButton;
    public override bool TimerAffectedByPlayer => true;
    public override bool AffectedByHack => false;

    public override bool Enabled(RoleBehaviour? role) => role is EsperRole;

    // Lets the player click again during the effect to return early.
    public override bool IsEffectCancellable() => true;

    public override bool CanUse()
    {
        // Starting a projection from inside a vent would leave the body in a vent.
        return base.CanUse() && (EffectActive || !PlayerControl.LocalPlayer.inVent);
    }

    protected override void OnClick()
    {
        var local = PlayerControl.LocalPlayer;
        var pos = local.transform.position;

        local.RpcEsperProject(pos.x, pos.y);
        OverrideName("launchpad.button.esperReturn".Translate().ToUpperInvariant());
    }

    public override void OnEffectEnd()
    {
        if (HudManager.InstanceExists)
        {
            OverrideName(Name.Translate().ToUpperInvariant());
        }

        var local = PlayerControl.LocalPlayer;
        if (local && EsperProjection.IsProjected(local))
        {
            local.RpcEsperReturn();
        }
    }
}
