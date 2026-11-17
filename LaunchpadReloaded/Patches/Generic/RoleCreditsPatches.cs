using System;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using LaunchpadRevamped.Features;
using MiraAPI.Patches.Options;
using MiraAPI.Roles;
using TMPro;
using UnityEngine;
using Object = UnityEngine.Object;

namespace LaunchpadRevamped.Patches.Generic;

/// <summary>
/// Shows "Role Idea by ..." / "Coded by ..." under the description (the INFO box) on the settings page of a role.
/// It is a separate postfix so it also works when the role category picker is switched off.
/// </summary>
[HarmonyPatch(typeof(RolesSettingsMenu), nameof(RolesSettingsMenu.Update))]
public static class RoleCreditsPatches
{
    // Tweak these if the credits sit too close to / too far from the description, or are too big.
    private const float FontScale = 0.6f;
    private const float Gap = 0.06f;
    private const string CreditsName = "LaunchpadRoleCredits";

    private static readonly PropertyInfo? CurrentRoleProperty = AccessTools.Property(
        typeof(RoleSettingMenuPatches), "CurrentRole");

    private static TextMeshPro? _credits;
    private static bool _broken;

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
            Warning($"[RoleCredits] Turned off after an error: {e}");
        }
    }

    private static void Refresh(RolesSettingsMenu menu)
    {
        var page = menu.AdvancedRolesSettings;
        if (!page || !page.activeInHierarchy)
        {
            SetCreditsActive(false);
            return;
        }

        var role = CurrentRoleProperty?.GetValue(null) as RoleBehaviour;
        if (!role || role is not ICustomRole customRole
            || !RoleCredits.TryGet(role!.GetType(), out var credit))
        {
            SetCreditsActive(false);
            return;
        }

        var description = FindDescription(menu, customRole);
        if (!description)
        {
            SetCreditsActive(false);
            return;
        }

        if (!_credits || _credits!.transform.parent != description!.transform.parent)
        {
            if (_credits)
            {
                Object.Destroy(_credits!.gameObject);
            }

            _credits = Object.Instantiate(description, description.transform.parent);
            _credits.name = CreditsName;
        }

        var tmp = _credits!;
        tmp.gameObject.SetActive(true);
        tmp.enableAutoSizing = false;
        tmp.enableWordWrapping = false;
        tmp.fontSize = description.fontSize * FontScale;
        tmp.fontStyle = FontStyles.Normal;
        tmp.alignment = TextAlignmentOptions.Top;
        tmp.color = new Color(1f, 1f, 1f, 0.6f);
        tmp.text = credit.ToMenuText();
        tmp.rectTransform.pivot = new Vector2(0.5f, 1f);

        // Sit just under the last line of the description, centred on it.
        description.ForceMeshUpdate();
        var bounds = description.textBounds;
        var bottom = description.transform.TransformPoint(new Vector3(bounds.center.x, bounds.min.y, 0f));
        var local = description.transform.parent.InverseTransformPoint(bottom);
        tmp.transform.localPosition = new Vector3(local.x, local.y - Gap, description.transform.localPosition.z);
    }

    // The description is the text of the role page that shows the role's description. The field is tried first,
    // then any visible text on the page that matches one of the role's descriptions.
    private static TextMeshPro? FindDescription(RolesSettingsMenu menu, ICustomRole role)
    {
        // Il2Cpp fields are exposed as properties, so the lookup goes by property name.
        TextMeshPro? field = null;
        try
        {
            field = Traverse.Create(menu).Property("roleDescriptionText").GetValue<TextMeshPro>();
        }
        catch (Exception)
        {
            // not there in this game version: the text scan below is used instead
        }

        if (field && field!.gameObject.activeInHierarchy)
        {
            return field;
        }

        var candidates = new[] { role.RoleMedDescription, role.RoleLongDescription, role.RoleDescription }
            .Where(text => !string.IsNullOrWhiteSpace(text))
            .Select(text => text.Trim())
            .ToArray();

        return menu.AdvancedRolesSettings.GetComponentsInChildren<TextMeshPro>(false)
            .FirstOrDefault(tmp => tmp.name != CreditsName
                && candidates.Any(text => tmp.text.Contains(text, StringComparison.OrdinalIgnoreCase)));
    }

    private static void SetCreditsActive(bool active)
    {
        if (_credits && _credits!.gameObject.activeSelf != active)
        {
            _credits.gameObject.SetActive(active);
        }
    }
}
