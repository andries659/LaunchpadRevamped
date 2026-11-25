using System.Linq;
using LaunchpadRevamped.Options.Roles.Impostor;
using LaunchpadRevamped.Roles.Impostor;
using LaunchpadRevamped.Utilities;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.GameOptions;
using MiraAPI.Networking;

namespace LaunchpadRevamped.Events;

public static class VampireEvents
{
    [RegisterEvent]
    public static void OnRoundStart(RoundStartEvent @event)
    {
        if (@event.TriggeredByIntro)
        {
            VampireRole.Bitten.Clear();
            return;
        }

        // A meeting just ended. With "Bitten Players Die After Meeting" on, everyone still bitten dies now
        // (TOHO kills them the moment a meeting is called). Otherwise the bite timers simply carry on.
        var local = PlayerControl.LocalPlayer;
        if (!local || !local.Data || local.Data.IsDead || local.Data.Role is not VampireRole
            || !OptionGroupSingleton<VampireOptions>.Instance.DieAfterMeeting)
        {
            return;
        }

        foreach (var id in VampireRole.Bitten.ToArray())
        {
            var victim = AbilityUtils.GetPlayer(id);
            if (victim && victim!.Data && !victim.Data.IsDead && !victim.Data.Disconnected)
            {
                local.RpcCustomMurder(victim, teleportMurderer: false, showKillAnim: false);
            }
        }

        VampireRole.Bitten.Clear();
    }
}
