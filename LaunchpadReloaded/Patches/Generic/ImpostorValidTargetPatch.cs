using HarmonyLib;
using LaunchpadRevamped.Options;
using LaunchpadRevamped.Utilities;
using MiraAPI.GameOptions;

namespace LaunchpadRevamped.Patches.Generic;

/// <summary>
/// Allow Impostors to kill each other if friendly fire is toggled on or an Infiltrator is in the game
/// </summary>
[HarmonyPatch(typeof(ImpostorRole), nameof(ImpostorRole.IsValidTarget))]
public static class ImpostorValidTargetPatch
{
    public static bool Prefix(ImpostorRole __instance, [HarmonyArgument(0)] NetworkedPlayerInfo target, ref bool __result)
    {
        if (!OptionGroupSingleton<FunOptions>.Instance.FriendlyFire && !InfiltratorUtilities.InfiltratorInGame())
        {
            return true;
        }
        
        __result = target is { Disconnected: false, IsDead: false } &&
                   target.PlayerId != __instance.Player.PlayerId && !(target.Role == null) &&
                   !(target.Object == null) && !target.Object.inVent && !target.Object.inMovingPlat;
        return false;

    }
}