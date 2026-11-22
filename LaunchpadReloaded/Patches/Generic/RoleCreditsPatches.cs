using System;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using LaunchpadRevamped.Features;
using MiraAPI.Patches.Options;
using MiraAPI.Roles;
using MiraAPI.Utilities.Components;
using TMPro;
using UnityEngine;
using Object = UnityEngine.Object;

namespace LaunchpadRevamped.Patches.Generic;

/// <summary>
/// Shows "Role Idea by ..." on a flipped copy of the role name tab (RolesSettingsMenu.roleHeaderSprite, the coloured
/// tab with the role name), hanging directly under it. Nothing else on the page is moved.
/// </summary>
[HarmonyPatch(typeof(RolesSettingsMenu), nameof(RolesSettingsMenu.Update))]
public static class RoleCreditsPatches
{
    // Tweak these if the tab sits too close to / too far from the role name tab, or is too big / small.
    private const float FontScale = 0.8f;   // credit text size relative to the role name text
    private const float HeightScale = 0.6f; // credit tab height relative to the role name tab
    private const float WidthScale = 1f;    // credit tab width relative to the role name tab (grows to the left)
    private const bool ShowCodedBy = false; // also show the "Coded by" line
    private const float Gap = 0f;           // space between the two tabs (positive = further apart)
    private const float OffsetX = 0f;       // positive = further right
    private const float Darken = 0.55f;     // credit tab colour = role tab colour * this (1 = same, 0 = black)
    private const string TabName = "LaunchpadRoleCreditsTab";
    private const string TextName = "LaunchpadRoleCreditsText";

    private static readonly PropertyInfo? CurrentRoleProperty = AccessTools.Property(
        typeof(RoleSettingMenuPatches), "CurrentRole");

    private static GameObject? _tab;
    private static GameObject? _text;
    private static bool _broken;
    private static bool _logged;

    /// <summary>
    /// Kept so RoleCategorySwitcher still compiles. The credits tab hangs off the role name tab and takes no room
    /// on the page, so nothing has to be moved down any more.
    /// </summary>
    public static float ExtraHeight => 0f;

    [HarmonyPostfix]
    public static void UpdatePostfix(RolesSettingsMenu __instance)
    {
        if (_broken)
        {
            return;
        }

        try
        {
            Refresh(__instance);
        }
        catch (Exception e)
        {
            _broken = true;
            Hide();
            Warning($"[RoleCredits] Turned off after an error: {e}");
        }
    }

    private static void Refresh(RolesSettingsMenu menu)
    {
        var page = menu.AdvancedRolesSettings;
        if (!page || !page.activeInHierarchy)
        {
            Hide();
            return;
        }

        var role = CurrentRoleProperty?.GetValue(null) as RoleBehaviour;
        if (!role || role is not ICustomRole
            || !RoleCredits.TryGet(role!.GetType(), out var credit))
        {
            Hide();
            return;
        }

        // "Role Idea by" and (optionally) "Coded by", one per line.
        var line = string.Join("\n", credit.ToMenuText()
            .Split('\n')
            .Where(l => !string.IsNullOrWhiteSpace(l)
                        && (ShowCodedBy || !l.Contains("Coded", StringComparison.OrdinalIgnoreCase))));
        if (string.IsNullOrWhiteSpace(line))
        {
            Hide();
            return;
        }

        var header = menu.roleHeaderSprite;
        var headerText = menu.roleHeaderText;
        if (!header || !headerText || !header.gameObject.activeInHierarchy)
        {
            Hide();
            return;
        }

        EnsureClones(header, headerText);

        var tabRenderer = _tab!.GetComponent<SpriteRenderer>();
        var textMesh = _text!.GetComponent<TextMeshPro>();
        if (!tabRenderer || !textMesh)
        {
            Hide();
            return;
        }

        // Same look as the role name tab, mirrored top to bottom and shorter.
        // Mirrored with a negative scale (works for every sprite draw mode, unlike flipY).
        tabRenderer.flipY = false;
        tabRenderer.enabled = true;
        var tabColor = header.color;
        tabRenderer.color = new Color(tabColor.r * Darken, tabColor.g * Darken, tabColor.b * Darken, tabColor.a);
        var scale = header.transform.localScale;
        _tab.transform.localScale = new Vector3(scale.x * WidthScale, -Mathf.Abs(scale.y) * HeightScale, scale.z);
        _tab.transform.position = header.transform.position;

        // Hang it right under the role name tab, right edges lined up (absolute every frame: nothing builds up).
        var original = header.bounds;
        var flipped = tabRenderer.bounds;
        _tab.transform.position += new Vector3(
            original.max.x - flipped.max.x + OffsetX,
            original.min.y - flipped.max.y - Gap,
            0f);
        flipped = tabRenderer.bounds;

        // Text: a copy of the role name text, showing the credit line, centred vertically on the new tab.
        textMesh.enableAutoSizing = false;
        textMesh.enableWordWrapping = false;
        textMesh.fontSize = headerText.fontSize * FontScale;
        textMesh.fontStyle = FontStyles.Normal;
        textMesh.color = Color.white; // the role name colour is dark, which would vanish on the darker tab
        textMesh.text = line;

        // Same distance from the right edge as the role name has on its tab.
        var textInsetX = original.max.x - headerText.transform.position.x;
        _text.transform.position = new Vector3(
            flipped.max.x - textInsetX,
            flipped.center.y,
            headerText.transform.position.z);

        // Draw the tab just behind the credit text, in front of the dark strip it overlaps.
        var tabPosition = _tab.transform.position;
        _tab.transform.position = new Vector3(tabPosition.x, tabPosition.y, _text.transform.position.z + 0.01f);
        tabRenderer.sortingLayerID = header.sortingLayerID;
        tabRenderer.sortingOrder = Mathf.Max(header.sortingOrder, textMesh.sortingOrder);

        _tab.SetActive(true);
        _text.SetActive(true);

        if (!_logged)
        {
            _logged = true;
            Warning($"[RoleCredits] header z={header.transform.position.z} order={header.sortingOrder} "
                + $"mode={header.drawMode} size={header.size} bounds={header.bounds}; "
                + $"tab z={_tab.transform.position.z} order={tabRenderer.sortingOrder} bounds={tabRenderer.bounds}; "
                + $"text z={_text.transform.position.z} order={textMesh.sortingOrder}");
        }
    }

    private static void EnsureClones(SpriteRenderer header, TMP_Text headerText)
    {
        if (!_tab || _tab!.transform.parent != header.transform.parent)
        {
            if (_tab)
            {
                Object.Destroy(_tab!);
            }

            _tab = Object.Instantiate(header.gameObject, header.transform.parent);
            _tab.name = TabName;

            // Only the tab shape is wanted, not whatever hangs under it.
            for (var i = 0; i < _tab.transform.childCount; i++)
            {
                Object.Destroy(_tab.transform.GetChild(i).gameObject);
            }
        }

        if (!_text || _text!.transform.parent != headerText.transform.parent)
        {
            if (_text)
            {
                Object.Destroy(_text!);
            }

            _text = Object.Instantiate(headerText.gameObject, headerText.transform.parent);
            _text.name = TextName;
            DropTranslator(_text);
        }
    }

    private static void DropTranslator(GameObject target)
    {
        var translator = target.GetComponent<TextTranslatorTMP>();
        if (translator)
        {
            Object.Destroy(translator);
        }
    }

    private static void Hide()
    {
        if (_tab && _tab!.activeSelf)
        {
            _tab.SetActive(false);
        }

        if (_text && _text!.activeSelf)
        {
            _text.SetActive(false);
        }
    }
}