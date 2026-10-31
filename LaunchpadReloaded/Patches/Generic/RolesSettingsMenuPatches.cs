using HarmonyLib;
using LaunchpadRevamped.Features;

namespace LaunchpadRevamped.Patches.Generic;

[HarmonyPatch(typeof(RolesSettingsMenu), nameof(RolesSettingsMenu.Update))]
public static class RolesSettingsMenuPatches
{
    // MiraAPI skips the original Update on mod pages, but postfixes still run.
    [HarmonyPostfix]
    public static void UpdatePostfix(RolesSettingsMenu __instance)
    {
        RoleCategorySwitcher.Update(__instance);
    }
}

[HarmonyPatch(typeof(RolesSettingsMenu), nameof(RolesSettingsMenu.OpenChancesTab))]
public static class RolesSettingsMenuOpenPatches
{
    // MiraAPI sets up its own mod page layout when it opens the tab, so the picker's layout is applied right after.
    [HarmonyPostfix]
    public static void OpenChancesTabPostfix(RolesSettingsMenu __instance)
    {
        RoleCategorySwitcher.AfterOpenChancesTab(__instance);
    }
}
