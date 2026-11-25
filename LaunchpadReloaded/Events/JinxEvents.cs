using LaunchpadRevamped.Networking.Roles;
using LaunchpadRevamped.Options.Roles.Coven;
using LaunchpadRevamped.Roles.Coven;
using LaunchpadRevamped.Utilities;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.GameOptions;
using MiraAPI.Networking;

namespace LaunchpadRevamped.Events;

/// <summary>
/// Ported from TOHO (Jinx). The host cancels an attack on a jinxed player and kills the attacker instead.
/// The jinx is used up. If the Jinx is dead, the jinx does nothing (it stays on the player).
/// </summary>
public static class JinxEvents
{
    [RegisterEvent]
    public static void OnBeforeMurder(BeforeMurderEvent @event)
    {
        if (!AmongUsClient.Instance.AmHost || @event.IsCancelled || @event.IgnoreDefense)
        {
            return;
        }

        var attacker = @event.Source;
        var target = @event.Target;
        if (!attacker || !target || !target.Data || target.Data.IsDead
            || !JinxRole.Jinxed.TryGetValue(target.PlayerId, out var jinxId))
        {
            return;
        }

        var jinx = AbilityUtils.GetPlayer(jinxId);
        if (!jinx || !jinx!.Data || jinx.Data.IsDead || jinx == target || jinx == attacker)
        {
            return;
        }

        if (CovenUtils.IsCoven(attacker) && !OptionGroupSingleton<JinxOptions>.Instance.CovenCanDieToJinx)
        {
            return;
        }

        @event.Cancel();
        jinx.RpcJinx(target.PlayerId, false);
        jinx.RpcCustomMurder(attacker, resetKillTimer: false, teleportMurderer: false, showKillAnim: false);
    }
}
