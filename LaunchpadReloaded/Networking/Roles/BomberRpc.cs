using System.Collections;
using LaunchpadRevamped.Options.Roles.Impostor;
using LaunchpadRevamped.Roles.Impostor;
using MiraAPI.GameOptions;
using MiraAPI.Networking;
using Reactor.Networking.Attributes;
using Reactor.Utilities;
using UnityEngine;

namespace LaunchpadRevamped.Networking.Roles;

public static class BomberRpc
{
    private static Sprite? _circleSprite;

    [MethodRpc((uint)LaunchpadRpc.PlaceBomb)]
    public static void RpcPlaceBomb(this PlayerControl bomber, float x, float y)
    {
        if (bomber.Data.Role is not BomberRole)
        {
            bomber.KickForCheating();
            return;
        }

        Coroutines.Start(CoBomb(bomber, new Vector2(x, y)));
    }

    private static IEnumerator CoBomb(PlayerControl bomber, Vector2 position)
    {
        var options = OptionGroupSingleton<BomberOptions>.Instance;
        var fuse = options.BombFuse;
        var radius = options.BombRadius;
        var killImpostors = options.BombKillsImpostors;

        var worldPosition = new Vector3(position.x, position.y, position.y / 1000f);
        var bomb = CreateDisc("LaunchpadBomb", worldPosition, UnityEngine.Color.black, 0.4f, 5);
        var bombRenderer = bomb.GetComponent<SpriteRenderer>();

        // Fuse: blink faster and faster. Bombs disappear if a meeting starts or the game ends.
        var elapsed = 0f;
        while (elapsed < fuse)
        {
            if (!bomb)
            {
                yield break;
            }

            if (MeetingHud.Instance || !ShipStatus.Instance)
            {
                Object.Destroy(bomb);
                yield break;
            }

            elapsed += Time.deltaTime;
            var blinkSpeed = Mathf.Lerp(6f, 30f, elapsed / fuse);
            bombRenderer.color = Mathf.Sin(elapsed * blinkSpeed) > 0f ? UnityEngine.Color.red : new UnityEngine.Color(0.15f, 0.15f, 0.15f, 1f);
            yield return null;
        }

        Object.Destroy(bomb);

        // The bomber's client does the killing (same approach as the Hitman and Surgeon).
        // A bomb whose owner died before detonation is a dud.
        if (bomber.AmOwner && !bomber.Data.IsDead)
        {
            KillVictims(bomber, position, radius, killImpostors);
        }

        // Blast visual
        var blast = CreateDisc("LaunchpadBombBlast", worldPosition, new UnityEngine.Color(1f, 0.55f, 0f, 0.75f), 0.4f, 6);
        var blastRenderer = blast.GetComponent<SpriteRenderer>();
        const float blastTime = 0.3f;
        var t = 0f;
        while (t < blastTime)
        {
            if (!blast)
            {
                yield break;
            }

            t += Time.deltaTime;
            var progress = t / blastTime;
            var size = Mathf.Lerp(0.4f, radius * 2f, progress);
            blast.transform.localScale = new Vector3(size, size, 1f);
            blastRenderer.color = new UnityEngine.Color(1f, 0.55f, 0f, Mathf.Lerp(0.75f, 0f, progress));
            yield return null;
        }

        Object.Destroy(blast);

        var local = PlayerControl.LocalPlayer;
        if (local && HudManager.InstanceExists && Vector2.Distance(local.GetTruePosition(), position) <= radius * 2f)
        {
            yield return HudManager.Instance.PlayerCam.CoShakeScreen(0.3f, 3f);
        }
    }

    private static void KillVictims(PlayerControl bomber, Vector2 position, float radius, bool killImpostors)
    {
        foreach (var player in PlayerControl.AllPlayerControls.ToArray())
        {
            if (player == bomber || player.Data.IsDead || player.Data.Disconnected || player.inVent)
            {
                continue;
            }

            if (player.Data.Role.IsImpostor && !killImpostors)
            {
                continue;
            }

            if (Vector2.Distance(player.GetTruePosition(), position) > radius)
            {
                continue;
            }

            bomber.RpcCustomMurder(player, resetKillTimer: false, createDeadBody: true, teleportMurderer: false, showKillAnim: false, playKillSound: true);
        }
    }

    // No bomb art in the asset bundle yet, so draw simple discs at runtime.
    private static GameObject CreateDisc(string name, Vector3 position, UnityEngine.Color color, float diameter, int sortingOrder)
    {
        var disc = new GameObject(name);
        disc.transform.position = position;
        disc.transform.localScale = new Vector3(diameter, diameter, 1f);

        var renderer = disc.AddComponent<SpriteRenderer>();
        renderer.sprite = GetCircleSprite();
        renderer.color = color;
        renderer.sortingOrder = sortingOrder;

        var shader = Shader.Find("Sprites/Default");
        if (shader)
        {
            renderer.material = new Material(shader);
        }

        return disc;
    }

    private static Sprite GetCircleSprite()
    {
        if (_circleSprite)
        {
            return _circleSprite!;
        }

        const int size = 64;
        var texture = new Texture2D(size, size, TextureFormat.RGBA32, false)
        {
            hideFlags = HideFlags.HideAndDontSave,
            filterMode = FilterMode.Bilinear,
        };

        var center = (size - 1) / 2f;
        for (var y = 0; y < size; y++)
        {
            for (var x = 0; x < size; x++)
            {
                var distance = Vector2.Distance(new Vector2(x, y), new Vector2(center, center)) / (size / 2f);
                var alpha = Mathf.Clamp01((1f - distance) * 8f);
                texture.SetPixel(x, y, new UnityEngine.Color(1f, 1f, 1f, alpha));
            }
        }

        texture.Apply();

        // 64 pixels per unit makes the sprite exactly 1 world unit wide, so scale == diameter.
        _circleSprite = Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
        _circleSprite.hideFlags = HideFlags.HideAndDontSave;
        return _circleSprite;
    }
}
