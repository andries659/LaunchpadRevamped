// EXPERIMENTAL - Infection. Disabled until it has been tested.
// Written against MiraAPI 0.6.0 (AbstractGameMode). A game mode is registered just by existing, so it stays commented out.
using System.Collections;
using System.Linq;
using AmongUs.GameOptions;
using MiraAPI.GameModes;
using Reactor.Utilities;
using UnityEngine;

namespace LaunchpadRevamped.Gamemodes;

/// <summary>
/// One player starts infected (Impostor). Everyone they kill comes back as infected instead of staying dead.
/// Crewmates win by finishing their tasks or by voting out every infected player. The infected win when no Crewmates are left.
/// </summary>
public class Infection : AbstractGameMode
{
    public override string Name => "Infection";
    public override string Description => "Someone is infected.\n<b><i>Their victims rise again as infected.</b></i>";
    public override Color Color => new(0.45f, 0.8f, 0.25f);

    public override bool ShowNormalRoleSettings => false;

    public override void AssignRoles(out bool runOriginal, LogicRoleSelectionNormal instance)
    {
        // Standard assignment, but with exactly one Impostor (patient zero).
        runOriginal = true;
        if (AmongUsClient.Instance.AmHost && GameOptionsManager.Instance.CurrentGameOptions.TryCast<NormalGameOptionsV10>() is { } options)
        {
            options.SetInt(Int32OptionNames.NumImpostors, 1);
        }
    }

    public override void OnPlayerDeath(PlayerControl player, bool assignGhostRole)
    {
        // Murder victims are turned instead of becoming ghosts. Only the host decides.
        if (!AmongUsClient.Instance.AmHost || player.Data.Role.IsImpostor)
        {
            base.OnPlayerDeath(player, assignGhostRole);
            return;
        }

        Coroutines.Start(Infect(player));
    }

    private static IEnumerator Infect(PlayerControl victim)
    {
        yield return new WaitForSeconds(1f);

        if (!victim || victim.Data.Disconnected || AmongUsClient.Instance.GameState != InnerNet.InnerNetClient.GameStates.Started)
        {
            yield break;
        }

        victim.Revive();
        victim.RpcSetRole(RoleTypes.Impostor);
    }

    public override void CheckGameEnd(out bool runOriginal, LogicGameFlowNormal instance)
    {
        runOriginal = false;

        var alive = GameData.Instance.AllPlayers.ToArray().Where(p => !p.Disconnected && !p.IsDead).ToList();
        var infected = alive.Count(p => p.Role.IsImpostor);
        var crew = alive.Count - infected;

        if (crew <= 0)
        {
            instance.Manager.RpcEndGame(GameOverReason.ImpostorsByKill, false);
        }
        else if (infected <= 0)
        {
            instance.Manager.RpcEndGame(GameOverReason.CrewmatesByVote, false);
        }
        else if (GameData.Instance.TotalTasks > 0 && GameData.Instance.CompletedTasks >= GameData.Instance.TotalTasks)
        {
            instance.Manager.RpcEndGame(GameOverReason.CrewmatesByTask, false);
        }
    }
}
