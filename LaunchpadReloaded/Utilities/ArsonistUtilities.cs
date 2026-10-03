using System.Linq;
using LaunchpadRevamped.Modifiers;
using MiraAPI.Modifiers;

namespace LaunchpadRevamped.Utilities;

public static class ArsonistUtilities
{
    /// <summary>
    /// All living, connected players that are currently doused.
    /// </summary>
    public static PlayerControl[] GetDousedPlayers()
    {
        return PlayerControl.AllPlayerControls.ToArray()
            .Where(player => player && player.Data && !player.Data.IsDead && !player.Data.Disconnected && player.HasModifier<DousedModifier>())
            .ToArray();
    }
}
