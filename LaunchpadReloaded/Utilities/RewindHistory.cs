using System.Collections.Generic;
using UnityEngine;

namespace LaunchpadRevamped.Utilities;

/// <summary>
/// Rolling record of the local player's recent positions, used by the Time Rewinder.
/// Every client records its own position, so a rewind can't be wrong about where "I" used to be.
/// </summary>
public static class RewindHistory
{
    private const float SampleInterval = 0.1f;

    // A little above the maximum value of the Rewind Duration option.
    private const float MaxHistorySeconds = 35f;

    private static readonly LinkedList<(float Stamp, Vector2 Position)> Samples = new();
    private static float _lastSampleTime;

    public static bool HasHistory => Samples.Count > 0;

    public static void Clear()
    {
        Samples.Clear();
    }

    public static void Sample(PlayerControl player)
    {
        // Meetings and exile cutscenes teleport everyone, so older positions are meaningless afterwards.
        if (!ShipStatus.Instance || MeetingHud.Instance || ExileController.Instance || !player.Data || player.Data.IsDead)
        {
            Clear();
            return;
        }

        if (player.inVent)
        {
            return;
        }

        var now = Time.time;
        if (now - _lastSampleTime < SampleInterval)
        {
            return;
        }

        _lastSampleTime = now;
        Samples.AddLast((now, (Vector2)player.transform.position));

        while (Samples.First != null && Samples.First.Value.Stamp < now - MaxHistorySeconds)
        {
            Samples.RemoveFirst();
        }
    }

    /// <summary>
    /// Where the player was <paramref name="secondsAgo"/> seconds ago, or as far back as is recorded.
    /// </summary>
    public static bool TryGetPosition(float secondsAgo, out Vector2 position)
    {
        position = default;
        if (Samples.Count == 0)
        {
            return false;
        }

        var target = Time.time - secondsAgo;
        var found = Samples.First!.Value;
        for (var node = Samples.First; node != null; node = node.Next)
        {
            if (node.Value.Stamp > target)
            {
                break;
            }

            found = node.Value;
        }

        position = found.Position;
        return true;
    }
}
