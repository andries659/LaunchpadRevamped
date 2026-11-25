using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using AmongUs.GameOptions;
using HarmonyLib;
using LaunchpadRevamped.Buttons.Impostor;
using LaunchpadRevamped.Features;
using LaunchpadRevamped.Modifiers.Game.Coven;
using LaunchpadRevamped.Modifiers.Game.Impostor;
using LaunchpadRevamped.Modifiers.Game.Neutral;
using LaunchpadRevamped.Options.Modifiers;
using LaunchpadRevamped.Utilities;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Networking;
using UnityEngine;

namespace LaunchpadRevamped.Patches.Modifiers;

// Harmony patches of the modifiers ported from TOHO that change something the game does by itself.

/// <summary>
/// Kill cooldown: Mare (lights out), Last Impostor, Overclocked and Underclocked. The first argument of SetKillTimer is
/// the new cooldown. Tiny values are left alone, they are "ready now" resets.
/// </summary>
[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.SetKillTimer))]
public static class ExtraKillTimerPatch
{
    public static void Prefix(PlayerControl __instance, ref float __0)
    {
        if (__0 <= 0.5f || !__instance)
        {
            return;
        }

        var impostor = OptionGroupSingleton<ExtraImpostorModifierOptions>.Instance;
        var neutral = OptionGroupSingleton<ExtraNeutralModifierOptions>.Instance;
        var time = __0;

        if (__instance.HasModifier<MareModifier>() && ModifierHelpers.IsLightsOut())
        {
            time = Mathf.Min(time, impostor.MareKillCooldown);
        }

        if (__instance.HasModifier<LastImpostorModifier>())
        {
            time *= 1f - impostor.LastImpostorReduction / 100f;
        }

        if (__instance.HasModifier<OverclockedModifier>())
        {
            time *= 1f - neutral.OverclockedReduction / 100f;
        }

        if (__instance.HasModifier<UnderclockedModifier>())
        {
            time *= 1f + neutral.UnderclockedIncrease / 100f;
        }

        __0 = time;
    }
}

/// <summary>Swift: the killer does not jump to the body.</summary>
[HarmonyPatch]
public static class SwiftPatch
{
    public static IEnumerable<MethodBase> TargetMethods()
    {
        return typeof(CustomMurderRpc).GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Where(method => method.Name == "CustomMurder");
    }

    public static void Prefix(PlayerControl source, ref bool teleportMurderer)
    {
        if (source && source.HasModifier<SwiftModifier>())
        {
            teleportMurderer = false;
        }
    }
}

/// <summary>Who may use the sabotage button without their role allowing it: Subversion (Neutral) and Saboteur (Coven).</summary>
internal static class SabotageAccess
{
    public static bool Has(PlayerControl? player)
    {
        return player && player!.Data && !player.Data.IsDead &&
               (player.HasModifier<SubversionModifier>() || player.HasModifier<SaboteurModifier>());
    }
}

[HarmonyPatch(typeof(HudManager), nameof(HudManager.SetHudActive), typeof(PlayerControl), typeof(RoleBehaviour), typeof(bool))]
public static class SabotageButtonVisiblePatch
{
    // Last, so it runs after MiraAPI hides the button for roles without sabotage.
    [HarmonyPostfix]
    [HarmonyPriority(Priority.Last)]
    public static void Postfix(HudManager __instance, PlayerControl localPlayer, bool isActive)
    {
        if (SabotageAccess.Has(localPlayer))
        {
            __instance.SabotageButton.gameObject.SetActive(isActive);
        }
    }
}

[HarmonyPatch(typeof(SabotageButton), nameof(SabotageButton.DoClick))]
public static class SabotageButtonClickPatch
{
    // First, so MiraAPI's own check (which blocks roles without sabotage) is skipped for these players.
    [HarmonyPrefix]
    [HarmonyPriority(Priority.First)]
    public static bool Prefix()
    {
        var player = PlayerControl.LocalPlayer;
        if (!SabotageAccess.Has(player))
        {
            return true;
        }

        if (player.inVent || !GameManager.Instance.SabotagesEnabled())
        {
            return false;
        }

        HudManager.Instance.ToggleMapVisible(new MapOptions { Mode = MapOptions.Modes.Sabotage });
        return false;
    }
}

