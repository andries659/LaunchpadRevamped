using System;
using LaunchpadRevamped.Features;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using UnityEngine;

namespace LaunchpadRevamped.Roles.Impostor;

/// <summary>
/// Impostor ghost role. A dead Impostor can still help the team by dragging dead bodies around (see DragButton).
/// Assigned on death, see PoltergeistPatches.
/// </summary>
public class PoltergeistRole(IntPtr ptr) : RoleBehaviour(ptr), ICustomRole
{
    public Color RoleColor => LaunchpadPalette.PoltergeistColor;
    public ModdedRoleTeams Team => ModdedRoleTeams.Impostor;

    public CustomRoleConfiguration Configuration => new(this)
    {
        HideSettings = false,
        RoleHintType = RoleHintType.TaskHint,
        TasksCountForProgress = false,
        CanUseVent = false,
        ShowInFreeplay = true,
        UseVanillaKillButton = false,
        Icon = LaunchpadAssets.PoltergeistIcon,
    };

    public override bool IsDead => true;
    public override bool IsAffectedByComms => false;

    public override void SpawnTaskHeader(PlayerControl playerControl)
    {
        playerControl.ClearTasks();
        PlayerTask.GetOrCreateTask<ImportantTextTask>(playerControl).Text =
            $"{RoleColor.ToTextColor()}You are a Poltergeist.\nDrag dead bodies around to help your fellow Impostors.";
    }

    public override void AppendTaskHint(Il2CppSystem.Text.StringBuilder taskStringBuilder)
    {
        // remove default task hint
    }

    // A dead Impostor still wins together with its team.
    public override bool DidWin(GameOverReason gameOverReason)
    {
        return GameManager.Instance.DidImpostorsWin(gameOverReason);
    }
}
