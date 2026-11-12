using LaunchpadRevamped.Roles.Coven;
using LaunchpadRevamped.Roles.Neutral;

namespace LaunchpadRevamped.Utilities;

/// <summary>
/// Which of this mod's teams a role is on. Coven roles are built on <c>CrewmateRole</c> (so the vanilla team type says
/// "Crewmate") and Neutral roles are custom, so the mod's own role interfaces are checked first.
/// </summary>
public static class RoleTeamChecks
{
    public static bool IsCoven(RoleBehaviour role) => role is ICovenRole;

    public static bool IsNeutral(RoleBehaviour role) => role is INeutralRole;

    public static bool IsImpostorTeam(RoleBehaviour role) => !IsCoven(role) && !IsNeutral(role) && role.TeamType == RoleTeamTypes.Impostor;
}
