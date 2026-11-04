using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace LaunchpadRevamped.Features;

public sealed class EsperProjectionData
{
    public Vector2 BodyPosition { get; init; }
    public DeadBody? Body { get; init; }
}

/// <summary>
/// Shared Esper state. Begin/End are driven by RPCs, so every client holds the same dictionary.
/// </summary>
public static class EsperProjection
{
    public static readonly Dictionary<byte, EsperProjectionData> Active = new();

    public static bool IsProjected(PlayerControl? player) => player && Active.ContainsKey(player!.PlayerId);

    public static void Begin(PlayerControl player, Vector2 bodyPosition)
    {
        if (Active.ContainsKey(player.PlayerId))
        {
            End(player, snapToBody: false);
        }

        // The corpse is built the same way MiraAPI's CoPerformCustomKill builds one.
        var body = Object.Instantiate(GameManager.Instance.GetDeadBody(player.Data.Role));
        body.ParentId = player.PlayerId;
        foreach (var renderer in body.bodyRenderers)
        {
            player.SetPlayerMaterialColors(renderer);
        }

        player.SetPlayerMaterialColors(body.bloodSplatter);

        var position = (Vector3)bodyPosition;
        position.z = position.y / 1000f;
        body.transform.position = position;
        body.enabled = true;

        Active[player.PlayerId] = new EsperProjectionData { BodyPosition = bodyPosition, Body = body };

        if (player.AmOwner)
        {
            player.Collider.enabled = false; // walk through walls
            if (HudManager.InstanceExists)
            {
                HudManager.Instance.FullScreen.gameObject.SetActive(true);
                HudManager.Instance.FullScreen.color = new Color(0.35f, 0.55f, 1f, 0.15f);
            }
        }

        SetVisual(player, true);
    }

    public static void End(PlayerControl player, bool snapToBody)
    {
        if (!Active.Remove(player.PlayerId, out var data))
        {
            return;
        }

        if (data.Body)
        {
            Object.Destroy(data.Body!.gameObject);
        }

        player.Collider.enabled = true;
        SetVisual(player, false);

        if (player.AmOwner)
        {
            if (HudManager.InstanceExists)
            {
                HudManager.Instance.FullScreen.gameObject.SetActive(false);
            }

            if (snapToBody)
            {
                player.NetTransform.RpcSnapTo(data.BodyPosition);
            }
        }
    }

    /// <summary>
    /// A meeting is starting: every projected Esper dies. Runs on every client (see EsperPatches), so the result is the
    /// same everywhere. Dies like a normal kill so ghost roles still get assigned by the host.
    /// </summary>
    public static void KillAllProjected()
    {
        foreach (var id in Active.Keys.ToList())
        {
            var player = GameData.Instance.GetPlayerById(id)?.Object;
            if (!player)
            {
                Active.Remove(id);
                continue;
            }

            End(player!, snapToBody: false);
            if (!player!.Data.IsDead)
            {
                player.Die(DeathReason.Kill, true);
            }
        }
    }

    /// <summary>
    /// Hook for roles that can attack the corpse (e.g. a Sheriff shot). Must run on every client, so call it from an RPC.
    /// </summary>
    public static void KillBody(PlayerControl esper)
    {
        if (!IsProjected(esper))
        {
            return;
        }

        End(esper, snapToBody: false);
        esper.Die(DeathReason.Kill, true);
    }

    public static void Reset()
    {
        foreach (var data in Active.Values)
        {
            if (data.Body)
            {
                Object.Destroy(data.Body!.gameObject);
            }
        }

        Active.Clear();
    }

    // Invisible to living players, translucent for the Esper itself and for dead players.
    private static void SetVisual(PlayerControl player, bool projected)
    {
        var seen = player.AmOwner || PlayerControl.LocalPlayer.Data.IsDead;
        var alpha = !projected ? 1f : seen ? 0.5f : 0f;

        player.cosmetics.SetPhantomRoleAlpha(alpha);
        player.cosmetics.ToggleNameVisible(!projected || seen);
    }
}
