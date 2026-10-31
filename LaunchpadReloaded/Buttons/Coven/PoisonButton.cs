using System.Collections;
using LaunchpadRevamped.Features;
using LaunchpadRevamped.Options.Roles.Coven;
using LaunchpadRevamped.Roles.Coven;
using LaunchpadRevamped.Utilities;
using MiraAPI.GameOptions;
using MiraAPI.Networking;
using MiraAPI.Utilities.Assets;
using Reactor.Utilities;
using UnityEngine;

namespace LaunchpadRevamped.Buttons.Coven;

public class PoisonButton : BaseLaunchpadButton<PlayerControl>
{
    public override string Name => "launchpad.button.poison";
    public override float Cooldown => OptionGroupSingleton<PoisonerOptions>.Instance.PoisonCooldown;
    public override float EffectDuration => 0;
    public override int MaxUses => 0;
    public override LoadableAsset<Sprite> Sprite => LaunchpadAssets.PoisonButton;
    public override bool TimerAffectedByPlayer => true;
    public override bool AffectedByHack => true;

    public override bool Enabled(RoleBehaviour? role)
    {
        return role is PoisonerRole;
    }

    public override PlayerControl? GetTarget()
    {
        return CovenUtils.ClosestTarget(Distance);
    }

    public override void SetOutline(bool active)
    {
        Target?.cosmetics.SetOutline(active, new Il2CppSystem.Nullable<Color>(LaunchpadPalette.PoisonerColor));
    }

    protected override void OnClick()
    {
        if (Target == null)
        {
            return;
        }

        Coroutines.Start(CoPoison(PlayerControl.LocalPlayer, Target, OptionGroupSingleton<PoisonerOptions>.Instance.PoisonDelay));
        ResetTarget();
    }

    // The timer runs on the Poisoner's client and pauses during meetings. If the Poisoner dies first, the poison fades.
    private static IEnumerator CoPoison(PlayerControl poisoner, PlayerControl victim, float delay)
    {
        var elapsed = 0f;
        while (elapsed < delay)
        {
            if (!poisoner || !victim || poisoner.Data.IsDead || victim.Data.IsDead)
            {
                yield break;
            }

            if (!MeetingHud.Instance)
            {
                elapsed += Time.deltaTime;
            }

            yield return null;
        }

        if (poisoner && victim && !poisoner.Data.IsDead && !victim.Data.IsDead)
        {
            poisoner.RpcCustomMurder(victim, teleportMurderer: false);
        }
    }
}
