using HarmonyLib;
using LaunchpadRevamped.Utilities;

namespace LaunchpadRevamped.Patches.Roles.Snitch;

[HarmonyPatch(typeof(HudManager), nameof(HudManager.Update))]
public static class SnitchPatches
{
    [HarmonyPostfix]
    public static void UpdatePostfix()
    {
        SnitchUtilities.Update();
    }
}
