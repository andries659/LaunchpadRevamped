using System.Collections;
using LaunchpadRevamped.Features;
using LaunchpadRevamped.Modifiers.Game.Universal;
using LaunchpadRevamped.Options.Roles.Impostor;
using LaunchpadRevamped.Roles.Impostor;
using LaunchpadRevamped.Utilities;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Networking;
using MiraAPI.Utilities.Assets;
using Reactor.Utilities;
using UnityEngine;

namespace LaunchpadRevamped.Buttons.Impostor;

public class BiteButton : BaseLaunchpadButton<PlayerControl>
{
    public override string Name => "launchpad.button.bite";
    public override float Cooldown => OptionGroupSingleton<VampireOptions>.Instance.BiteCooldown;
    public override float EffectDuration => 0;
    public override int MaxUses => 0;
    public override LoadableAsset<Sprite> Sprite => LaunchpadAssets.BiteButton;
    public override bool TimerAffectedByPlayer => true;
    public override bool AffectedByHack => true;

    public override bool Enabled(RoleBehaviour? role)
    {
        return role is VampireRole;
    }

    public override PlayerControl? GetTarget()
    {
        return AbilityUtils.ClosestNonImpostor(Distance, player => !VampireRole.Bitten.Contains(player.PlayerId));
    }

    public override bool IsTargetValid(PlayerControl? target)
    {
        return target && !target!.Data.IsDead && !VampireRole.Bitten.Contains(target.PlayerId);
    }

    public override void SetOutline(bool active)
    {
        Target?.cosmetics.SetOutline(active, new Il2CppSystem.Nullable<Color>(LaunchpadPalette.VampireColor));
    }

    protected override void OnClick()
    {
        if (Target == null)
        {
            return;
        }

        var vampire = PlayerControl.LocalPlayer;

        // Same as TOHO: biting a Bait is an ordinary kill, so the Bait still gets to expose the Vampire.
        if (Target.HasModifier<BaitModifier>())
        {
            vampire.RpcCustomMurder(Target, teleportMurderer: false);
            ResetTarget();
            return;
        }

        VampireRole.Bitten.Add(Target.PlayerId);
        Coroutines.Start(CoBite(vampire, Target, OptionGroupSingleton<VampireOptions>.Instance.BiteDelay));
        ResetTarget();
    }

    // The timer runs on the Vampire's client and pauses during meetings. If the Vampire dies first, the bite fades.
    private static IEnumerator CoBite(PlayerControl vampire, PlayerControl victim, float delay)
    {
        var id = victim.PlayerId;
        var elapsed = 0f;

        while (elapsed < delay)
        {
            if (!vampire || !victim || vampire.Data.IsDead || victim.Data.IsDead || !VampireRole.Bitten.Contains(id))
            {
                VampireRole.Bitten.Remove(id);
                yield break;
            }

            if (!MeetingHud.Instance)
            {
                elapsed += Time.deltaTime;
            }

            yield return null;
        }

        VampireRole.Bitten.Remove(id);

        if (vampire && victim && !vampire.Data.IsDead && !victim.Data.IsDead)
        {
            vampire.RpcCustomMurder(victim, teleportMurderer: false);
        }
    }
}
