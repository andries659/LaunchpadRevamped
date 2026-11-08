using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using LaunchpadRevamped.Modifiers;
using LaunchpadRevamped.Roles.Coven;
using MiraAPI.Hud;
using MiraAPI.Roles;
using MiraAPI.Translation;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace LaunchpadReloaded.Wiki; // <-- change to your mod's namespace

/// <summary>One ability shown as a block on a role's page.</summary>
public class WikiAbility
{
    public string Name = "Ability";
    public string Description = "";
    public string Cooldown = "";      // e.g. "Cooldown: 25s" / "Passive". Empty = hidden.
    public Sprite? Icon;              // shown on the left of the block. null = dim placeholder.
}

/// <summary>One role shown as a button in the left list.</summary>
public class WikiRole
{
    public string Name = "Role";
    public string Team = "Crewmate";  // free text, so custom teams work
    public Color Color = Color.white; // role colour (name, accent bar, ability titles)
    public string Description = "";
    public Func<string>? GetDescription; // optional: read live each time the entry is shown (overrides Description)
    public Sprite? Icon;              // shown in the left list. null = dim placeholder.
    public Func<int?>? GetAmount;     // role amount (how many can spawn). Read live each time the role is shown. null = hidden.
    public Func<int?>? GetChance;     // role chance 0-100. Read live each time the role is shown. null = hidden.
    public List<WikiAbility> Abilities = new();
}

/// <summary>
/// Holds the roles the wiki shows. Every MiraAPI role registered by the mod is added automatically the first time
/// the list is read (roles only exist once MiraAPI has finished loading, so this can't happen in Load()).
/// You can still call <see cref="Register"/> to add or override entries by hand.
/// </summary>
public static class WikiRegistry
{
    private static readonly List<WikiRole> RoleList = new();
    private static bool _autoDone;

    public static List<WikiRole> Roles
    {
        get
        {
            AutoPopulate();
            return RoleList;
        }
    }

    private static readonly List<WikiRole> ModifierList = new();
    private static readonly List<WikiRole> SettingList = new();
    private static bool _modifiersDone, _settingsDone;

    /// <summary>Modifiers shown on the "Modifiers" page. Built from the mod's game modifiers on first read.</summary>
    public static List<WikiRole> Modifiers
    {
        get
        {
            AutoPopulateModifiers();
            return ModifierList;
        }
    }

    /// <summary>Entries on the "Settings" page: one per option group, each option shown as a row.</summary>
    public static List<WikiRole> Settings
    {
        get
        {
            AutoPopulateSettings();
            return SettingList;
        }
    }

    public static void Register(WikiRole role)
    {
        RoleList.RemoveAll(r => r.Name == role.Name); // manual entries replace auto-generated ones
        RoleList.Add(role);
    }

    public static void RegisterModifier(WikiRole modifier)
    {
        ModifierList.RemoveAll(r => r.Name == modifier.Name);
        ModifierList.Add(modifier);
    }

    public static void RegisterSetting(WikiRole setting)
    {
        SettingList.RemoveAll(r => r.Name == setting.Name);
        SettingList.Add(setting);
    }

    private static void AutoPopulate()
    {
        if (_autoDone) return;

        var behaviours = CustomRoleManager.CustomRoleBehaviours;
        if (behaviours == null || behaviours.Count == 0) return; // MiraAPI not ready yet, try again next time
        _autoDone = true;

        foreach (var behaviour in behaviours)
        {
            try
            {
                if (behaviour == null || behaviour.IsDead) continue;
                if (behaviour is not ICustomRole custom) continue;

                var config = custom.Configuration;
                if (config.HideSettings) continue;

                var role = new WikiRole
                {
                    Name = custom.RoleName,
                    Team = TeamName(behaviour, custom),
                    Color = custom.RoleColor,
                    Description = WikiDescription(behaviour, custom),
                    Icon = LoadSprite(() => config.Icon?.LoadAsset()),
                    GetAmount = () => custom.GetCount(),   // MiraAPI: ICustomRole.GetCount()  (int?)
                    GetChance = () => custom.GetChance(),  // MiraAPI: ICustomRole.GetChance() (int?)
                };

                foreach (var button in CustomButtonManager.Buttons)
                {
                    try
                    {
                        if (!button.Enabled(behaviour)) continue;

                        var cooldown = button.Cooldown;
                        role.Abilities.Add(new WikiAbility
                        {
                            Name = button.Name.Translate(),
                            Description = AbilityDescription(button.Name),
                            Cooldown = cooldown > 0f ? "Cooldown: " + Mathf.RoundToInt(cooldown) + "s" : "",
                            Icon = LoadSprite(() => button.Sprite?.LoadAsset()),
                        });
                    }
                    catch (Exception e)
                    {
                        Debug.LogWarning("[Wiki] Skipping a button for " + role.Name + ": " + e.Message);
                    }
                }

                if (RoleList.Any(r => r.Name == role.Name)) continue; // already registered by hand
                RoleList.Add(role);
            }
            catch (Exception e)
            {
                Debug.LogError("[Wiki] Failed to add role '" + behaviour?.name + "': " + e);
            }
        }

        RoleList.Sort((a, b) =>
        {
            var byTeam = string.Compare(a.Team, b.Team, StringComparison.Ordinal);
            return byTeam != 0 ? byTeam : string.Compare(a.Name, b.Name, StringComparison.Ordinal);
        });
    }

