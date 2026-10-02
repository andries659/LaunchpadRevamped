using MiraAPI.Roles;
using UnityEngine;

namespace LaunchpadRevamped.Roles.Neutral;

public interface INeutralRole : ICustomRole
{
    ModdedRoleTeams ICustomRole.Team => ModdedRoleTeams.Custom;
    RoleOptionsGroup ICustomRole.RoleOptionsGroup => new("launchpad.neutral.group", Color.gray);
    TeamIntroConfiguration? ICustomRole.IntroConfiguration => new(Color.gray, "launchpad.neutral.roleTitle", "launchpad.neutral.teamIntroDescription");
}