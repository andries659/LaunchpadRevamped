using HarmonyLib;
using LaunchpadRevamped.Features;
using MiraAPI.LocalSettings;

namespace LaunchpadRevamped.Patches.Generic;

[HarmonyPatch(typeof(FollowerCamera), nameof(FollowerCamera.SnapToTarget))]
public static class BloomPatch
{
    public static void Postfix()
    {
        LaunchpadSettings.SetBloom(LocalSettingsTabSingleton<LaunchpadSettings>.Instance.Bloom.Value);
    }
}