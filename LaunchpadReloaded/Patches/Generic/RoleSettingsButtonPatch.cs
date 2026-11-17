using System;
using System.Reflection;
using HarmonyLib;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using MiraAPI.Utilities.Assets;
using Reactor.Utilities.Extensions;
using UnityEngine;
using UnityEngine.Events;
using static UnityEngine.UI.Button;

namespace LaunchpadRevamped.Patches.Generic;

/// <summary>
/// Mira only shows the role's cog button when the role has visible options.
/// This adds it for every other custom role so the description panel can still be opened.
/// </summary>
[HarmonyPatch]
public static class RoleSettingsButtonPatch
{
    private static readonly Type MenuPatches =
        AccessTools.TypeByName("MiraAPI.Patches.Options.RoleSettingMenuPatches");

    private static readonly MethodInfo ChangeTab = AccessTools.Method(MenuPatches, "ChangeTab");

    public static MethodBase TargetMethod() => AccessTools.Method(MenuPatches, "CreateQuotaOption");

    // __0 / __1 = the target's first and second arguments (it's static, so __instance would be null)
    public static void Postfix(RolesSettingsMenu __0, RoleBehaviour __1, RoleOptionSetting? __result)
    {
        var menu = __0;
        var role = __1;

        if (__result == null || role is not ICustomRole customRole) return;
        if (__result.transform.Find("ConfigButton") != null) return; // Mira already added one

        var newButton = UnityEngine.Object.Instantiate(__result.buttons[0], __result.transform);
        newButton.name = "ConfigButton";
        newButton.transform.localPosition = new Vector3(0.4473f, -0.3f, -2f);
        newButton.transform.FindChild("Text_TMP").gameObject.Destroy();
        newButton.activeSprites.Destroy();

        var btnRend = newButton.transform.FindChild("ButtonSprite").GetComponent<SpriteRenderer>();
        btnRend.sprite = MiraAssets.Cog.LoadAsset();

        var button = newButton.GetComponent<GameOptionButton>();
        button.OnClick = new ButtonClickedEvent();
        button.interactableColor = btnRend.color = customRole.OptionsMenuColor.FindAlternateColor();
        button.interactableHoveredColor = Color.white;
        button.OnClick.AddListener((UnityAction)(() =>
        {
            ChangeTab.Invoke(null, [role, menu]);
        }));
    }
}
