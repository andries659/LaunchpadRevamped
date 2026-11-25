using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using AmongUs.GameOptions;
using LaunchpadReloaded.Wiki;
using LaunchpadRevamped.Modifiers;
using MiraAPI.Roles;
using UnityEngine;

namespace LaunchpadRevamped.Features;

/// <summary>
/// The <c>/r</c> chat command: lists which roles and modifiers are enabled in the current game settings and which are
/// not. Enabled roles are shown in their role colour, disabled ones in italics.
/// </summary>
public static class RoleListCommand
{
    private static List<LPModifier>? _modifiers;

    private readonly record struct Entry(string Name, Color Color, bool Enabled);

    /// <summary>The chat messages to show: first the enabled list, then the disabled list.</summary>
    public static string[] Build()
    {
        var entries = new List<Entry>();
        entries.AddRange(RoleEntries());
        entries.AddRange(ModifierEntries());

        var enabled = entries.Where(entry => entry.Enabled).Select(entry => Colored(entry.Name, entry.Color)).ToList();
        var disabled = entries.Where(entry => !entry.Enabled).Select(entry => $"<i>{entry.Name}</i>").ToList();

        var messages = new List<string>
        {
            Block("<b><color=#00FF00>Enabled Roles</color></b>", enabled, "None"),
            Block("<b><color=#FF0000>Disabled Roles</color></b>", disabled, "None"),
        };

        return messages.ToArray();
    }

    private static string Block(string header, List<string> lines, string whenEmpty)
    {
        return header + "\n" + (lines.Count == 0 ? whenEmpty : string.Join("\n", lines));
    }

    private static string Colored(string text, Color color)
    {
        return $"<color=#{ColorUtility.ToHtmlStringRGB(color)}>{text}</color>";
    }

    // Vanilla and custom roles, in the order the game lists them.
    private static IEnumerable<Entry> RoleEntries()
    {
        var options = GameOptionsManager.Instance.CurrentGameOptions;
        var roleOptions = options.RoleOptions;

        foreach (var role in RoleManager.Instance.AllRoles.ToArray())
        {
            // Skips the ghost roles, and anything the mod keeps out of the settings.
            if (!role || role.IsDead)
            {
                continue;
            }

            if (role is ICustomRole custom)
            {
                if (custom.Configuration.HideSettings)
                {
                    continue;
                }

                var enabled = (custom.GetCount() ?? 0) > 0 && (custom.GetChance() ?? 0) > 0;
                yield return new Entry(custom.RoleName, custom.RoleColor, enabled);
                continue;
            }

            bool vanillaEnabled;
            if (role.Role == RoleTypes.Impostor)
            {
                // The plain Impostor has no chance setting, it is simply as many as the Impostor count.
                vanillaEnabled = options.NumImpostors > 0;
            }
            else
            {
                vanillaEnabled = roleOptions.GetNumPerGame(role.Role) > 0 && roleOptions.GetChancePerGame(role.Role) > 0;
            }

            yield return new Entry(role.NiceName, role.NameColor, vanillaEnabled);
        }
    }

    // The modifiers players can be assigned (Modifiers/Game/<Team>/...), like the wiki's modifier page.
    private static IEnumerable<Entry> ModifierEntries()
    {
        foreach (var modifier in Modifiers())
        {
            if (!modifier.ListInRoleList)
            {
                continue;
            }

            var color = modifier.ListColor ?? WikiRegistry.ModifierTeam(modifier.GetType().Namespace ?? "").Color;
            var enabled = modifier.GetAssignmentChance() > 0 && modifier.GetAmountPerGame() > 0;
            yield return new Entry(modifier.ModifierName, color, enabled);
        }
    }

    private static List<LPModifier> Modifiers()
    {
        if (_modifiers != null)
        {
            return _modifiers;
        }

        _modifiers = new List<LPModifier>();

        Type[] types;
        try
        {
            types = typeof(RoleListCommand).Assembly.GetTypes();
        }
        catch (ReflectionTypeLoadException e)
        {
            types = e.Types.Where(type => type != null).ToArray()!;
        }

        foreach (var type in types)
        {
            try
            {
                if (type.IsAbstract || !typeof(LPModifier).IsAssignableFrom(type)
                    || !(type.Namespace ?? "").StartsWith("LaunchpadRevamped.Modifiers.Game", StringComparison.Ordinal))
                {
                    continue;
                }

                _modifiers.Add((LPModifier)Activator.CreateInstance(type)!);
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogWarning("[RoleList] Skipping modifier " + type.Name + ": " + e.Message);
            }
        }

        _modifiers = _modifiers.OrderBy(modifier => modifier.ModifierName, StringComparer.Ordinal).ToList();
        return _modifiers;
    }
}
