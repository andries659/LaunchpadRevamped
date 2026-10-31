using System.Linq;
using LaunchpadRevamped.Roles.Coven;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.Networking;

namespace LaunchpadRevamped.Events;

public static class CovenEvents
{
    [RegisterEvent]
    public static void OnRoundStart(RoundStartEvent @event)
    {
        if (@event.TriggeredByIntro)
        {
            // A new game: start with a clean slate.
            HexMasterRole.Hexed.Clear();
            return;
        }

        // A meeting just ended: every hexed player who is still alive dies. If the Hex Master died first, the hexes break.
        var local = PlayerControl.LocalPlayer;
        if (local && local.Data && !local.Data.IsDead && local.Data.Role is HexMasterRole)
        {
            foreach (var id in HexMasterRole.Hexed.ToArray())
            {
                var victim = PlayerControl.AllPlayerControls.ToArray().FirstOrDefault(player => player.PlayerId == id);
                if (victim == null || !victim.Data || victim.Data.IsDead || victim.Data.Disconnected)
                {
                    continue;
                }

                local.RpcCustomMurder(victim, teleportMurderer: false, showKillAnim: false);
            }
        }

        HexMasterRole.Hexed.Clear();
    }
}
