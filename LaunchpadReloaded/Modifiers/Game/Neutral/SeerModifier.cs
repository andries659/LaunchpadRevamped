using LaunchpadRevamped.Options.Modifiers;
using LaunchpadRevamped.Utilities;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Player;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using Reactor.Utilities;
using UnityEngine;

namespace LaunchpadRevamped.Modifiers.Game.Neutral;

/// <summary>
/// Ported from TOHO (Seer). Whenever another player is killed, your screen flashes red.
/// </summary>
public sealed class SeerModifier : ExtraModifier
{
    protected override string Key => "seer";
    protected override string DisplayName => "Seer";
    protected override string Description => "Whenever another player is killed, your screen flashes red.";

    public override int GetAssignmentChance() => (int)OptionGroupSingleton<ExtraNeutralModifierOptions>.Instance.SeerChance;

    public override bool IsModifierValidOn(RoleBehaviour role) => base.IsModifierValidOn(role) && RoleTeamChecks.IsNeutral(role);

    [RegisterEvent]
    public static void OnPlayerDeath(PlayerDeathEvent @event)
    {
        var local = PlayerControl.LocalPlayer;
        if (!local || !local.Data || local.Data.IsDead || @event.Player == local || @event.DeathReason != DeathReason.Kill
            || !local.HasModifier<SeerModifier>())
        {
            return;
        }

        Coroutines.Start(ModifierHelpers.CoFlash(new Color(1f, 0f, 0f, 0.45f), 0.6f));
    }
}
