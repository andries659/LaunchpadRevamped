using AmongUs.GameOptions;
using LaunchpadRevamped.Networking;
using LaunchpadRevamped.Roles.Impostor;
using LaunchpadRevamped.Utilities;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.Events.Vanilla.Meeting;
using MiraAPI.Translation;
using System.Linq;

namespace LaunchpadRevamped.Events;

/// <summary>
/// Ported from TOHO (Eraser). The erase itself is requested with the erase button in the meeting (EraserMeetingPatches
/// and EraserRpc). After the meeting the erased players become a plain Crewmate or Impostor.
/// </summary>
public static class EraserEvents
{
    [RegisterEvent]
    public static void OnMeetingStart(StartMeetingEvent @event)
    {
        EraserRole.UsedThisMeeting.Clear();
    }

    [RegisterEvent]
    public static void OnRoundStart(RoundStartEvent @event)
    {
        if (@event.TriggeredByIntro)
        {
            EraserRole.Used.Clear();
            EraserRole.Pending.Clear();
            EraserRole.UsedThisMeeting.Clear();
            return;
        }

        var local = PlayerControl.LocalPlayer;

        foreach (var id in EraserRole.Pending.ToArray())
        {
            var player = AbilityUtils.GetPlayer(id);
            if (!player || !player!.Data || player.Data.IsDead || player.Data.Disconnected)
            {
                continue;
            }

            var role = player.Data.Role;

            if (player == local)
            {
                RpcWelcomeMessage.ShowLocal(MiraLocaleManager.Get("launchpad.eraser.erased"));
            }

            // Only the host changes roles. Nothing to do for players that are already a plain Crewmate/Impostor.
            if (!AmongUsClient.Instance.AmHost || role.Role is RoleTypes.Crewmate or RoleTypes.Impostor)
            {
                continue;
            }

            player.RpcSetRole(RoleTeamChecks.IsImpostorTeam(role) ? RoleTypes.Impostor : RoleTypes.Crewmate, true);
        }

        EraserRole.Pending.Clear();
    }
}
