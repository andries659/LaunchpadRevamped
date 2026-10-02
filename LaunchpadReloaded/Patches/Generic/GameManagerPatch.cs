using HarmonyLib;
using LaunchpadRevamped.Components;
using Reactor.Utilities;

namespace LaunchpadRevamped.Patches.Generic;

[HarmonyPatch(typeof(GameManager), nameof(GameManager.Awake))]
public static class GameManagerPatch
{
    public static void Postfix(GameManager __instance)
    {
        foreach (var deadBody in __instance.deadBodyPrefab)
        {
            deadBody.gameObject.AddComponent<DeadBodyCacheComponent>();
            Logger<LaunchpadRevampedPlugin>.Info("Added DeadBodyCacheComponent to dead body prefab");
        }
    }
}