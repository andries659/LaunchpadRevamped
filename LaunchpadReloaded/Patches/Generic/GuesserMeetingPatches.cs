using System.Collections.Generic;
using AmongUs.GameOptions;
using HarmonyLib;
using LaunchpadRevamped.Components;
using LaunchpadRevamped.Features;
using LaunchpadRevamped.Modifiers.Game.Universal;
using LaunchpadRevamped.Networking;
using LaunchpadRevamped.Options.Modifiers.Universal;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace LaunchpadRevamped.Patches.Generic;

/// <summary>
/// Meeting UI of the Guesser: a crosshair button (<see cref="LaunchpadAssets.GuesserAbility"/>) over the avatar of every
/// player row. Clicking it opens the mod's own <see cref="GuessRoleMinigame"/> (the role picker the Gambler uses).
///
/// Layering rules this file follows, so nothing can end up on top of the picker:
///  - the button is a clone of the row's own CancelButton, so it uses the row's material and sorting, and it is raised
///    above every other renderer of its row, which puts it over the player icon;
///  - no custom text or sprite objects with hand-picked sorting orders are created;
///  - while the picker is open every renderer of every player row (the buttons included) is switched off, and switched
///    back on once the picker is gone.
/// </summary>
[HarmonyPatch(typeof(MeetingHud))]
public static class GuesserMeetingPatches
{
    // Size of the crosshair in row units (the longest side of the sprite is scaled to this), and its hover tint.
    private const float IconSize = 0.42f;
    private static readonly Color HoverColor = new(1f, 0.35f, 0.35f);

    // Used only when a row has no PlayerIcon to take the position from (same default as TOU-Mira's meeting buttons).
    private static readonly Vector3 FallbackPosition = new(-0.95f, 0.03f, -3f);

    private static readonly Dictionary<byte, GameObject> Buttons = new();
    private static readonly Dictionary<Renderer, bool> HiddenRenderers = new();

    private static GuessRoleMinigame? _minigame;

