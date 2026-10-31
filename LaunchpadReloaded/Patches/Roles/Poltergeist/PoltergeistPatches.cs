using HarmonyLib;
using LaunchpadRevamped.Options.Roles.Impostor;
using LaunchpadRevamped.Roles.Impostor;
using MiraAPI.GameOptions;
using MiraAPI.Roles;
using AmongUs.GameOptions;
using Random = UnityEngine.Random;

namespace LaunchpadRevamped.Patches.Roles.Poltergeist;

/// <summary>
/// When an Impostor dies there is a configurable chance that they come back as a Poltergeist instead of a plain
/// Impostor ghost. Runs before MiraAPI's own ghost role handling so that it can take over the assignment.
/// </summary>
[HarmonyPatch(typeof(RoleManager), nameof(RoleManager.AssignRoleOnDeath))]
public static class PoltergeistPatches
{
    [HarmonyPrefix]
    [HarmonyPriority(Priority.First)]
    public static bool AssignRoleOnDeathPrefix([HarmonyArgument(0)] PlayerControl plr)
    {
        if (!AmongUsClient.Instance.AmHost || !plr || !plr.Data || !plr.Data.IsDead)
        {
            return true;
        }

        var role = plr.Data.Role;
        if (role is PoltergeistRole || !role.IsImpostor)
        {
            return true;
        }

        // Custom Impostor roles that ask for a specific ghost role keep it.
        if (role is ICustomRole custom && custom.Configuration.GhostRole != RoleTypes.ImpostorGhost)
        {
            return true;
        }

        // The chance is the Poltergeist's own role chance from the role settings.
        if (RoleManager.Instance.GetRole((RoleTypes)RoleId.Get<PoltergeistRole>()) is not ICustomRole poltergeist
            || Random.value * 100f >= poltergeist.GetChance())
        {
            return true;
        }

        plr.RpcSetRole((RoleTypes)RoleId.Get<PoltergeistRole>());
        return false;
    }
}