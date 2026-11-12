using HarmonyLib;
using LaunchpadRevamped.Features;
using MiraAPI.Roles;
using MiraAPI.Utilities;

namespace LaunchpadRevamped.Patches.Generic;

/// <summary>
/// Adds the role's faction (for example "Impostor Concealing") under the "Your role is ..." line of the role tab.
/// </summary>
[HarmonyPatch(typeof(TaskPanelBehaviour), nameof(TaskPanelBehaviour.SetTaskText))]
public static class RoleTabFactionPatch
{
    private const string RoleLinePrefix = "Your role is";

    [HarmonyPrefix]
    public static void Prefix(ref string __0)
    {
        if (string.IsNullOrEmpty(__0) || !__0.Contains(RoleLinePrefix))
        {
            return;
        }

        var player = PlayerControl.LocalPlayer;
        if (!player || !player.Data || player.Data.Role is not ICustomRole role)
        {
            return;
        }

        if (!LaunchpadRoleGroups.TryGetFactionName(role.RoleOptionsGroup, out var faction))
        {
            return;
        }

        // The role tab text is "Your role is <role>.", then the description. The faction goes on its own line between them.
        var lineEnd = __0.IndexOf('\n');
        if (lineEnd < 0 || __0.Contains(faction))
        {
            return;
        }

        var line = $"<size=80%>{role.RoleColor.ToTextColor()}<i>{faction}</i></color></size>\n";
        __0 = __0.Insert(lineEnd + 1, line);
    }
}
