using System.Linq;
using LaunchpadRevamped.Options.Modifiers;
using LaunchpadRevamped.Utilities;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using UnityEngine;

namespace LaunchpadRevamped.Modifiers.Game.Impostor;

/// <summary>
/// Ported from TOHO (Last Impostor). Given automatically by the host to the last living Impostor when the game started
/// with more than one. It lowers the kill cooldown (see ExtraModifierPatches). It is never handed out by the normal
/// modifier assignment.
/// </summary>
public sealed class LastImpostorModifier : ExtraModifier
{
    protected override string Key => "lastImpostor";
    protected override string DisplayName => "Last Impostor";

    protected override string Description =>
        $"You are the last Impostor. Your kill cooldown is {OptionGroupSingleton<ExtraImpostorModifierOptions>.Instance.LastImpostorReduction:0}% shorter.";

    public override int GetAssignmentChance() => 0;

    public override bool IsModifierValidOn(RoleBehaviour role) => false;

    private static int _startingImpostors;

    public override void OnActivate()
    {
        if (Player && Player.AmOwner)
        {
            var reduction = OptionGroupSingleton<ExtraImpostorModifierOptions>.Instance.LastImpostorReduction;
            Player.SetKillTimer(Player.killTimer * (1f - reduction / 100f));
        }
    }

    [RegisterEvent]
    public static void OnRoundStart(RoundStartEvent @event)
    {
        if (@event.TriggeredByIntro)
        {
            _startingImpostors = ModifierHelpers.AliveImpostorTeam().Length;
            return;
        }

        CheckForLastImpostor();
    }

    [RegisterEvent]
    public static void OnAfterMurder(AfterMurderEvent @event)
    {
        CheckForLastImpostor();
    }

    private static void CheckForLastImpostor()
    {
        if (!AmongUsClient.Instance.AmHost || !OptionGroupSingleton<ExtraImpostorModifierOptions>.Instance.LastImpostorEnabled
            || _startingImpostors < 2)
        {
            return;
        }

        var alive = ModifierHelpers.AliveImpostorTeam();
        if (alive.Length != 1 || alive.Any(player => player.HasModifier<LastImpostorModifier>()))
        {
            return;
        }

        alive[0].RpcAddModifier<LastImpostorModifier>();
    }
}
