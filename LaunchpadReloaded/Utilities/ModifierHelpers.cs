using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AmongUs.GameOptions;
using HarmonyLib;
using MiraAPI.Roles;
using UnityEngine;

namespace LaunchpadRevamped.Utilities;

/// <summary>
/// Small helpers shared by the extra (TOHO) modifiers.
/// </summary>
public static class ModifierHelpers
{
    public static float BaseKillCooldown => GameOptionsManager.Instance.CurrentGameOptions.GetFloat(FloatOptionNames.KillCooldown);

    /// <summary>True while the Electrical (lights) sabotage is active.</summary>
    public static bool IsLightsOut()
    {
        var ship = ShipStatus.Instance;
        return ship && ship.Systems.TryGetValue(SystemTypes.Electrical, out var system) &&
               system.TryCast<SwitchSystem>() is { } switchSystem &&
               switchSystem.Value < SwitchSystem.MaxValue;
    }

    /// <summary>True for roles that use the normal kill button (Overclocked, Underclocked and Reach only make sense there).</summary>
    public static bool HasVanillaKillButton(RoleBehaviour role)
    {
        return role is ICustomRole { Configuration.UseVanillaKillButton: true };
    }

    /// <summary>Every living player on the Impostor team (Coven and Neutral roles do not count).</summary>
    public static PlayerControl[] AliveImpostorTeam()
    {
        return PlayerControl.AllPlayerControls.ToArray()
            .Where(player => player && player.Data && !player.Data.IsDead && !player.Data.Disconnected &&
                             player.Data.Role && RoleTeamChecks.IsImpostorTeam(player.Data.Role))
            .ToArray();
    }

    /// <summary>
    /// Adds time to the emergency button cooldown. The field is looked up by name, so a different game version simply
    /// does nothing instead of breaking the build.
    /// </summary>
    public static void AddEmergencyCooldown(float seconds)
    {
        var ship = ShipStatus.Instance;
        if (!ship)
        {
            return;
        }

        var traverse = Traverse.Create(ship);
        var property = traverse.Property("EmergencyCooldown");
        if (property.PropertyExists())
        {
            property.SetValue(property.GetValue<float>() + seconds);
            return;
        }

        var field = traverse.Field("EmergencyCooldown");
        if (field.FieldExists())
        {
            field.SetValue(field.GetValue<float>() + seconds);
        }
    }

    /// <summary>Reads a field or property by name, returning default when it does not exist.</summary>
    public static T? ReadMember<T>(object? target, string name)
    {
        if (target == null)
        {
            return default;
        }

        var traverse = Traverse.Create(target);
        var field = traverse.Field(name);
        if (field.FieldExists())
        {
            return field.GetValue<T>();
        }

        var property = traverse.Property(name);
        return property.PropertyExists() ? property.GetValue<T>() : default;
    }

    /// <summary>The player who called the current meeting (button or body report).</summary>
    public static PlayerControl? MeetingCaller() => ReadMember<PlayerControl>(MeetingRoomManager.Instance, "reporter");

    /// <summary>The body that was reported to start the current meeting, or null for an emergency meeting.</summary>
    public static NetworkedPlayerInfo? ReportedBody() => ReadMember<NetworkedPlayerInfo>(MeetingRoomManager.Instance, "target");

    /// <summary>Flashes the whole screen in a colour and fades it out.</summary>
    public static IEnumerator CoFlash(Color color, float duration)
    {
        var screen = HudManager.Instance.FullScreen;
        screen.gameObject.SetActive(true);
        for (var time = 0f; time < duration; time += Time.deltaTime)
        {
            screen.color = new Color(color.r, color.g, color.b, Mathf.Lerp(color.a, 0f, time / duration));
            yield return null;
        }

        screen.color = Color.clear;
        screen.gameObject.SetActive(false);
    }
}
