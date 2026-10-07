using HarmonyLib;
using LaunchpadRevamped.Utilities;

namespace LaunchpadRevamped.Patches.Roles.TimeRewinder;

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.FixedUpdate))]
public static class RewindPatches
{
    [HarmonyPostfix]
    public static void FixedUpdatePostfix(PlayerControl __instance)
    {
        if (__instance.AmOwner)
        {
            RewindHistory.Sample(__instance);
        }
    }
}
