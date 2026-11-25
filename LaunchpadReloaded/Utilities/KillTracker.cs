using System.Collections.Generic;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;

namespace LaunchpadRevamped.Utilities;

/// <summary>
/// Counts kills per player on every client (used by Stealer and Quota). Reset when a game starts.
/// </summary>
public static class KillTracker
{
    private static readonly Dictionary<byte, int> Kills = new();

    public static int Count(byte playerId) => Kills.GetValueOrDefault(playerId);

    [RegisterEvent]
    public static void OnRoundStart(RoundStartEvent @event)
    {
        if (@event.TriggeredByIntro)
        {
            Kills.Clear();
        }
    }

    [RegisterEvent]
    public static void OnAfterMurder(AfterMurderEvent @event)
    {
        if (!@event.Source || !@event.Target || @event.Source == @event.Target)
        {
            return;
        }

        Kills[@event.Source.PlayerId] = Count(@event.Source.PlayerId) + 1;
    }
}
