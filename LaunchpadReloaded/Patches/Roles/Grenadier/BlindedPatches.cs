using HarmonyLib;
using LaunchpadRevamped.Modifiers;
using MiraAPI.Modifiers;

namespace LaunchpadRevamped.Patches.Roles.Grenadier;

/// <summary>
/// Tunnel vision for as long as the local player is blinded.
/// </summary>
[HarmonyPatch(typeof(AirshipStatus), nameof(AirshipStatus.CalculateLightRadius))]
[HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.CalculateLightRadius))]
public static class BlindedPatches
{
    public static void Postfix(ShipStatus __instance, NetworkedPlayerInfo player, ref float __result)
    {
        if (!player || !player.Object || !player.Object.AmOwner || !player.Object.HasModifier<BlindedModifier>())
        {
            return;
        }

        __result = __instance.MinLightRadius * 0.5f;
    }
}
