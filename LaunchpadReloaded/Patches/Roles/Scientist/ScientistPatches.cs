using HarmonyLib;
using LaunchpadRevamped.Utilities;

namespace LaunchpadRevamped.Patches.Roles.Scientist;

[HarmonyPatch(typeof(ScientistRole))]
public static class ScientistPatches
{
    [HarmonyPatch(nameof(ScientistRole.RefreshAbilityButton))]
    public static bool Prefix()
    {
        if (!PlayerControl.LocalPlayer.Data.IsHacked()) return true;

        DestroyableSingleton<HudManager>.Instance.AbilityButton.SetDisabled();
        return false;

    }
}