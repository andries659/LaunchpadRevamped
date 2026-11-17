using HarmonyLib;
using LaunchpadRevamped.Utilities;
using AmongUs.GameOptions;

namespace LaunchpadRevamped.Patches.Roles.Infiltrator;

/// <summary>Impostors see the Infiltrator's name in red (in-game).</summary>
[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.FixedUpdate))]
public static class InfiltratorNameColorPatch
{
    public static void Postfix(PlayerControl __instance)
    {
        if (__instance.AmOwner || !__instance.IsInfiltrator() || !InfiltratorUtilities.LocalIsImpostor())
        {
            return;
        }

        __instance.cosmetics.SetNameColor(Palette.ImpostorRed);
    }
}

/// <summary>Impostors see the Infiltrator's name in red (meeting screen).</summary>
[HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Start))]
public static class InfiltratorMeetingNamePatch
{
    public static void Postfix(MeetingHud __instance)
    {
        if (!InfiltratorUtilities.LocalIsImpostor())
        {
            return;
        }

        foreach (var state in __instance.playerStates)
        {
            var player = GameData.Instance.GetPlayerById(state.PlayerId)?.Object;
            if (player && !player.AmOwner && player.IsInfiltrator())
            {
                state.NameText.color = Palette.ImpostorRed;
            }
        }
    }
}

/// <summary>Lists the Infiltrator(s) in the Impostors' intro team so they look like a partner.</summary>
[HarmonyPatch(typeof(IntroCutscene), nameof(IntroCutscene.BeginImpostor))]
public static class InfiltratorIntroPatch
{
    public static void Prefix([HarmonyArgument(0)] ref Il2CppSystem.Collections.Generic.List<PlayerControl> yourTeam)
    {
        foreach (var player in PlayerControl.AllPlayerControls.ToArray())
        {
            if (player.IsInfiltrator() && !yourTeam.Contains(player))
            {
                yourTeam.Add(player);
            }
        }
    }
}

/// <summary>The lights sabotage doesn't affect the Infiltrator.</summary>
[HarmonyPatch(typeof(AirshipStatus), nameof(AirshipStatus.CalculateLightRadius))]
[HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.CalculateLightRadius))]
public static class InfiltratorLightPatch
{
    public static void Postfix(ShipStatus __instance, NetworkedPlayerInfo player, ref float __result)
    {
        if (!player.Object.AmOwner || !player.Object.IsInfiltrator())
        {
            return;
        }

        // Only undo the sabotage so other light effects (torch, blind) keep working.
        if (__instance.Systems.TryGetValue(SystemTypes.Electrical, out var system) &&
            system.TryCast<SwitchSystem>() is { } switchSystem &&
            switchSystem.Value < SwitchSystem.MaxValue)
        {
            __result = __instance.MaxLightRadius * GameOptionsManager.Instance.CurrentGameOptions.GetFloat(FloatOptionNames.CrewLightMod);
        }
    }
}
