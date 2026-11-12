using System.Collections.Generic;
using MiraAPI.Roles;
using MiraAPI.Translation;
using UnityEngine;

namespace LaunchpadRevamped.Features;

/// <summary>
/// The role option groups used by this mod's teams and factions. Shared so the role classes, the role interfaces and the
/// role category buttons in the settings menu agree on them.
///
/// MiraAPI shows one header per <see cref="RoleOptionsGroup"/> in the role settings, so every faction (the sub-categories of
/// a team, like "Crewmate Killing") is its own group. That is what makes the factions show up in the role settings
/// whether or not the role category buttons are enabled. A role picks its faction by overriding
/// <c>ICustomRole.RoleOptionsGroup</c> with one of the groups below. Lower priority is listed first.
/// </summary>
public static class LaunchpadRoleGroups
{
    /// <summary>Name of the Coven's folder in the freeplay file browser (next to Impostor, Crewmate and Neutral).</summary>
    public const string CovenFreeplayFolder = "Coven";

    // Team-level fallbacks. Must match INeutralRole / ICovenRole. A role that doesn't pick a faction lands here.
    public static RoleOptionsGroup Neutral { get; } = new("launchpad.neutral.group", Color.gray);

    public static RoleOptionsGroup Coven { get; } = new("launchpad.coven.group", LaunchpadPalette.CovenColor, 1);

    // Crewmate factions
    public static RoleOptionsGroup CrewmateInvestigative { get; } = new("launchpad.faction.crewmate.investigative", Palette.CrewmateRoleHeaderBlue, -20);

    public static RoleOptionsGroup CrewmateKilling { get; } = new("launchpad.faction.crewmate.killing", Palette.CrewmateRoleHeaderBlue, -19);

    public static RoleOptionsGroup CrewmateProtective { get; } = new("launchpad.faction.crewmate.protective", Palette.CrewmateRoleHeaderBlue, -18);

    public static RoleOptionsGroup CrewmateSupport { get; } = new("launchpad.faction.crewmate.support", Palette.CrewmateRoleHeaderBlue, -17);

    // Impostor factions
    public static RoleOptionsGroup ImpostorConcealing { get; } = new("launchpad.faction.impostor.concealing", Palette.ImpostorRoleHeaderRed, -16);

    public static RoleOptionsGroup ImpostorKilling { get; } = new("launchpad.faction.impostor.killing", Palette.ImpostorRoleHeaderRed, -15);

    public static RoleOptionsGroup ImpostorSupport { get; } = new("launchpad.faction.impostor.support", Palette.ImpostorRoleHeaderRed, -14);

    // Neutral factions
    public static RoleOptionsGroup NeutralBenign { get; } = new("launchpad.faction.neutral.benign", Color.gray, 0);

    public static RoleOptionsGroup NeutralEvil { get; } = new("launchpad.faction.neutral.evil", Color.gray, 1);

    public static RoleOptionsGroup NeutralKilling { get; } = new("launchpad.faction.neutral.killing", Color.gray, 2);

    // Coven factions
    public static RoleOptionsGroup CovenPower { get; } = new("launchpad.faction.coven.power", LaunchpadPalette.CovenColor, 3);

    public static RoleOptionsGroup CovenKilling { get; } = new("launchpad.faction.coven.killing", LaunchpadPalette.CovenColor, 4);

    public static RoleOptionsGroup CovenSupport { get; } = new("launchpad.faction.coven.support", LaunchpadPalette.CovenColor, 5);

    /// <summary>Every group that belongs to the Crewmate team (the factions plus MiraAPI's default Crewmate group).</summary>
    public static IReadOnlyList<RoleOptionsGroup> CrewmateGroups { get; } = new[]
    {
        RoleOptionsGroup.Crewmate, CrewmateInvestigative, CrewmateKilling, CrewmateProtective, CrewmateSupport,
    };

    /// <summary>Every group that belongs to the Impostor team.</summary>
    public static IReadOnlyList<RoleOptionsGroup> ImpostorGroups { get; } = new[]
    {
        RoleOptionsGroup.Impostor, ImpostorConcealing, ImpostorKilling, ImpostorSupport,
    };

    /// <summary>Every group that belongs to the Neutral team.</summary>
    public static IReadOnlyList<RoleOptionsGroup> NeutralGroups { get; } = new[]
    {
        Neutral, NeutralBenign, NeutralEvil, NeutralKilling,
    };

    /// <summary>Every group that belongs to the Coven team.</summary>
    public static IReadOnlyList<RoleOptionsGroup> CovenGroups { get; } = new[]
    {
        Coven, CovenPower, CovenKilling, CovenSupport,
    };

    private static readonly RoleOptionsGroup[] AllFactions =
    {
        CrewmateInvestigative, CrewmateKilling, CrewmateProtective, CrewmateSupport,
        ImpostorConcealing, ImpostorKilling, ImpostorSupport,
        NeutralBenign, NeutralEvil, NeutralKilling,
        CovenPower, CovenKilling, CovenSupport,
    };

    /// <summary>
    /// Gets the display name of a faction group (for example "Crewmate Killing"). Returns false for groups that aren't factions.
    /// </summary>
    public static bool TryGetFactionName(RoleOptionsGroup group, out string name)
    {
        foreach (var faction in AllFactions)
        {
            if (faction == group)
            {
                name = MiraLocaleManager.Get(faction.Name);
                return true;
            }
        }

        name = string.Empty;
        return false;
    }
}
