using HarmonyLib;
using LaunchpadRevamped.Networking.Roles;
using LaunchpadRevamped.Options.Roles.Crewmate;
using LaunchpadRevamped.Roles.Crewmate;
using MiraAPI.GameOptions;
using Random = UnityEngine.Random;

namespace LaunchpadRevamped.Patches.Roles.Lucky;

/// <summary>
/// Lets a Lucky survive a standard kill attempt. The host decides, since CheckMurder is host-authoritative.
/// Custom murders (Sheriff, Hitman, Bomber, burns, ...) bypass CheckMurder and are not affected.
/// </summary>
[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.CheckMurder))]
public static class LuckyPatches
{
    [HarmonyPrefix]
    public static bool CheckMurderPrefix(PlayerControl __instance, PlayerControl target)
    {
        if (!AmongUsClient.Instance.AmHost || !target || !target.Data || target.Data.IsDead)
        {
            return true;
        }

        if (target.Data.Role is not LuckyRole lucky)
        {
            return true;
        }

        var options = OptionGroupSingleton<LuckyOptions>.Instance;
        var maxDodges = (int)options.MaxDodges;
        if (maxDodges > 0 && lucky.DodgesUsed >= maxDodges)
        {
            return true;
        }

        if (Random.value * 100f >= options.DodgeChance)
        {
            return true;
        }

        lucky.DodgesUsed++;
        target.RpcLuckyDodge(__instance);
        return false;
    }
}
