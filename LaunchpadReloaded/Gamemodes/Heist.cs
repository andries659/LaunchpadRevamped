// EXPERIMENTAL - Heist. Disabled until it has been tested. This is the roughest of the three: loot is a plain counter.
// Written against MiraAPI 0.6.0 (AbstractGameMode). A game mode is registered just by existing, so it stays commented out.
using System.Collections.Generic;
using System.Linq;
using AmongUs.GameOptions;
using MiraAPI.GameModes;
using TMPro;
using UnityEngine;

namespace LaunchpadRevamped.Gamemodes;

/// <summary>
/// Impostors are Thieves who must steal enough loot before the timer runs out. Crewmates are Guards who stop them by
/// voting Thieves out or by simply holding out until the timer ends.
/// </summary>
public class Heist : AbstractGameMode
{
    private const float HeistDuration = 480f; // seconds
    private const int LootGoal = 10;

    public override string Name => "Heist";
    public override string Description => "Thieves are robbing the place.\n<b><i>Steal everything before time runs out, or get caught.</b></i>";
    public override Color Color => new(0.95f, 0.72f, 0.15f);

    public override bool ShowNormalRoleSettings => false;

    public static int Loot { get; set; }
    private float _timeLeft;
    private TextMeshPro? _status;

    public override void Initialize()
    {
        Loot = 0;
        _timeLeft = HeistDuration;
    }

    public override void HudStart(HudManager instance)
    {
        _status = MiraAPI.Utilities.Helpers.CreateTextLabel("Heist_Status", instance.transform, AspectPosition.EdgeAlignments.Top, new Vector3(0, 0.25f, 0));
    }

    public override void HudUpdate(HudManager instance)
    {
        if (ShipStatus.Instance && !MeetingHud.Instance)
        {
            _timeLeft -= Time.deltaTime;
        }

        if (_status)
        {
            _status!.text = $"<size=75%>{Color.ToTextColor()}Heist</size></color>\nLoot {Loot}/{LootGoal}  -  {Mathf.CeilToInt(Mathf.Max(_timeLeft, 0f))}s";
        }

        instance.SabotageButton.gameObject.SetActive(false);
    }

    // TODO: Thieves should add to Loot when they finish a task. Needs a small RPC so every client sees the same counter
    // (host adds to Loot and syncs it). Until then Loot only changes on the host's own client.

    public override void CheckGameEnd(out bool runOriginal, LogicGameFlowNormal instance)
    {
        runOriginal = false;

        var thieves = GameData.Instance.AllPlayers.ToArray().Count(p => !p.Disconnected && !p.IsDead && p.Role.IsImpostor);

        if (Loot >= LootGoal)
        {
            instance.Manager.RpcEndGame(GameOverReason.ImpostorsByKill, false);
        }
        else if (thieves <= 0)
        {
            instance.Manager.RpcEndGame(GameOverReason.CrewmatesByVote, false);
        }
        else if (_timeLeft <= 0f)
        {
            instance.Manager.RpcEndGame(GameOverReason.CrewmatesByTask, false);
        }
    }
}
