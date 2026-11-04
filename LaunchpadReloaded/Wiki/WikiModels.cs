using System.Collections.Generic;
using UnityEngine;

namespace LaunchpadReloaded.Wiki; // <-- change to your mod's namespace

/// <summary>One ability shown as a block on a role's page.</summary>
public class WikiAbility
{
    public string Name = "Ability";
    public string Description = "";
    public string Cooldown = "";      // e.g. "Cooldown: 25s" / "Passive". Empty = hidden.
    public Sprite Icon;               // shown on the left of the block. null = dim placeholder.
}

/// <summary>One role shown as a button in the left list.</summary>
public class WikiRole
{
    public string Name = "Role";
    public string Team = "Crewmate";  // free text, so custom teams work
    public Color Color = Color.white; // role colour (name, accent bar, ability titles)
    public string Description = "";
    public Sprite Icon;               // shown in the left list. null = dim placeholder.
    public List<WikiAbility> Abilities = new();
}

/// <summary>Register your roles here (e.g. from wherever you already define them).</summary>
public static class WikiRegistry
{
    public static readonly List<WikiRole> Roles = new();

    public static void Register(WikiRole role) => Roles.Add(role);
}
