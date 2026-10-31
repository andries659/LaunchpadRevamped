using MiraAPI.Roles;
using UnityEngine;

namespace LaunchpadRevamped.Features;

/// <summary>
/// The role option groups used by this mod's custom teams. Shared so the role interfaces and the role category
/// buttons in the settings menu agree on them.
/// </summary>
public static class LaunchpadRoleGroups
{
    // Must match INeutralRole.
    public static RoleOptionsGroup Neutral { get; } = new("launchpad.neutral.group", Color.gray);

    public static RoleOptionsGroup Coven { get; } = new("launchpad.coven.group", LaunchpadPalette.CovenColor, 1);
}
