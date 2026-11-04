using LaunchpadRevamped.Features;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.Events.Vanilla.Meeting;
using MiraAPI.Events.Vanilla.Player;

namespace LaunchpadRevamped.Events;

public static class EsperEvents
{
    // The projection is a ghost: normal kills can't touch it.
    [RegisterEvent]
    public static void BeforeMurderEvent(BeforeMurderEvent @event)
    {
        if (EsperProjection.IsProjected(@event.Target))
        {
            @event.Cancel();
        }
    }

    // A projected Esper can't report bodies or call emergency meetings.
    [RegisterEvent]
    public static void ReportBodyEvent(ReportBodyEvent @event)
    {
        if (EsperProjection.IsProjected(@event.Reporter))
        {
            @event.Cancel();
        }
    }

    // Died some other way while projected: drop the fake body and restore visuals.
    [RegisterEvent]
    public static void PlayerDeathEvent(PlayerDeathEvent @event)
    {
        if (EsperProjection.IsProjected(@event.Player))
        {
            EsperProjection.End(@event.Player, snapToBody: false);
        }
    }

    // New game: forget anything left over from the last one (e.g. a game that ended mid-projection).
    [RegisterEvent]
    public static void IntroBeginEvent(IntroBeginEvent @event)
    {
        EsperProjection.Reset();
    }
}
