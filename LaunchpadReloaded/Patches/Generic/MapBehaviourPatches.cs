using HarmonyLib;
using LaunchpadRevamped.Utilities;

namespace LaunchpadRevamped.Patches.Generic;

[HarmonyPatch(typeof(MapBehaviour))]
public class MapBehaviourPatches
{
    /// <summary>
    /// Only show map if click is not cancelled
    /// </summary>
    [HarmonyPatch(nameof(MapBehaviour.Show))]
    public static bool Prefix()
    {
        return !Helpers.ShouldCancelClick();
    }
}