/// <summary>Watcher: votes are never anonymous for you.</summary>
[HarmonyPatch]
public static class WatcherPatch
{
    public static IEnumerable<MethodBase> TargetMethods()
    {
        return AccessTools.GetTypesFromAssembly(typeof(LogicOptions).Assembly)
            .Where(type => typeof(LogicOptions).IsAssignableFrom(type))
            .Select(type => (MethodBase?)AccessTools.DeclaredMethod(type, "GetAnonymousVotes"))
            .Where(method => method != null)
            .Select(method => method!);
    }

    public static void Postfix(ref bool __result)
    {
        var local = PlayerControl.LocalPlayer;
        if (__result && local && MeetingHud.Instance && local.HasModifier<WatcherModifier>())
        {
            __result = false;
        }
    }
}

/// <summary>Necroview: dead players' names in a meeting are coloured by team.</summary>
[HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Update))]
public static class NecroviewPatch
{
    public static void Postfix(MeetingHud __instance)
    {
        var local = PlayerControl.LocalPlayer;
        if (!local || !local.Data || local.Data.IsDead || !local.HasModifier<NecroviewModifier>())
        {
            return;
        }

        foreach (var area in __instance.playerStates)
        {
            var info = GameData.Instance.GetPlayerById(area.PlayerId);
            if (info == null || !info.IsDead || !info.Role)
            {
                continue;
            }

            area.NameText.color = TeamColor(info.Role);
        }
    }

    private static Color TeamColor(RoleBehaviour role)
    {
        if (RoleTeamChecks.IsCoven(role))
        {
            return LaunchpadPalette.CovenColor;
        }

        if (RoleTeamChecks.IsNeutral(role))
        {
            return Color.gray;
        }

        return RoleTeamChecks.IsImpostorTeam(role) ? Palette.ImpostorRed : Palette.CrewmateBlue;
    }
}

/// <summary>
/// Chronos: the meeting timer counts up, so adding to it shortens the discussion and subtracting from it lengthens it.
/// Every client does the same, so they all agree.
/// </summary>
[HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Start))]
public static class ChronosPatch
{
    public static void Postfix(MeetingHud __instance)
    {
        var discussion = GameOptionsManager.Instance.CurrentGameOptions.GetInt(Int32OptionNames.DiscussionTime);
        var caller = ModifierHelpers.MeetingCaller();
        if (discussion <= 0 || !caller)
        {
            return;
        }

        var options = OptionGroupSingleton<ExtraNeutralModifierOptions>.Instance;
        if (caller!.HasModifier<ChronosModifier>())
        {
            __instance.discussionTimer += discussion * options.ChronosCut / 100f;
        }
        else if (PlayerControl.AllPlayerControls.ToArray().Any(player =>
                     player && player.Data && !player.Data.IsDead && player.HasModifier<ChronosModifier>()))
        {
            __instance.discussionTimer -= options.ChronosExtend;
        }
    }
}

/// <summary>Loyal: the Puppeteer's control and the Vampire's bite fail on a Loyal player.</summary>
internal static class LoyalCheck
{
    public static bool Resists(PlayerControl? target)
    {
        if (!target || !target!.HasModifier<LoyalModifier>())
        {
            return false;
        }

        MiraAPI.Utilities.Helpers.CreateAndShowNotification($"{target.Data.PlayerName} is loyal and cannot be turned!", LaunchpadPalette.CovenColor);
        return true;
    }
}

[HarmonyPatch(typeof(PuppetButton), "OnClick")]
public static class LoyalPuppetPatch
{
    public static bool Prefix(PuppetButton __instance)
    {
        if (!LoyalCheck.Resists(__instance.Target))
        {
            return true;
        }

        __instance.ResetTarget();
        return false;
    }
}

[HarmonyPatch(typeof(BiteButton), "OnClick")]
public static class LoyalBitePatch
{
    public static bool Prefix(BiteButton __instance)
    {
        if (!LoyalCheck.Resists(__instance.Target))
        {
            return true;
        }

        __instance.ResetTarget();
        return false;
    }
}
