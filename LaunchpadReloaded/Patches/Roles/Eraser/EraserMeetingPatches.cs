using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using LaunchpadRevamped.Features;
using LaunchpadRevamped.Networking;
using LaunchpadRevamped.Networking.Roles;
using LaunchpadRevamped.Options.Roles.Impostor;
using LaunchpadRevamped.Roles.Impostor;
using MiraAPI.GameOptions;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace LaunchpadRevamped.Patches.Roles.Eraser;

/// <summary>
/// Meeting UI of the Eraser: a third button next to the green confirm and red cancel buttons of a player row. It is
/// cloned from the confirm button, lives in the same button group (so it appears and disappears together with them
/// when a player is selected) and sits to the left of the confirm button.
///
/// The button is available for every player, even ones that cannot be erased, so it never gives away who is a
/// Neutral or Coven.
/// </summary>
[HarmonyPatch(typeof(MeetingHud))]
public static class EraserMeetingPatches
{
    // Tint of the button's box, so it can't be mistaken for the green confirm button. Tweak to taste.
    private static readonly Color BoxTint = new(0.65f, 0.55f, 1f);

    // How much of the box the icon fills (0.7 = 70%). Tweak to taste.
    private const float IconFill = 0.7f;

    // Used when the row has no cancel button to measure the button spacing from.
    private static readonly Vector3 FallbackSpacing = new(0.6f, 0f, 0f);

    private static readonly Dictionary<byte, GameObject> Buttons = new();

    [HarmonyPostfix]
    [HarmonyPatch(nameof(MeetingHud.Start))]
    public static void StartPostfix(MeetingHud __instance)
    {
        Buttons.Clear();

        var local = PlayerControl.LocalPlayer;
        if (!local || !local.Data || local.Data.IsDead || local.Data.Role is not EraserRole)
        {
            return;
        }

        var sprite = LaunchpadAssets.EraseButton.LoadAsset();
        if (!sprite)
        {
            UnityEngine.Debug.LogWarning("[Eraser] The erase sprite could not be loaded, no erase buttons were created.");
            return;
        }

        foreach (var area in __instance.playerStates)
        {
            var targetId = area.TargetId();
            if (targetId == local.PlayerId)
            {
                continue;
            }

            var button = CreateButton(area, targetId, sprite);
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

        var local = PlayerControl.LocalPlayer;
        EraserRole.Used.TryGetValue(local.PlayerId, out var used);

        var can = local && local.Data && !local.Data.IsDead && local.Data.Role is EraserRole && !__instance.IsOver()
                  && used < (int)OptionGroupSingleton<EraserOptions>.Instance.EraseUses
                  && !EraserRole.UsedThisMeeting.Contains(local.PlayerId);

        // Whether the button is actually on screen is up to its parent group: it only shows while the row's
        // confirm / cancel buttons do.
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

    private static GameObject? CreateButton(PlayerVoteArea area, byte targetId, Sprite sprite)
    {
        if (!area.Buttons)
        {
            return null;
        }

        var group = area.Buttons.transform;
        var confirm = group.Find("ConfirmButton");
        var cancel = group.Find("CancelButton");
        if (!confirm)
        {
            UnityEngine.Debug.LogWarning("[Eraser] ConfirmButton template not found on a player row, erase button skipped.");
            return null;
        }

        var go = Object.Instantiate(confirm!.gameObject, group);
        go.name = "EraseButton";

        // One button-width to the left of the confirm button (same spacing as between confirm and cancel).
        var spacing = cancel ? cancel!.localPosition - confirm.localPosition : FallbackSpacing;
        spacing.y = 0f;
        spacing.z = 0f;
        go.transform.localPosition = confirm.localPosition - spacing;
        go.transform.localRotation = confirm.localRotation;
        go.transform.localScale = confirm.localScale;

        // The box keeps its look (tinted). Every other renderer on the clone (the check mark) is hidden and
        // replaced by our own icon child, so LaunchpadAssets.EraseButton is always what gets drawn.
        var root = go.GetComponent<SpriteRenderer>();
        if (root)
        {
            root.color = BoxTint;
        }

        foreach (var renderer in go.GetComponentsInChildren<SpriteRenderer>(true))
        {
            if (renderer && renderer.gameObject != go)
            {
                renderer.enabled = false;
            }
        }

        var iconObject = new GameObject("EraseIcon");
        iconObject.transform.SetParent(go.transform, false);
        iconObject.transform.localPosition = new Vector3(0f, 0f, -0.1f);
        iconObject.layer = go.layer;

        var iconRenderer = iconObject.AddComponent<SpriteRenderer>();
        iconRenderer.sprite = sprite;
        iconRenderer.color = Color.white;
        if (root)
        {
            iconRenderer.sortingLayerID = root.sortingLayerID;
            iconRenderer.sortingOrder = root.sortingOrder + 1;
        }

        // Fit the icon inside the box, whatever the PNG's resolution is.
        var boxSize = root && root.sprite ? root.sprite.bounds.size : Vector3.one;
        var spriteSize = sprite.bounds.size;
        var iconScale = Mathf.Min(boxSize.x, boxSize.y) * IconFill / Mathf.Max(spriteSize.x, spriteSize.y, 0.01f);
        iconObject.transform.localScale = Vector3.one * iconScale;

        // Keep the clone's collider and hover behaviour, only replace what a click does.
        var button = go.GetComponent<PassiveButton>();
        if (button)
        {
            button.OnClick = new Button.ButtonClickedEvent();
            button.OnClick.AddListener((UnityAction)(() => PlayerControl.LocalPlayer.RpcErase(targetId)));
        }

        return go;
    }
}
