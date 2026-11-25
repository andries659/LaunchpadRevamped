using LaunchpadRevamped.Options.Roles.Impostor;
using LaunchpadRevamped.Roles.Impostor;
using LaunchpadRevamped.Utilities;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Translation;
using Reactor.Networking.Attributes;

namespace LaunchpadRevamped.Networking.Roles;

public static class EraserRpc
{
    /// <summary>
    /// Sent when the Eraser clicks the erase button on a player row in a meeting. Runs on every client, so every client
    /// keeps the same uses and the same list of players to erase. The role change itself is done by the host after the
    /// meeting (EraserEvents).
    /// </summary>
    [MethodRpc((uint)LaunchpadRpc.Erase)]
    public static void RpcErase(this PlayerControl eraser, byte targetId)
    {
        if (eraser.Data.Role is not EraserRole)
        {
            eraser.KickForCheating();
            return;
        }

        var meeting = MeetingHud.Instance;
        if (!meeting || meeting.IsOver() || eraser.Data.IsDead)
        {
            return;
        }

        EraserRole.Used.TryGetValue(eraser.PlayerId, out var used);
        if (used >= (int)OptionGroupSingleton<EraserOptions>.Instance.EraseUses || EraserRole.UsedThisMeeting.Contains(eraser.PlayerId))
        {
            return;
        }

        var target = GameData.Instance.GetPlayerById(targetId)?.Object;
        if (!target || !target!.Data || target.Data.IsDead || target.Data.Disconnected)
        {
            return;
        }

        // Rejections do not use up an erase. The Eraser is told why, and only the Eraser.
        if (target == eraser)
        {
            Say(eraser, "launchpad.eraser.self", target);
            return;
        }

        var role = target.Data.Role;
        if (role is EraserRole || RoleTeamChecks.IsCoven(role) || RoleTeamChecks.IsNeutral(role) || EraserRole.Pending.Contains(targetId))
        {
            Say(eraser, "launchpad.eraser.cannotErase", target);
            return;
        }

        EraserRole.Used[eraser.PlayerId] = used + 1;
        EraserRole.UsedThisMeeting.Add(eraser.PlayerId);
        EraserRole.Pending.Add(targetId);
        Say(eraser, "launchpad.eraser.erasing", target);
    }

    private static void Say(PlayerControl eraser, string key, PlayerControl target)
    {
        if (eraser.AmOwner)
        {
            RpcWelcomeMessage.ShowLocal(MiraLocaleManager.GetParsed(key, new[] { ("{player}", target.Data.PlayerName) }));
        }
    }
}
