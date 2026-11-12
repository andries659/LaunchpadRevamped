using System;
using LaunchpadRevamped.Features;
using LaunchpadRevamped.Options;
using MiraAPI.GameOptions;
using Reactor.Utilities.Attributes;
using UnityEngine;

namespace LaunchpadRevamped.Components;

/// <summary>
/// Shows a role icon to the left of this player's name, when the local player is allowed to see it.
/// </summary>
[RegisterInIl2Cpp]
public class RoleIconManager(IntPtr ptr) : MonoBehaviour(ptr)
{
    private const float IconHeight = 0.3f;
    private const float Gap = 0.05f;

    private PlayerControl _player = null!;
    private SpriteRenderer? _icon;

    public void Awake()
    {
        _player = GetComponent<PlayerControl>();
    }

    public void OnDestroy()
    {
        if (_icon)
        {
            Destroy(_icon!.gameObject);
        }
    }

    private static bool Enabled()
    {
        try
        {
            return OptionGroupSingleton<GeneralOptions>.Instance.RoleIcons;
        }
        catch
        {
            return true;
        }
    }

    public void LateUpdate()
    {
        if (!_player || !_player.cosmetics || !_player.cosmetics.nameText)
        {
            return;
        }

        var nameText = _player.cosmetics.nameText;
        var nameRenderer = nameText.GetComponent<Renderer>();

        // Roles only exist during a game, and the icon follows the name: no name, no icon.
        var visible = Enabled()
                      && ShipStatus.Instance
                      && nameRenderer
                      && nameText.gameObject.activeInHierarchy
                      && RoleIconRules.TryGetIcon(PlayerControl.LocalPlayer, _player, out _);

        if (!visible)
        {
            if (_icon)
            {
                _icon!.enabled = false;
            }

            return;
        }

        RoleIconRules.TryGetIcon(PlayerControl.LocalPlayer, _player, out var sprite);

        if (!_icon)
        {
            var go = new GameObject("RoleIcon") { layer = nameText.gameObject.layer };
            go.transform.SetParent(nameText.transform.parent, false);
            _icon = go.AddComponent<SpriteRenderer>();
        }

        var icon = _icon!;
        icon.enabled = true;

        if (icon.sprite != sprite)
        {
            icon.sprite = sprite;
        }

        // Same layer and order as the name, so it is drawn with it.
        icon.sortingLayerID = nameRenderer.sortingLayerID;
        icon.sortingOrder = nameRenderer.sortingOrder;

        var spriteSize = sprite.bounds.size;
        var worldWidth = IconHeight * (spriteSize.x / spriteSize.y);

        var bounds = nameRenderer.bounds;
        var position = new Vector3(bounds.min.x - Gap - worldWidth / 2f, bounds.center.y, nameText.transform.position.z);

        var parentScale = icon.transform.parent ? icon.transform.parent.lossyScale.x : 1f;
        var scale = IconHeight / spriteSize.y / (Mathf.Abs(parentScale) > 0.0001f ? Mathf.Abs(parentScale) : 1f);

        icon.transform.position = position;
        icon.transform.localScale = new Vector3(scale, scale, 1f);
    }
}