    [HarmonyPostfix]
    [HarmonyPatch(nameof(MeetingHud.Start))]
    public static void StartPostfix(MeetingHud __instance)
    {
        // Anything left over from the previous meeting (the picker lives under the HUD, not under the meeting) must go.
        ClosePanel();
        RestoreRows();
        Buttons.Clear();

        var local = PlayerControl.LocalPlayer;
        if (!local || !local.HasModifier<GuesserModifier>())
        {
            return;
        }

        var sprite = LaunchpadAssets.GuesserAbility.LoadAsset();
        if (!sprite)
        {
            UnityEngine.Debug.LogWarning("[Guesser] The GuesserAbility sprite could not be loaded, no guess buttons were created.");
            return;
        }

        foreach (var area in __instance.playerStates)
        {
            var targetId = area.TargetId();
            if (targetId == local.PlayerId)
            {
                continue;
            }

            var button = CreateGuessButton(__instance, area, targetId, sprite);
            if (button)
            {
                Buttons[targetId] = button!;
            }
        }
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(MeetingHud.Update))]
    public static void UpdatePostfix(MeetingHud __instance)
    {
        if (Buttons.Count == 0)
        {
            return;
        }

        // The picker is gone (closed with the X, by clicking outside, or after a guess): bring the rows back.
        if (HiddenRenderers.Count > 0 && !_minigame)
        {
            RestoreRows();
        }

        var modifier = PlayerControl.LocalPlayer.GetModifier<GuesserModifier>();
        var over = __instance.IsOver();
        var can = modifier != null && modifier.CanGuessNow && !over;

        if (!can)
        {
            ClosePanel();
        }

        foreach (var (targetId, button) in Buttons)
        {
            if (!button)
            {
                continue;
            }

            var target = GameData.Instance.GetPlayerById(targetId);
            button.SetActive(can && target != null && !target.IsDead && !target.Disconnected);
        }
    }

    /// <summary>
    /// Clones the row's own cancel-vote button and turns it into the guess button: only the crosshair sprite is left,
    /// it sits on the player's avatar and it is raised above everything else in the row.
    /// </summary>
    private static GameObject? CreateGuessButton(MeetingHud meeting, PlayerVoteArea area, byte targetId, Sprite sprite)
    {
        var template = area.Buttons ? area.Buttons.transform.Find("CancelButton") : null;
        if (!template)
        {
            UnityEngine.Debug.LogWarning("[Guesser] CancelButton template not found on a player row, guess button skipped.");
            return null;
        }

        var go = Object.Instantiate(template!.gameObject, area.transform);
        go.name = "GuessButton";
        go.SetActive(true);

        // Drop everything the cancel button brought along (controller highlight, ...): the icon is the whole button.
        for (var i = go.transform.childCount - 1; i >= 0; i--)
        {
            Object.Destroy(go.transform.GetChild(i).gameObject);
        }

        // Centre it on the avatar of this row.
        var position = FallbackPosition;
        if (area.PlayerIcon)
        {
            position = area.transform.InverseTransformPoint(area.PlayerIcon.transform.position);
            position.z = FallbackPosition.z;
        }

        go.transform.localRotation = Quaternion.identity;
        go.transform.localPosition = position;

        var renderer = go.GetComponent<SpriteRenderer>();
        renderer.sprite = sprite;
        renderer.color = Color.white;

        // Scale by the sprite's real size, so the icon looks the same whatever pixels-per-unit it was imported with.
        var spriteSize = sprite.bounds.size;
        go.transform.localScale = Vector3.one * (IconSize / Mathf.Max(spriteSize.x, spriteSize.y, 0.01f));

        var collider = go.GetComponent<BoxCollider2D>();
        if (!collider)
        {
            collider = go.AddComponent<BoxCollider2D>();
        }

        collider.size = spriteSize;
        collider.offset = sprite.bounds.center;

        var button = go.GetComponent<PassiveButton>();
        if (!button)
        {
            button = go.AddComponent<PassiveButton>();
        }

        button.Colliders = new Collider2D[] { collider };
        button.OnClick = new Button.ButtonClickedEvent();
        button.OnMouseOver = new UnityEvent();
        button.OnMouseOut = new UnityEvent();
        button.OnClick.AddListener((UnityAction)(() => OpenPanel(meeting, targetId)));
        button.OnMouseOver.AddListener((UnityAction)(() => renderer.color = HoverColor));
        button.OnMouseOut.AddListener((UnityAction)(() => renderer.color = Color.white));

        RaiseAboveRow(area, renderer);
        return go;
    }

    /// <summary>
    /// Puts <paramref name="icon"/> in front of every other renderer of the row (avatar, hat, name plate, ...), so the
    /// crosshair can never end up behind the player icon.
    /// </summary>
    private static void RaiseAboveRow(PlayerVoteArea area, Renderer icon)
    {
        var order = icon.sortingOrder;

        foreach (var other in area.GetComponentsInChildren<Renderer>(true))
        {
            if (!other || other == icon || other.sortingLayerID != icon.sortingLayerID)
            {
                continue;
            }

            order = Mathf.Max(order, other.sortingOrder);
        }

        icon.sortingOrder = order + 1;
    }

    private static void OpenPanel(MeetingHud meeting, byte targetId)
    {
        // Never stack on top of another minigame (this also stops a second click opening a second picker).
        if (Minigame.Instance)
        {
            return;
        }

        var target = GameData.Instance.GetPlayerById(targetId);
        if (target == null)
        {
            return;
        }

        var canGuessBasic = OptionGroupSingleton<GuesserOptions>.Instance.CanGuessBasicRoles;

        var minigame = GuessRoleMinigame.Create();
        _minigame = minigame;

        // Switch the rows off before the picker appears: nothing in them may draw over it.
        HideRows(meeting);

        minigame.Open(
            role => role && !role.IsDead && (canGuessBasic || role.Role is not (RoleTypes.Crewmate or RoleTypes.Impostor)),
            selectedRole =>
            {
                // The picker reports null when it was dismissed (X button or a click outside of it).
                if (selectedRole == null)
                {
                    return;
                }

                PlayerControl.LocalPlayer.RpcGuess(targetId, (ushort)selectedRole.Role);
                minigame.Close();
            });
    }

    private static void ClosePanel()
    {
        if (_minigame)
        {
            _minigame!.Close();
        }

        _minigame = null;
    }

    /// <summary>Switches off every renderer of every player row (the guess buttons included) and remembers what was on.</summary>
    private static void HideRows(MeetingHud meeting)
    {
        foreach (var area in meeting.playerStates)
        {
            if (!area)
            {
                continue;
            }

            foreach (var renderer in area.GetComponentsInChildren<Renderer>(true))
            {
                if (!renderer || HiddenRenderers.ContainsKey(renderer))
                {
                    continue;
                }

                HiddenRenderers[renderer] = renderer.enabled;
                renderer.enabled = false;
            }
        }
    }

    /// <summary>Switches back on what <see cref="HideRows"/> switched off (leaving alone anything the game changed meanwhile).</summary>
    private static void RestoreRows()
    {
        foreach (var (renderer, wasEnabled) in HiddenRenderers)
        {
            if (renderer && !renderer.enabled)
            {
                renderer.enabled = wasEnabled;
            }
        }

        HiddenRenderers.Clear();
    }
}
