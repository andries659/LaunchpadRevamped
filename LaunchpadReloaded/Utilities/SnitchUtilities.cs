using System.Collections.Generic;
using LaunchpadRevamped.Components;
using LaunchpadRevamped.Features;
using LaunchpadRevamped.Options.Roles.Crewmate;
using LaunchpadRevamped.Roles.Crewmate;
using MiraAPI.GameOptions;
using UnityEngine;

namespace LaunchpadRevamped.Utilities;

/// <summary>
/// Client-side logic for the Snitch. Impostors get an arrow (and a tag) pointing at the Snitch once it is close
/// to finishing its tasks, and a Snitch that finished all tasks sees who the Impostors are.
/// </summary>
public static class SnitchUtilities
{
    private const string SnitchTagName = "SnitchTag";
    private const string ImpostorTagName = "SnitchImpostorTag";

    private static readonly Dictionary<byte, ArrowBehaviour> Arrows = [];

    /// <summary>
    /// Number of unfinished tasks, or -1 if the player has no tasks at all.
    /// </summary>
    public static int GetTasksLeft(PlayerControl player)
    {
        var tasks = player.Data.Tasks;
        if (tasks == null || tasks.Count == 0)
        {
            return -1;
        }

        var left = 0;
        foreach (var task in tasks)
        {
            if (!task.Complete)
            {
                left++;
            }
        }

        return left;
    }

    /// <summary>
    /// A Snitch is exposed to Impostors once it has few enough tasks left (including none).
    /// </summary>
    public static bool IsExposed(PlayerControl snitch)
    {
        var left = GetTasksLeft(snitch);
        return left >= 0 && left <= (int)OptionGroupSingleton<SnitchOptions>.Instance.TasksLeftToExpose;
    }

    public static void Update()
    {
        var local = PlayerControl.LocalPlayer;
        if (!local || !local.Data || !ShipStatus.Instance || MeetingHud.Instance)
        {
            HideArrows();
            return;
        }

        var localIsSnitch = local.Data.Role is SnitchRole;
        var localIsImpostor = local.Data.Role.IsImpostor;
        if (!localIsSnitch && !localIsImpostor)
        {
            HideArrows();
            return;
        }

        foreach (var player in PlayerControl.AllPlayerControls.ToArray())
        {
            if (!player || !player.Data || player.Data.Disconnected)
            {
                continue;
            }

            if (localIsSnitch && player.Data.Role.IsImpostor)
            {
                EnsureTag(player, ImpostorTagName, "Impostor", Palette.ImpostorRed, IsImpostorRevealedToLocal);
            }

            if (localIsImpostor && player.Data.Role is SnitchRole)
            {
                EnsureTag(player, SnitchTagName, "Snitch", LaunchpadPalette.SnitchColor, IsSnitchExposedToLocal);
                UpdateArrow(player);
            }
        }
    }

    public static void HideArrows()
    {
        foreach (var arrow in Arrows.Values)
        {
            if (arrow)
            {
                arrow.gameObject.SetActive(false);
            }
        }
    }

    private static void EnsureTag(PlayerControl player, string name, string text, Color color, System.Func<PlayerControl, bool> isVisible)
    {
        var tags = player.GetTagManager();
        if (!tags || tags!.GetTagByName(name).HasValue)
        {
            return;
        }

        tags.AddTag(new PlayerTag(name, text, color) { IsLocallyVisible = isVisible });
    }

    private static void UpdateArrow(PlayerControl snitch)
    {
        Arrows.TryGetValue(snitch.PlayerId, out var arrow);

        if (!IsSnitchExposedToLocal(snitch))
        {
            if (arrow)
            {
                arrow!.gameObject.SetActive(false);
            }

            return;
        }

        if (!arrow)
        {
            arrow = MiraAPI.Utilities.Helpers.CreateArrow(snitch.transform, LaunchpadPalette.SnitchColor);
            Arrows[snitch.PlayerId] = arrow;
        }

        arrow!.gameObject.SetActive(true);
        arrow.target = snitch.transform.position;
    }

    private static bool IsSnitchExposedToLocal(PlayerControl snitch)
    {
        var local = PlayerControl.LocalPlayer;
        if (!local || !local.Data || local.Data.IsDead || !local.Data.Role.IsImpostor)
        {
            return false;
        }

        return snitch.Data.Role is SnitchRole && !snitch.Data.IsDead && IsExposed(snitch);
    }

    private static bool IsImpostorRevealedToLocal(PlayerControl impostor)
    {
        var local = PlayerControl.LocalPlayer;
        if (!local || !local.Data || local.Data.IsDead || local.Data.Role is not SnitchRole)
        {
            return false;
        }

        if (!OptionGroupSingleton<SnitchOptions>.Instance.RevealImpostors)
        {
            return false;
        }

        return impostor.Data.Role.IsImpostor && !impostor.Data.IsDead && GetTasksLeft(local) == 0;
    }
}
