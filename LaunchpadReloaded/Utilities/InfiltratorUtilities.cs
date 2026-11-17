using LaunchpadRevamped.Roles.Crewmate;
using System.Linq;

namespace LaunchpadRevamped.Utilities;

public static class InfiltratorUtilities
{
    public static bool IsInfiltrator(this PlayerControl? player)
    {
        return player && player!.Data && player.Data.Role is InfiltratorRole;
    }

    /// <summary>True if any Infiltrator is in this game, alive or dead (so a death doesn't reveal who it was).</summary>
    public static bool InfiltratorInGame()
    {
        return PlayerControl.AllPlayerControls.ToArray().Any(p => p.IsInfiltrator());
    }

    public static bool LocalIsImpostor()
    {
        var local = PlayerControl.LocalPlayer;
        return local && local.Data && local.Data.Role && local.Data.Role.IsImpostor;
    }
}
