using System.Collections;
using System.Collections.Generic;
using LaunchpadRevamped.Options.Modifiers;
using LaunchpadRevamped.Utilities;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Networking;
using Reactor.Utilities;
using UnityEngine;

namespace LaunchpadRevamped.Modifiers.Game.Impostor;

/// <summary>
/// Ported from TOHO (Lag). The kill does not happen right away: the host holds it back for a moment and then does it,
/// as long as both players are still alive and no meeting started in between.
/// </summary>
public sealed class LagModifier : ExtraModifier
{
    protected override string Key => "lag";
    protected override string DisplayName => "Lag";

    protected override string Description =>
        $"Your kills happen {OptionGroupSingleton<ExtraImpostorModifierOptions>.Instance.LagDelay:0.#}s late.";

    public override int GetAssignmentChance() => (int)OptionGroupSingleton<ExtraImpostorModifierOptions>.Instance.LagChance;

    public override bool IsModifierValidOn(RoleBehaviour role) => base.IsModifierValidOn(role) && RoleTeamChecks.IsImpostorTeam(role);

    // Attackers whose kill is being held back, and attackers whose delayed kill is going through right now.
    private static readonly HashSet<byte> Pending = [];
    private static readonly HashSet<byte> Releasing = [];

    [RegisterEvent(20)]
    public static void OnBeforeMurder(BeforeMurderEvent @event)
    {
        if (!AmongUsClient.Instance.AmHost || @event.IsCancelled || @event.IsIndirectAttack)
        {
            return;
        }

        var source = @event.Source;
        var target = @event.Target;
        if (!source || !target || !source.HasModifier<LagModifier>())
        {
            return;
        }

        if (Releasing.Contains(source.PlayerId))
        {
            return;
        }

        // The kill is held back. Pressing the button again while it is waiting does nothing.
        @event.Cancel();
        if (Pending.Add(source.PlayerId))
        {
            Coroutines.Start(CoDelayedKill(source, target, OptionGroupSingleton<ExtraImpostorModifierOptions>.Instance.LagDelay));
        }
    }

    private static IEnumerator CoDelayedKill(PlayerControl source, PlayerControl target, float delay)
    {
        var id = source.PlayerId;
        yield return new WaitForSeconds(delay);

        Pending.Remove(id);
        if (!source || !target || !source.Data || !target.Data || source.Data.IsDead || target.Data.IsDead
            || target.Data.Disconnected || MeetingHud.Instance || !ShipStatus.Instance)
        {
            yield break;
        }

        Releasing.Add(id);
        source.RpcCustomMurder(target);
        Releasing.Remove(id);
    }
}
