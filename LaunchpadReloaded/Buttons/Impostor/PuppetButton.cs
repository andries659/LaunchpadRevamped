using LaunchpadRevamped.Features;
using MiraAPI.Modifiers;
using LaunchpadRevamped.Options.Roles.Impostor;
using LaunchpadRevamped.Roles.Impostor;
using LaunchpadRevamped.Utilities;
using MiraAPI.GameOptions;
using MiraAPI.Utilities.Assets;
using UnityEngine;

namespace LaunchpadRevamped.Buttons.Impostor;

public class PuppetButton : BaseLaunchpadButton<PlayerControl>
{
    public override string Name => "launchpad.button.puppet";
    public override float Cooldown => OptionGroupSingleton<PuppeteerOptions>.Instance.PuppetCooldown;
    public override float EffectDuration => 0;
    public override int MaxUses => 0;
    public override LoadableAsset<Sprite> Sprite => LaunchpadAssets.PuppetButton;
    public override bool TimerAffectedByPlayer => true;
    public override bool AffectedByHack => true;

    public override bool Enabled(RoleBehaviour? role)
    {
        return role is PuppeteerRole;
    }

    public override PlayerControl? GetTarget()
    {
        return AbilityUtils.ClosestNonImpostor(Distance, player => !PuppeteerRole.Puppets.Contains(player.PlayerId));
    }

    public override bool IsTargetValid(PlayerControl? target)
    {
        return target && !target!.Data.IsDead && !PuppeteerRole.Puppets.Contains(target.PlayerId);
    }

    public override void SetOutline(bool active)
    {
        Target?.cosmetics.SetOutline(active, new Il2CppSystem.Nullable<Color>(LaunchpadPalette.PuppeteerColor));
    }

    protected override void OnClick()
    {
        if (Target == null)
        {
            return;
        }

        PuppeteerRole.Puppets.Add(Target.PlayerId);
        ResetTarget();
    }
}
