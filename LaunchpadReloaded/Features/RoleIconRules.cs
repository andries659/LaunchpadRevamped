using LaunchpadRevamped.Roles.Coven;
using MiraAPI.Roles;
using UnityEngine;

namespace LaunchpadRevamped.Features;

/// <summary>
/// Decides who sees whose role icon next to the player names.
/// </summary>
public static class RoleIconRules
{
    /// <summary>
    /// The role to show an icon for. A dead player's role is replaced by a ghost role, so the role they had alive is used.
    /// </summary>
    private static RoleBehaviour? DisplayRole(PlayerControl target)
    {
        var data = target.Data;
        if (data == null)
        {
            return null;
        }

        if (data.IsDead && RoleManager.Instance && data.RoleWhenAlive != null && data.RoleWhenAlive.HasValue)
        {
            var alive = RoleManager.Instance.GetRole(data.RoleWhenAlive.Value);
            if (alive)
            {
                return alive;
            }
        }

        return data.Role;
    }

    /// <summary>
    /// The role's own icon. Modifiers are never part of this: only the role is shown.
    /// </summary>
    private static Sprite? IconOf(RoleBehaviour role)
    {
        if (role is ICustomRole custom)
        {
            return custom.Configuration.Icon?.LoadAsset();
        }

        return role.RoleIconSolid;
    }

    /// <summary>
    /// True when the viewer may see the target's role icon. Returns that icon.
    /// </summary>
    public static bool TryGetIcon(PlayerControl viewer, PlayerControl target, out Sprite icon)
    {
        icon = null!;

        if (!viewer || !target || viewer.Data == null || target.Data == null)
        {
            return false;
        }

        // A ghost's icon is hidden from the living.
        if (target.Data.IsDead && !viewer.Data.IsDead)
        {
            return false;
        }

        var targetRole = DisplayRole(target);
        var viewerRole = DisplayRole(viewer);
        if (!targetRole || !viewerRole)
        {
            return false;
        }

        var allowed =
            target == viewer                                                   // your own icon
            || (viewerRole!.IsImpostor && targetRole!.IsImpostor)              // impostors see each other
            || (viewerRole is ICovenRole && targetRole is ICovenRole);         // the Coven sees each other

        // Crewmates and neutrals never see anybody else's icon.
        if (!allowed)
        {
            return false;
        }

        var sprite = IconOf(targetRole!);
        if (!sprite)
        {
            return false;
        }

        icon = sprite!;
        return true;
    }
}
