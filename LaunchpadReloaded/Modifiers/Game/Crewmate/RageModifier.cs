using System.Collections.Generic;
using System.Linq;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using LaunchpadRevamped.Buttons.Modifiers;
using LaunchpadRevamped.Features;
using LaunchpadRevamped.Options.Modifiers;
using LaunchpadRevamped.Options.Modifiers.Crewmate;
using LaunchpadRevamped.Roles.Crewmate;
using LaunchpadRevamped.Utilities;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.Events.Vanilla.Player;
using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Modifiers;
using MiraAPI.Translation;
using UnityEngine;

namespace LaunchpadRevamped.Modifiers.Game.Crewmate;

/// <summary>
/// You get extra tasks. Once you have finished ALL of your tasks, your kill button starts working
/// (with a cooldown) for the rest of the game.
/// </summary>
public sealed class RageModifier : LPModifier
{
    public override string ModifierName => MiraLocaleManager.Get("launchpad.modifier.rage");
    public override string GetDescription() => MiraLocaleManager.GetParsed(
        "launchpad.modifier.rage.TabDescription",
        new[]
        {
            ("{extra}", OptionGroupSingleton<RageOptions>.Instance.ExtraTasks.ToString("0")),
            ("{cooldown}", OptionGroupSingleton<RageOptions>.Instance.KillCooldown.ToString("0")),
        });

    public override int GetAssignmentChance() => (int)OptionGroupSingleton<CrewmateModifierOptions>.Instance.RageChance;
    public override int GetAmountPerGame() => 1;

    /// <summary>Gets a value indicating whether every task of the player is finished, which unlocks the kill button.</summary>
    public bool KillUnlocked
    {
        get
        {
            var tasks = Player.Data.Tasks.ToArray();
            return tasks.Length > 0 && tasks.All(t => t.Complete);
        }
    }

    public override bool IsModifierValidOn(RoleBehaviour role)
    {
        // Crewmate team only. Coven roles are Crewmate-based, and the Sheriff and Infiltrator already have their own kill.
        return base.IsModifierValidOn(role)
               && role.TeamType == RoleTeamTypes.Crewmate
               && !RoleTeamChecks.IsCoven(role)
               && !RoleTeamChecks.IsNeutral(role)
               && role is not SheriffRole
               && role is not InfiltratorRole;
    }

    // The modifier is usually added after the HUD was set up, so the button has to be shown by hand (same as Vendetta).
    public override void OnActivate()
    {
        if (!Player.AmOwner)
        {
            return;
        }

        CustomButtonSingleton<RageKillButton>.Instance.Button?.Show();
    }

    public override void OnDeactivate()
    {
        if (!Player.AmOwner)
        {
            return;
        }

        CustomButtonSingleton<RageKillButton>.Instance.Button?.Hide();
    }

    [RegisterEvent]
    public static void RoundStartEvent(RoundStartEvent @event)
    {
        // Tasks are assigned by now: the host gives every Rage player their extra tasks once, at the start of the game.
        if (!@event.TriggeredByIntro || !AmongUsClient.Instance.AmHost)
        {
            return;
        }

        foreach (var player in PlayerControl.AllPlayerControls.ToArray())
        {
            if (player.HasModifier<RageModifier>())
            {
                AddExtraTasks(player, (int)OptionGroupSingleton<RageOptions>.Instance.ExtraTasks);
            }
        }
    }

    [RegisterEvent]
    public static void TaskCompletedEvent(CompleteTaskEvent @event)
    {
        if (!@event.Player.AmOwner)
        {
            return;
        }

        var rage = @event.Player.GetModifier<RageModifier>();
        if (rage == null || !rage.KillUnlocked)
        {
            return;
        }

        CustomButtonSingleton<RageKillButton>.Instance.ResetCooldownAndOrEffect();

        MiraAPI.Utilities.Helpers.CreateAndShowNotification(
            MiraLocaleManager.Get("launchpad.modifier.rage.unlocked"),
            LaunchpadPalette.RageColor);
    }

    private static void AddExtraTasks(PlayerControl player, int extra)
    {
        var ship = ShipStatus.Instance;
        if (ship == null || player.Data == null || extra <= 0)
        {
            return;
        }

        var current = player.Data.Tasks.ToArray().Select(t => t.TypeId).ToList();
        if (current.Count == 0)
        {
            return;
        }

        // Short tasks first, long tasks only if there are not enough short ones. Tasks the player already has are skipped.
        var shortPool = ship.ShortTasks.ToArray().Select(t => (byte)t.Index).Where(i => !current.Contains(i)).OrderBy(_ => Random.value).ToList();
        var longPool = ship.LongTasks.ToArray().Select(t => (byte)t.Index).Where(i => !current.Contains(i)).OrderBy(_ => Random.value).ToList();
        List<byte> extraIds = shortPool.Concat(longPool).Take(extra).ToList();
        if (extraIds.Count == 0)
        {
            return;
        }

        player.Data.RpcSetTasks(new Il2CppStructArray<byte>(current.Concat(extraIds).ToArray()));
    }
}