    /// <summary>
    /// Looks up "&lt;Role&gt;WikiDesc" (e.g. "CaptainWikiDesc") in the mod's locale files.
    /// Tries the class name without the "Role" suffix first (CaptainRole -> Captain), then the role's display name
    /// with spaces removed. Falls back to the role's long description if no key exists.
    /// </summary>
    private static string WikiDescription(RoleBehaviour behaviour, ICustomRole custom)
    {
        var candidates = new List<string>();

        var typeName = behaviour.GetType().Name;
        if (typeName.EndsWith("Role", StringComparison.Ordinal) && typeName.Length > 4)
            typeName = typeName.Substring(0, typeName.Length - 4);
        candidates.Add(typeName);
        candidates.Add(custom.RoleName.Replace(" ", ""));

        foreach (var name in candidates.Distinct())
        {
            var key = name + "WikiDesc";
            var text = MiraLocaleManager.Get(key);
            if (!string.IsNullOrEmpty(text) && text != key) return text; // Get() returns the key itself when it's missing
        }

        return custom.RoleLongDescription;
    }

    private static void AutoPopulateModifiers()
    {
        if (_modifiersDone) return;
        _modifiersDone = true;

        Type[] types;
        try
        {
            types = typeof(WikiRegistry).Assembly.GetTypes();
        }
        catch (ReflectionTypeLoadException e)
        {
            types = e.Types.Where(t => t != null).ToArray()!;
        }

        foreach (var type in types)
        {
            try
            {
                if (type.IsAbstract || !typeof(LPModifier).IsAssignableFrom(type)) continue;

                // only the modifiers players can be assigned (Modifiers/Game/<Team>/...), not status effects like Hacked
                var ns = type.Namespace ?? "";
                if (!ns.StartsWith("LaunchpadRevamped.Modifiers.Game", StringComparison.Ordinal)) continue;

                var mod = (LPModifier)Activator.CreateInstance(type)!;
                var crew = ns.EndsWith(".Crewmate", StringComparison.Ordinal);

                var entry = new WikiRole
                {
                    Name = mod.ModifierName,
                    Team = crew ? "Crewmate" : "Universal",
                    Color = crew ? new Color(0.55f, 1f, 1f) : new Color(0.74f, 0.52f, 1f),
                    Description = SafeText(() => mod.GetDescription()),
                    GetDescription = () => SafeText(() => mod.GetDescription()),
                    GetAmount = () => mod.GetAmountPerGame(),
                    GetChance = () => mod.GetAssignmentChance(),
                };

                if (ModifierList.Any(r => r.Name == entry.Name)) continue; // already registered by hand
                ModifierList.Add(entry);
            }
            catch (Exception e)
            {
                Debug.LogWarning("[Wiki] Skipping modifier " + type.Name + ": " + e.Message);
            }
        }

        ModifierList.Sort((a, b) =>
        {
            var byTeam = string.Compare(a.Team, b.Team, StringComparison.Ordinal);
            return byTeam != 0 ? byTeam : string.Compare(a.Name, b.Name, StringComparison.Ordinal);
        });
    }

    private static void AutoPopulateSettings()
    {
        if (_settingsDone) return;
        _settingsDone = true;

        // Names come from the existing option keys; text comes from "<key>.WikiDesc" in the locale files.
        AddSettingGroup("launchpad.options.general", "notepad", "banCheaters", "disableMeetingTeleport");
        AddSettingGroup("launchpad.options.funOptions", "friendlyFire", "uniqueColors", "character");
        AddSettingGroup("launchpad.options.votingType", "votingType", "maxVotes", "allowMultipleVotesOnSamePlayer",
            "allowConfirmingVotes", "hideVotingIcons", "showPercentages");
    }

    private static void AddSettingGroup(string groupKey, params string[] optionKeys)
    {
        try
        {
            var group = new WikiRole
            {
                Name = MiraLocaleManager.Get(groupKey),
                Team = "Game Settings",
                Color = new Color(1f, 0.74f, 0.30f),
                Description = LocaleOrEmpty(groupKey + ".WikiDesc"),
            };

            foreach (var option in optionKeys)
            {
                var key = groupKey + "." + option;
                group.Abilities.Add(new WikiAbility
                {
                    Name = MiraLocaleManager.Get(key),
                    Description = LocaleOrEmpty(key + ".WikiDesc"),
                });
            }

            if (SettingList.Any(r => r.Name == group.Name)) return; // already registered by hand
            SettingList.Add(group);
        }
        catch (Exception e)
        {
            Debug.LogWarning("[Wiki] Skipping settings group " + groupKey + ": " + e.Message);
        }
    }

    private static string SafeText(Func<string> get)
    {
        try { return get() ?? ""; }
        catch { return ""; }
    }

    private static string LocaleOrEmpty(string key)
    {
        var text = MiraLocaleManager.Get(key);
        return !string.IsNullOrEmpty(text) && text != key ? text : ""; // Get() returns the key itself when it's missing
    }

    /// <summary>
    /// Looks up "&lt;button key&gt;.WikiDesc" (e.g. "launchpad.button.seal.WikiDesc") in the mod's locale files.
    /// Keyed by button, so every role that shares a button (like Drag) shows the same text. Empty if no key exists.
    /// </summary>
    private static string AbilityDescription(string buttonKey)
    {
        return LocaleOrEmpty(buttonKey + ".WikiDesc");
    }

    private static string TeamName(RoleBehaviour behaviour, ICustomRole custom)
    {
        if (behaviour is ICovenRole) return "Coven";

        return custom.Team switch
        {
            ModdedRoleTeams.Crewmate => "Crewmate",
            ModdedRoleTeams.Impostor => "Impostor",
            _ => "Neutral",
        };
    }

    private static Sprite? LoadSprite(Func<Sprite?> load)
    {
        try
        {
            var sprite = load();
            return sprite != null ? sprite : null;
        }
        catch
        {
            return null; // placeholder icon is shown instead
        }
    }
}
