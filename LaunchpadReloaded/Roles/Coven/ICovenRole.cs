using LaunchpadRevamped.Features;
using MiraAPI.Roles;

namespace LaunchpadRevamped.Roles.Coven;

/// <summary>
/// Shared setup for every Coven role. The Coven is a custom team, like the neutrals, with its own group in the role settings.
/// </summary>
public interface ICovenRole : ICustomRole
{
    ModdedRoleTeams ICustomRole.Team => ModdedRoleTeams.Custom;
    RoleOptionsGroup ICustomRole.RoleOptionsGroup => LaunchpadRoleGroups.Coven;
    TeamIntroConfiguration? ICustomRole.IntroConfiguration => new(LaunchpadPalette.CovenColor, "launchpad.coven.roleTitle", "launchpad.coven.teamIntroDescription");

    // Coven members can see each other's roles.
    bool ICustomRole.CanLocalPlayerSeeRole(PlayerControl player)
    {
        var local = PlayerControl.LocalPlayer;
        return local.Data.IsDead || local.Data.Role is ICovenRole;
    }

    // The intro screen shows the whole Coven as your team.
    bool ICustomRole.SetupIntroTeam(IntroCutscene instance, ref Il2CppSystem.Collections.Generic.List<PlayerControl> yourTeam)
    {
        var team = new Il2CppSystem.Collections.Generic.List<PlayerControl>();
        team.Add(PlayerControl.LocalPlayer);

        foreach (var player in PlayerControl.AllPlayerControls.ToArray())
        {
            if (player && player != PlayerControl.LocalPlayer && player.Data && player.Data.Role is ICovenRole)
            {
                team.Add(player);
            }
        }

        yourTeam = team;
        return true;
    }
}
