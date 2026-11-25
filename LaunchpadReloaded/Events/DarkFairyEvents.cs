using LaunchpadRevamped.Features;
using LaunchpadRevamped.Modifiers;
using LaunchpadRevamped.Networking.Roles;
using LaunchpadRevamped.Options.Roles.Coven;
using LaunchpadRevamped.Roles.Coven;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.Events.Vanilla.Player;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Translation;

namespace LaunchpadRevamped.Events;

public static class DarkFairyEvents
{
    [RegisterEvent]
    public static void OnRoundStart(RoundStartEvent @event)
    {
        // Marks last until the next meeting ends, and a new game starts clean.
        DarkFairyRole.Marked.Clear();
        DarkFairyRole.MarkedThisRound = 0;
    }

    [RegisterEvent]
    public static void OnTaskCompleted(CompleteTaskEvent @event)
    {
        var player = @event.Player;
        if (!player || !player.AmOwner || !player.Data || player.Data.IsDead || @event.Task == null)
        {
            return;
        }

        var index = @event.Task.Index;

        if (player.Data.Role is DarkFairyRole)
        {
            if (DarkFairyRole.MarkedThisRound >= (int)OptionGroupSingleton<DarkFairyOptions>.Instance.MarkedTasksPerRound
                || DarkFairyRole.Marked.Contains(index))
            {
                return;
            }

            DarkFairyRole.MarkedThisRound++;
            player.RpcDarkFairyMark(index, true);
            MiraAPI.Utilities.Helpers.CreateAndShowNotification(
                MiraLocaleManager.Get("launchpad.darkFairy.marked"), LaunchpadPalette.DarkFairyColor);
            return;
        }

        if (DarkFairyRole.Marked.Contains(index) && DarkFairyRole.CanBeDarkened(player))
        {
            player.RpcAddModifier<DarkenedModifier>();
            player.RpcDarkFairyMark(index, false);
        }
    }
}
