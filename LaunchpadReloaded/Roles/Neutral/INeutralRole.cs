using MiraAPI.Roles;
using UnityEngine;

namespace LaunchpadReloaded.Roles.Neutral;

public interface INeutralRole : ICustomRole
{
    ModdedRoleTeams ICustomRole.Team => ModdedRoleTeams.Custom;
    RoleOptionsGroup ICustomRole.RoleOptionsGroup => new("Neutral Roles", Color.gray);
    TeamIntroConfiguration? ICustomRole.IntroConfiguration => new(Color.gray, "Neutral", "You are an Neutral. You do not have a team.");
}