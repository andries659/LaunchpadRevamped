using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using LaunchpadRevamped.Modifiers;
using MiraAPI.GameOptions;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Modifiers;
using MiraAPI.Modifiers.Types;
using MiraAPI.Roles;
using MiraAPI.Translation;
using MiraAPI.Utilities;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace LaunchpadReloaded.Wiki; // <-- change to your mod's namespace

/// <summary>
/// Builds the entries for the "My Role" page: the local player's role (description, abilities and the role's
/// current settings) followed by any modifiers they have (description only).
/// </summary>
public static class WikiMyRole
{
    private static Dictionary<Type, AbstractOptionGroup>? _roleGroups;

    public static List<WikiRole> Build()
    {
        var list = new List<WikiRole>();

        var player = PlayerControl.LocalPlayer;
        if (!player || player.Data == null) return list; // not in a game yet

        try
        {
            var role = player.Data.Role;
            if (role) list.Add(BuildRole(role));
        }
        catch (Exception e)
        {
            Debug.LogError("[Wiki] Could not build the role page: " + e);
        }

        try
        {
            foreach (var modifier in player.GetModifierComponent().ActiveModifiers.ToList())
            {
                var entry = BuildModifier(modifier);
                if (entry != null) list.Add(entry);
            }
        }
        catch (Exception e)
        {
            Debug.LogError("[Wiki] Could not build the modifier pages: " + e);
        }

        return list;
    }

    // ------------------------------------------------------------------ role

    private static WikiRole BuildRole(RoleBehaviour role)
    {
        if (role is ICustomRole custom)
        {
            var source = WikiRegistry.Roles.FirstOrDefault(r => r.Name == custom.RoleName);

            // roles hidden from the wiki are not in the registry: build the entry here instead
            var entry = source != null
                ? Copy(source)
                : new WikiRole
                {
                    Name = custom.RoleName,
                    Team = WikiRegistry.TeamName(role, custom),
                    Color = custom.RoleColor,
                    Description = WikiRegistry.WikiDescription(role, custom),
                    Icon = WikiRegistry.LoadSprite(() => custom.Configuration.Icon?.LoadAsset()),
                };

            entry.Settings = BuildSettings(role.GetType());
            return entry;
        }

        // vanilla roles have no abilities or settings of their own
        bool impostor = role.TeamType == RoleTeamTypes.Impostor;
        return new WikiRole
        {
            Name = role.NiceName,
            Team = impostor ? "Impostor" : "Crewmate",
            Color = impostor ? new Color(1f, 0.30f, 0.30f) : new Color(0.55f, 1f, 1f),
            Description = impostor
                ? "Kill the crew and sabotage the ship without getting caught."
                : "Finish your tasks and find the Impostors.",
        };
    }

    /// <summary>A copy of a registry entry, without the amount/chance (they make no sense for your own role).</summary>
    private static WikiRole Copy(WikiRole source)
    {
        return new WikiRole
        {
            Name = source.Name,
            Team = source.Team,
            Color = source.Color,
            Description = source.Description,
            GetDescription = source.GetDescription,
            Icon = source.Icon,
            Abilities = source.Abilities,
        };
    }

    // ------------------------------------------------------------------ modifiers

    private static WikiRole? BuildModifier(BaseModifier modifier)
    {
        if (modifier is not GameModifier) return null; // skip status effects (hacked, doused, ...)

        var name = modifier.ModifierName;
        var source = WikiRegistry.Modifiers.FirstOrDefault(m => m.Name == name);
        if (source != null)
        {
            var entry = Copy(source);
            entry.Team = "Modifier";
            return entry;
        }

        if (modifier is LPModifier) return null; // one of ours that is not shown in the wiki

        // modifiers from other mods
        return new WikiRole
        {
            Name = name,
            Team = "Modifier",
            Color = new Color(0.74f, 0.52f, 1f),
            Description = WikiRegistry.SafeText(() => modifier.GetDescription()),
            GetDescription = () => WikiRegistry.SafeText(() => modifier.GetDescription()),
            Icon = WikiRegistry.LoadSprite(() => modifier.ModifierIcon?.LoadAsset()),
        };
    }

    // ------------------------------------------------------------------ role settings

    private static List<WikiAbility> BuildSettings(Type roleType)
    {
        var rows = new List<WikiAbility>();

        var group = FindGroup(roleType);
        if (group == null) return rows;

        foreach (var option in group.Children)
        {
            try
            {
                if (option.Visible != null && !option.Visible()) continue; // e.g. a setting that depends on a toggle

                rows.Add(new WikiAbility
                {
                    Name = MiraLocaleManager.Get(option.Title),
                    Cooldown = FormatValue(option), // shown on the right of the row
                });
            }
            catch (Exception e)
            {
                Debug.LogWarning("[Wiki] Skipping a setting: " + e.Message);
            }
        }

        return rows;
    }

    private static AbstractOptionGroup? FindGroup(Type roleType)
    {
        _roleGroups ??= LoadGroups();
        return _roleGroups.TryGetValue(roleType, out var group) ? group : null;
    }

    /// <summary>
    /// MiraAPI keeps its option groups internal, so find them the way OptionGroupSingleton does: through the
    /// group classes of this mod. A group belongs to a role when its OptionableType is the role's class.
    /// </summary>
    private static Dictionary<Type, AbstractOptionGroup> LoadGroups()
    {
        var map = new Dictionary<Type, AbstractOptionGroup>();

        Type[] types;
        try
        {
            types = typeof(WikiMyRole).Assembly.GetTypes();
        }
        catch (ReflectionTypeLoadException e)
        {
            types = e.Types.Where(t => t != null).ToArray()!;
        }

        foreach (var type in types)
        {
            try
            {
                if (type.IsAbstract || !typeof(AbstractOptionGroup).IsAssignableFrom(type)) continue;

                var singleton = typeof(OptionGroupSingleton<>).MakeGenericType(type);
                var group = singleton.GetProperty("Instance", BindingFlags.Public | BindingFlags.Static)?.GetValue(null) as AbstractOptionGroup;
                var target = group?.OptionableType;
                if (group != null && target != null && !map.ContainsKey(target)) map[target] = group;
            }
            catch
            {
                // group not registered (yet): ignore it
            }
        }

        if (map.Count == 0) _roleGroups = null; // nothing found, try again next time instead of caching an empty list
        return map;
    }

    private static string FormatValue(IModdedOption option)
    {
        switch (option)
        {
            case ModdedToggleOption toggle:
                return toggle.Value ? toggle.OnValue : toggle.OffValue;

            case ModdedNumberOption number:
            {
                var value = number.Value;
                if (value == 0f && !string.IsNullOrEmpty(number.ZeroWordValue)) return number.ZeroWordValue;
                if (value < 0f && !string.IsNullOrEmpty(number.NegativeWordValue)) return number.NegativeWordValue;

                var text = value.ToString("0.##", CultureInfo.InvariantCulture);
                return number.SuffixType switch
                {
                    MiraNumberSuffixes.Seconds => text + "s",
                    MiraNumberSuffixes.Percent => text + "%",
                    MiraNumberSuffixes.Multiplier => text + "x",
                    MiraNumberSuffixes.Distance => text + "u",
                    _ => text,
                };
            }
        }

        // enum / string options: GetFloatData() is the index into Values
        var index = Mathf.RoundToInt(option.GetFloatData());
        if (option.GetType().GetProperty("Values")?.GetValue(option) is string[] values && index >= 0 && index < values.Length)
            return values[index];

        return option.GetFloatData().ToString("0.##", CultureInfo.InvariantCulture);
    }
}
