using HarmonyLib;

namespace LaunchpadRevamped.Patches.Generic;

/// <summary>
/// The Role Count and Chance rows of the role category switcher are vanilla number options that only exist to call the
/// switcher's own change handler. The vanilla <c>NumberOption.UpdateValue</c> doesn't know them and logs
/// "Could not update value of ..." on every click, so it is skipped for those rows.
/// </summary>
[HarmonyPatch(typeof(NumberOption), "UpdateValue")]
public static class RoleDetailNumberOptionPatches
{
    [HarmonyPrepare]
    public static bool Prepare()
    {
        return AccessTools.Method(typeof(NumberOption), "UpdateValue") != null;
    }

    [HarmonyPrefix]
    public static bool Prefix(NumberOption __instance)
    {
        return !__instance.name.StartsWith("LaunchpadRole.");
    }
}
