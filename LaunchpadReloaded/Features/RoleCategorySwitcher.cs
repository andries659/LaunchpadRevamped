using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using AmongUs.GameOptions;
using HarmonyLib;
using MiraAPI.GameOptions;
using MiraAPI.LocalSettings;
using MiraAPI.Patches.Options;
using MiraAPI.Roles;
using MiraAPI.Translation;
using MiraAPI.Utilities;
using Reactor.Localization.Utilities;
using Reactor.Utilities.Extensions;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using Object = UnityEngine.Object;

namespace LaunchpadRevamped.Features;

/// <summary>
/// The role category picker for this mod's page of the role settings tab. Its visuals are cloned from the map picker
/// on the normal Game settings page, with the map buttons repurposed for this mod's role categories.
///
/// MiraAPI already groups roles under collapsible headers (one per <see cref="RoleOptionsGroup"/>), so instead of
/// laying rows out by hand the picker drives that same mechanism: it opens only the chosen team's group, lets MiraAPI
/// rebuild the list, and then hides the headers of the collapsed groups.
///
/// Everything here is defensive: if MiraAPI's internals change, the feature switches itself off
/// and puts the tab back the way MiraAPI lays it out. It can also be turned off in the Launchpad client settings.
/// </summary>
public static class RoleCategorySwitcher
{
    private enum Category
    {
        Crewmate,
        Impostor,
        Neutral,
        Coven,
        Modifiers,
    }

    private static readonly Category[] AllCategories = [Category.Crewmate, Category.Impostor, Category.Neutral, Category.Coven];
    private static readonly Category[] TeamCategories = [Category.Crewmate, Category.Impostor, Category.Neutral, Category.Coven];

    // Geometry of the role tab, in the role menu's local space. These are the exact values MiraAPI uses itself:
    // on page 0 the scroller sits lower and the list is shorter to leave room for a row of header buttons. On a mod
    // page MiraAPI hides that row and stretches the list over it, and here the picker takes the freed space instead.
    private static readonly Vector3 PickerScrollerPosition = new(-1.4957f, 0.657f, -4f);
    // MiraAPI's picker-page mask, with its top edge lowered by 0.37 (bottom edge unchanged). The list content starts about
    // half a unit below the top of the mask, so that empty strip is given to the picker instead.
    private static readonly Vector3 PickerMaskPosition = new(1.5353f, -0.7594f, -0.1f);
    private static readonly Vector3 PickerMaskScale = new(6.6811f, 2.9843f, 0.5598f);
    private static readonly Vector3 PickerHitboxPosition = new(0.3297f, -0.2333f, 4f);
    private static readonly Vector3 PickerHitboxScale = new(1f, 1f, 1f);

    private static readonly Vector3 ModScrollerPosition = new(-1.4957f, 1.5261f, -4f);
    private static readonly Vector3 ModMaskPosition = new(1.5353f, -1.0607f, -0.1f);
    private static readonly Vector3 ModMaskScale = new(6.6811f, 4.1563f, 0.5598f);
    private static readonly Vector3 ModHitboxPosition = new(0.3297f, -0.6333f, 4f);
    private static readonly Vector3 ModHitboxScale = new(1f, 1.2f, 1f);

    // Handmade map-picker-style selector, placed in the free band above the role list.
    private const float PickerCenterX = 0.04f;
    private const float PickerBottomY = 1.43f;
    private const float PickerZ = -2f;
    private const float PickerWidth = 5.0f;
    private const float NameHeightRatio = 0.62f;
    private const float IconSize = 0.46f;
    private const float IconSpacing = 0.62f;
    private const float LabelWidth = 1.2f;
    private const float LabelFontSize = 2.2f;
    private const float RowGap = 0.12f;
    private const int PickerSortingOrder = 5000;
    private static readonly Color SelectedColor = new(0.18f, 0.80f, 0.70f, 1f);

    // MiraAPI sizes the scroll range for the taller list, so with the shorter one it is recomputed from the content.
    private const float RowBottomOffset = 0.55f; // how far a row reaches below its position
    private const float ScrollBottomPad = 0.1f;   // space left under the last row
    private const float MinScroll = 0.2f;         // lists that overflow by less than this don't scroll

    // Vertical space MiraAPI reserves for a collapsed group header.
    private const float CollapsedHeaderHeight = 0.422f;

    private const float FilterCheckInterval = 0.25f;

    private sealed class PickerButton
    {
        public Category Category;
        public GameObject Object = null!;
        public PassiveButton Button = null!;
        public SpriteRenderer Fill = null!;
        public SpriteRenderer Check = null!;
        public SpriteRenderer? Icon;
        public Vector3 BaseScale;
        public Color BaseColor = new(0.08f, 0.09f, 0.10f, 1f);
    }

    private static readonly List<PickerButton> PickerButtons = [];

    private static GameObject? _pickerRoot;
    private static SpriteRenderer? _banner;
    private static SpriteRenderer? _name;
    private static TextMeshPro? _pickerLabel;
    private static float _bannerHeight;
    private static Sprite? _circleSprite;
    private static Sprite? _checkSprite;

    // The picker is cloned from the real map picker on the Game settings page. If that can't be done (map picker not
    // found, or its layout isn't what is expected) the handmade selector below is used instead.
    private const bool UseClonedPicker = false;
    private static bool _clonedPicker;
    private static bool _cloneWarned;
    private static bool _cloneDumped;
    private static Vector2 _bannerBox;
    private static Vector2 _nameBox;

    private static Category? _selected;
    private static Category? _hovered;
    private static bool _needsInitialSelect = true;
    private static bool _visualsDirty;
    private static bool _broken;
    private static float _nextFilterCheck;
    private static float _nextPickerAttempt;

    // While MiraAPI rebuilds the list it briefly shows every group header. The list is hidden (scaled down to nothing,
    // which doesn't touch the positions the layout code works with) until the filter has run.
    private static Transform? _hiddenList;
    private static Vector3 _hiddenListScale = Vector3.one;
    private static float _hideDeadline;
    private static bool _wasInDetails;
    private static readonly Dictionary<int, Renderer> LeakHidden = new();
    private static bool _initialHideDone;
    private const float HideTimeout = 4f;
    private const float LeakMargin = 0.05f;
    private const bool UseLeakHiding = false; // rows are clipped by the mask now, hiding them by hand only made them pop

    /// <summary>
    /// Called every frame while the role settings menu is open.
    /// </summary>
    public static void Update(RolesSettingsMenu menu)
    {
        if (_broken)
        {
            return;
        }

        try
        {
            if (!ShouldShow())
            {
                SetPickerActive(false);
                ShowList();
                RestoreLeaks();
                return;
            }

            // The settings page of a single role has its own layout, so the picker steps aside for it.
            if (IsRoleDetailsOpen(menu))
            {
                _wasInDetails = true;
                SetPickerActive(false);
                ApplyGeometry(menu, false);
                ShowList();
                RestoreLeaks();
                UpdateDetails(menu);
                return;
            }

            if (_wasInDetails)
            {
                // Back on the list: keep it hidden until the filter has run, in case it was rebuilt.
                _wasInDetails = false;
                HideList(menu);
                _nextFilterCheck = 0f;
            }

            if (!HasPicker())
            {
                if (Time.unscaledTime < _nextPickerAttempt)
                {
                    SetPickerActive(false);
                    ShowList();
                    ApplyGeometry(menu, false);
                    return;
                }

                _nextPickerAttempt = Time.unscaledTime + 2f;
                if (!CreatePicker(menu))
                {
                    SetPickerActive(false);
                    ShowList();
                    ApplyGeometry(menu, false);
                    return;
                }
            }

            SetPickerActive(true);
            ApplyGeometry(menu, true);

            if (UseLeakHiding && !_hiddenList)
            {
                HideLeaks(menu);
            }

            if (_needsInitialSelect)
            {
                if (!_initialHideDone)
                {
                    _initialHideDone = true;
                    HideList(menu);
                }

                TryInitialSelect(menu);
            }

            if (_visualsDirty)
            {
                ApplyVisuals();
            }

            if (_hiddenList && Time.unscaledTime > _hideDeadline)
            {
                ShowList();
            }

            // While the list is hidden the filter is checked every frame, so it is shown again as soon as it is ready.
            if (Time.unscaledTime >= _nextFilterCheck || _hiddenList)
            {
                _nextFilterCheck = Time.unscaledTime + FilterCheckInterval;

                // The list is restyled while it is still hidden, so MiraAPI's own rows are never seen.
                SkinList(menu);
                ApplyFilter(menu);
                EnsureScrollBounds(menu);
                RefreshToggles();
            }
        }
        catch (Exception e)
        {
            Fail(menu, $"Disabled after an error: {e}");
        }
    }

    /// <summary>
    /// MiraAPI lays out the tab for a mod page when it (re)opens it, so the picker's layout is applied right after that.
    /// </summary>
    public static void AfterOpenChancesTab(RolesSettingsMenu menu)
    {
        if (_broken || !HasPicker() || !ShouldShow())
        {
            return;
        }

        try
        {
            ApplyGeometry(menu, true);
        }
        catch (Exception e)
        {
            Fail(menu, $"Disabled after an error: {e}");
        }
    }

    private static void Fail(RolesSettingsMenu menu, string message)
    {
        _broken = true;
        ShowList();
        RestoreLeaks();
        Error($"[RoleCategorySwitcher] {message}");

        try
        {
            SetPickerActive(false);
            ApplyGeometry(menu, false);
        }
        catch (Exception e)
        {
            Error($"[RoleCategorySwitcher] Could not restore the default layout: {e}");
        }
    }

    private static bool ShouldShow()
    {
        if (!LocalSettingsTabSingleton<LaunchpadSettings>.Instance.RoleCategoryButtons.Value)
        {
            return false;
        }

        var state = MenuState.Instance;
        if (!state || state.CurrentModIdx == 0 || state.CurrentMenu != MenuCategory.Roles)
        {
            return false;
        }

        return state.CurrentMod.PluginId == LaunchpadRevampedPlugin.Id;
    }

    /// <summary>
    /// True while the settings page of a single role is open, which switches the role list off. (The back button can't be
    /// used for this: it is active on the list page too.)
    /// </summary>
    private static bool IsRoleDetailsOpen(RolesSettingsMenu menu)
    {
        return !menu.RoleChancesSettings.transform.gameObject.activeInHierarchy;
    }

    private static bool HasPicker()
    {
        return _pickerRoot && _banner && _name && _pickerLabel && PickerButtons.Count == AllCategories.Length
               && PickerButtons.All(button => button.Object && button.Button && button.Fill && button.Check);
    }

    private static void SetPickerActive(bool active)
    {
        if (_pickerRoot && _pickerRoot!.activeSelf != active)
        {
            _pickerRoot.SetActive(active);
        }
    }

    /// <summary>
    /// Builds a small, self-contained selector using the same banner and circular-button layout as the normal map picker.
    /// Category clicks continue to use the role group's existing filtering/rebuild behavior.
    /// </summary>
    private static bool CreatePicker(RolesSettingsMenu menu)
    {
        if (_pickerRoot)
        {
            Object.Destroy(_pickerRoot);
        }

        PickerButtons.Clear();
        _banner = null;
        _name = null;
        _pickerLabel = null;
        _clonedPicker = false;

        if (UseClonedPicker && TryCreateClonedPicker(menu))
        {
            return true;
        }

        var layer = menu.gameObject.layer;
        var root = new GameObject("RoleCategoryPicker") { layer = layer };
        root.transform.SetParent(menu.transform, false);
        root.transform.localPosition = new Vector3(PickerCenterX, PickerBottomY, PickerZ);
        root.transform.localRotation = Quaternion.identity;
        root.SetActive(false);
        _pickerRoot = root;

        var bannerSprite = BannerFor(Category.Crewmate).LoadAsset();
        if (!bannerSprite || bannerSprite.bounds.size.x <= 0f)
        {
            Object.Destroy(root);
            _pickerRoot = null;
            Warning("[RoleCategorySwitcher] The role category banner art could not be loaded.");
            return false;
        }

        _bannerHeight = PickerWidth * bannerSprite.bounds.size.y / bannerSprite.bounds.size.x;
        var bannerY = IconSize + RowGap + _bannerHeight / 2f;
        _banner = CreateSprite(root, "Banner", layer, new Vector3(0f, bannerY, 0f), PickerSortingOrder);
        _name = CreateSprite(root, "CategoryName", layer, new Vector3(0f, bannerY, -0.1f), PickerSortingOrder + 2);
        FitWidth(_banner, bannerSprite, PickerWidth);

        // Dims the banner art and gives it a thin rounded border, like the map picker's banner.
        var frame = CreateSprite(root, "BannerFrame", layer, new Vector3(0f, bannerY, -0.05f), PickerSortingOrder + 1);
        frame.sprite = GetRoundedRectSprite("frame", PickerWidth, _bannerHeight, 512, 14f, 3f,
            new Color(0f, 0f, 0f, 0.5f), new Color(0.78f, 0.80f, 0.82f, 0.95f));
        SetPreview(Category.Crewmate);

        var rowWidth = LabelWidth + AllCategories.Length * IconSpacing;
        var rowStart = -rowWidth / 2f;
        _pickerLabel = CreateLabel(menu, root, layer, new Vector3(rowStart + LabelWidth / 2f, IconSize / 2f, 0f));
        if (!_pickerLabel)
        {
            Object.Destroy(root);
            _pickerRoot = null;
            PickerButtons.Clear();
            Warning("[RoleCategorySwitcher] Could not find a game font for the handmade category selector.");
            return false;
        }

        var plate = CreateSprite(root, "LabelPlate", layer, new Vector3(rowStart + LabelWidth / 2f, IconSize / 2f, 0.05f), PickerSortingOrder);
        plate.sprite = GetRoundedRectSprite("plate", LabelWidth * 0.9f, IconSize * 0.78f, 256, 8f, 0f,
            new Color(0.07f, 0.08f, 0.09f, 1f), Color.clear);

        var clickSound = menu.BackButton ? menu.BackButton.GetComponent<PassiveButton>()?.ClickSound : null;
        for (var i = 0; i < AllCategories.Length; i++)
        {
            var category = AllCategories[i];
            var position = new Vector3(rowStart + LabelWidth + (i + 0.5f) * IconSpacing, IconSize / 2f, -0.1f);
            CreateCategoryButton(root, category, position, layer, menu, clickSound);
        }

        ClearPickerMasks(root);

        // ClearPickerMasks removes every mask interaction; the icons need theirs back to be clipped to their circle.
        foreach (var pickerButton in PickerButtons)
        {
            if (pickerButton.Icon)
            {
                pickerButton.Icon!.maskInteraction = SpriteMaskInteraction.VisibleInsideMask;
            }
        }

        root.SetActive(true);

        _selected = null;
        _hovered = null;
        _needsInitialSelect = true;
        _initialHideDone = false;
        _visualsDirty = true;
        _nextFilterCheck = 0f;

        return PickerButtons.Count == AllCategories.Length;
    }

    // ------------------------------------------------------------------------------------------------------------
    // Cloned map picker
    // ------------------------------------------------------------------------------------------------------------

    /// <summary>
    /// Clones the map picker from the Game settings page and repurposes it: the banner, the name sprite and the
    /// round buttons are kept (so it looks exactly like the map picker), and only their sprites and click handlers
    /// are swapped for the role categories. Returns false (and cleans up) if anything doesn't look as expected, in
    /// which case the handmade selector is built instead.
    /// </summary>
    private static bool TryCreateClonedPicker(RolesSettingsMenu menu)
    {
        GameObject? holder = null;
        GameObject? root = null;

        try
        {
            var source = FindMapPickerSource();
            if (!source)
            {
                if (!_cloneWarned)
                {
                    _cloneWarned = true;
                    Warning("[RoleCategorySwitcher] The map picker was not found; using the handmade category selector.");
                }

                return false;
            }

            // Cloned under an inactive holder so none of the map picker's scripts or buttons wake up before they are rewired.
            holder = new GameObject("RoleCategoryPickerHolder");
            holder.SetActive(false);
            root = Object.Instantiate(source!, holder.transform, false);
            root.name = "RoleCategoryPicker";
            root.SetActive(false);

            // The map picker's own script would write the selected map back into the lobby settings, so it is removed.
            foreach (var behaviour in root.GetComponentsInChildren<MonoBehaviour>(true))
            {
                if (behaviour && behaviour.GetIl2CppType().Name.Contains("MapPicker"))
                {
                    Object.DestroyImmediate(behaviour);
                }
            }

            var layer = menu.gameObject.layer;
            root.transform.SetParent(menu.transform, false);
            foreach (var child in root.GetComponentsInChildren<Transform>(true))
            {
                child.gameObject.layer = layer;
            }

            Object.Destroy(holder);
            holder = null;

            if (!_cloneDumped)
            {
                _cloneDumped = true;
                DumpHierarchy(root.transform, 0);
            }

            var sourceButtons = root.GetComponentsInChildren<PassiveButton>(true)
                .Where(button => button && button.GetComponentsInChildren<SpriteRenderer>(true).Length > 0)
                .OrderBy(button => button.transform.localPosition.x)
                .ToList();
            if (sourceButtons.Count == 0)
            {
                throw new InvalidOperationException("the map picker has no buttons");
            }

            var buttonRenderers = new HashSet<int>();
            foreach (var button in sourceButtons)
            {
                foreach (var renderer in button.GetComponentsInChildren<SpriteRenderer>(true))
                {
                    buttonRenderers.Add(renderer.GetInstanceID());
                }
            }

            // Everything that isn't part of a button: the biggest sprite is the banner, the next one the map name.
            var loose = root.GetComponentsInChildren<SpriteRenderer>(true)
                .Where(renderer => renderer && renderer.sprite && !buttonRenderers.Contains(renderer.GetInstanceID()))
                .OrderByDescending(RenderArea)
                .ToList();
            if (loose.Count == 0)
            {
                throw new InvalidOperationException("the map picker has no banner sprite");
            }

            var banner = loose[0];
            var name = loose.Count > 1 ? loose[1] : null;
            banner.drawMode = SpriteDrawMode.Simple;
            _bannerBox = Box(banner);

            if (name == null)
            {
                name = CreateSprite(banner.transform.parent.gameObject, "CategoryName", layer,
                    banner.transform.localPosition + new Vector3(0f, 0f, -0.1f), banner.sortingOrder + 1);
                _nameBox = new Vector2(_bannerBox.x * 0.8f, _bannerBox.y * NameHeightRatio);
            }
            else
            {
                name.drawMode = SpriteDrawMode.Simple;
                _nameBox = Box(name);
            }

            // Exactly as many buttons as there are categories.
            while (sourceButtons.Count < AllCategories.Length)
            {
                var last = sourceButtons[^1];
                var step = sourceButtons.Count > 1
                    ? last.transform.localPosition - sourceButtons[^2].transform.localPosition
                    : new Vector3(IconSpacing, 0f, 0f);
                var copy = Object.Instantiate(last.gameObject, last.transform.parent, false);
                copy.transform.localPosition = last.transform.localPosition + step;
                sourceButtons.Add(copy.GetComponent<PassiveButton>());
            }

            for (var i = sourceButtons.Count - 1; i >= AllCategories.Length; i--)
            {
                Object.Destroy(sourceButtons[i].gameObject);
                sourceButtons.RemoveAt(i);
            }

            var label = root.GetComponentsInChildren<TextMeshPro>(true).FirstOrDefault(text => text);
            if (label)
            {
                label!.GetComponent<TextTranslatorTMP>()?.Destroy();
                label.text = MiraLocaleManager.Get("launchpad.rolecategory.label");
            }
            else
            {
                var first = sourceButtons[0].transform.localPosition;
                label = CreateLabel(menu, root, layer, new Vector3(first.x - LabelWidth / 2f - IconSize, first.y, 0f));
                if (!label)
                {
                    throw new InvalidOperationException("no font was found for the label");
                }
            }

            PickerButtons.Clear();
            var clickSound = menu.BackButton ? menu.BackButton.GetComponent<PassiveButton>()?.ClickSound : null;
            for (var i = 0; i < AllCategories.Length; i++)
            {
                SetUpClonedButton(menu, sourceButtons[i], AllCategories[i], layer, clickSound);
            }

            _pickerRoot = root;
            _banner = banner;
            _name = name;
            _pickerLabel = label;
            _clonedPicker = true;

            SetPreview(Category.Crewmate);
            ClearPickerMasks(root);

            root.transform.localPosition = new Vector3(PickerCenterX, PickerBottomY, PickerZ);
            root.transform.localScale = Vector3.one;
            root.SetActive(true);

            var ratio = 0.35f;
            var bannerSprite = BannerFor(Category.Crewmate).LoadAsset();
            if (bannerSprite && bannerSprite.bounds.size.x > 0f)
            {
                ratio = bannerSprite.bounds.size.y / bannerSprite.bounds.size.x;
            }

            PlaceClonedRoot(menu, root, IconSize + RowGap + PickerWidth * ratio);

            _selected = null;
            _hovered = null;
            _needsInitialSelect = true;
            _initialHideDone = false;
            _visualsDirty = true;
            _nextFilterCheck = 0f;

            return HasPicker();
        }
        catch (Exception e)
        {
            Warning($"[RoleCategorySwitcher] Could not clone the map picker ({e.Message}); using the handmade category selector.");
            if (holder)
            {
                Object.Destroy(holder);
            }

            if (root)
            {
                Object.Destroy(root);
            }

            PickerButtons.Clear();
            _pickerRoot = null;
            _banner = null;
            _name = null;
            _pickerLabel = null;
            _clonedPicker = false;
            return false;
        }
    }

    private static GameObject? FindMapPickerSource()
    {
        var settingMenu = GameSettingMenu.Instance;
        if (!settingMenu)
        {
            return null;
        }

        foreach (var behaviour in settingMenu.GetComponentsInChildren<MonoBehaviour>(true))
        {
            if (behaviour && behaviour.GetIl2CppType().Name == "GameOptionsMapPicker")
            {
                return behaviour.gameObject;
            }
        }

        return null;
    }

    private static void SetUpClonedButton(RolesSettingsMenu menu, PassiveButton button, Category category, int layer, AudioClip? clickSound)
    {
        button.gameObject.name = $"RoleCategory_{category}";
        WireButton(button, category, menu, clickSound);

        var renderers = button.GetComponentsInChildren<SpriteRenderer>(true).Where(renderer => renderer).ToList();
        var check = FindRenderer(renderers, "check", "select", "tick", "mark");
        var rest = renderers.Where(renderer => renderer != check).ToList();
        var icon = FindRenderer(rest, "icon", "map", "image", "sprite");
        var fill = FindRenderer(rest.Where(renderer => renderer != icon).ToList(),
            "fill", "back", "bg", "outline", "ring", "border", "frame", "button");

        fill ??= rest.Where(renderer => renderer != icon).OrderByDescending(RenderArea).FirstOrDefault();
        icon ??= rest.Where(renderer => renderer != fill).OrderByDescending(RenderArea).FirstOrDefault() ?? fill;
        fill ??= icon;
        if (fill == null || icon == null)
        {
            throw new InvalidOperationException("a map button has no sprites");
        }

        var iconSprite = IconFor(category).LoadAsset();
        if (iconSprite)
        {
            icon.drawMode = SpriteDrawMode.Simple;
            FitBox(icon, iconSprite, Box(icon), true);
        }

        check ??= CreateOwnCheck(button, fill, layer);
        check.enabled = false;

        PickerButtons.Add(new PickerButton
        {
            Category = category,
            Object = button.gameObject,
            Button = button,
            Fill = fill,
            Check = check,
            Icon = icon,
            BaseScale = button.transform.localScale,
            BaseColor = fill.color,
        });
    }

    private static SpriteRenderer? FindRenderer(List<SpriteRenderer> renderers, params string[] tokens)
    {
        return renderers.FirstOrDefault(renderer => renderer
            && tokens.Any(token => renderer.gameObject.name.IndexOf(token, StringComparison.OrdinalIgnoreCase) >= 0));
    }

    private static SpriteRenderer CreateOwnCheck(PassiveButton button, SpriteRenderer fill, int layer)
    {
        var size = Box(fill);
        var centre = button.transform.InverseTransformPoint(fill.transform.position);
        var position = centre + new Vector3(size.x * 0.27f, size.y * 0.27f, -0.03f);
        var check = CreateSprite(button.gameObject, "SelectedCheck", layer, position, fill.sortingOrder + 4);
        check.sprite = GetCheckSprite();
        check.transform.localScale = Vector3.one * (size.x * 0.32f);
        return check;
    }

    private static float RenderArea(SpriteRenderer renderer)
    {
        var size = renderer.sprite.bounds.size;
        var scale = renderer.transform.lossyScale;
        return size.x * size.y * Mathf.Abs(scale.x * scale.y);
    }

    /// <summary>
    /// A sprite's size in its parent's space (sprite size times the renderer's local scale).
    /// </summary>
    private static Vector2 Box(SpriteRenderer renderer)
    {
        var size = renderer.sprite.bounds.size;
        var scale = renderer.transform.localScale;
        return new Vector2(size.x * Mathf.Abs(scale.x), size.y * Mathf.Abs(scale.y));
    }

    /// <summary>
    /// Puts a new sprite into the space the old one took. "contain" keeps the whole sprite inside the box; otherwise it
    /// is fitted to the box's width.
    /// </summary>
    private static void FitBox(SpriteRenderer renderer, Sprite sprite, Vector2 box, bool contain)
    {
        renderer.sprite = sprite;
        var size = sprite.bounds.size;
        var scaleX = box.x / Math.Max(0.001f, size.x);
        var scaleY = box.y / Math.Max(0.001f, size.y);
        var scale = contain ? Math.Min(scaleX, scaleY) : scaleX;
        renderer.transform.localScale = new Vector3(scale, scale, renderer.transform.localScale.z);
    }

    /// <summary>
    /// Scales the cloned picker so it fits the band above the role list, then puts its bottom edge and horizontal
    /// centre where the handmade selector used to sit.
    /// </summary>
    private static void PlaceClonedRoot(RolesSettingsMenu menu, GameObject root, float maxHeight)
    {
        var bounds = RendererBounds(root);
        if (bounds.size.x <= 0.0001f || bounds.size.y <= 0.0001f)
        {
            return;
        }

        var menuScale = menu.transform.lossyScale;
        var factor = Math.Min(PickerWidth * menuScale.x / bounds.size.x, maxHeight * menuScale.y / bounds.size.y);
        root.transform.localScale = Vector3.one * factor;

        bounds = RendererBounds(root);
        var target = menu.transform.TransformPoint(new Vector3(PickerCenterX, PickerBottomY, PickerZ));
        root.transform.position += new Vector3(target.x - bounds.center.x, target.y - bounds.min.y, 0f);
    }

    private static Bounds RendererBounds(GameObject root)
    {
        var bounds = new Bounds();
        var first = true;
        foreach (var renderer in root.GetComponentsInChildren<SpriteRenderer>(false))
        {
            if (!renderer || !renderer.enabled || !renderer.sprite)
            {
                continue;
            }

            if (first)
            {
                bounds = renderer.bounds;
                first = false;
            }
            else
            {
                bounds.Encapsulate(renderer.bounds);
            }
        }

        return bounds;
    }

    /// <summary>
    /// Logged once, so the layout of the real map picker can be checked in the BepInEx log if it ever needs tuning.
    /// </summary>
    private static void DumpHierarchy(Transform transform, int depth)
    {
        var parts = transform.GetComponents<Component>().Where(component => component).Select(component => component.GetIl2CppType().Name);
        var renderer = transform.GetComponent<SpriteRenderer>();
        var sprite = renderer && renderer.sprite ? renderer.sprite.name : "-";
        Info($"[RoleCategorySwitcher] map picker clone: {new string(' ', depth * 2)}{transform.name} [{string.Join(", ", parts)}] pos={transform.localPosition} scale={transform.localScale} sprite={sprite}");

        for (var i = 0; i < transform.childCount; i++)
        {
            DumpHierarchy(transform.GetChild(i), depth + 1);
        }
    }


    private static void CreateCategoryButton(
        GameObject root,
        Category category,
        Vector3 position,
        int layer,
        RolesSettingsMenu menu,
        AudioClip? clickSound)
    {
        var buttonObject = new GameObject($"RoleCategory_{category}") { layer = layer };
        buttonObject.transform.SetParent(root.transform, false);
        buttonObject.transform.localPosition = position;

        var collider = buttonObject.AddComponent<CircleCollider2D>();
        collider.radius = IconSize * 0.55f;
        var button = buttonObject.AddComponent<PassiveButton>();
        WireButton(button, category, menu, clickSound);

        var circle = GetCircleSprite();
        var ring = CreateSprite(buttonObject, "Ring", layer, Vector3.zero, PickerSortingOrder + 1);
        ring.sprite = circle;
        ring.transform.localScale = Vector3.one * IconSize;

        var fill = CreateSprite(buttonObject, "Fill", layer, new Vector3(0f, 0f, -0.01f), PickerSortingOrder + 2);
        fill.sprite = circle;
        fill.transform.localScale = Vector3.one * IconSize * 0.86f;
        fill.color = new Color(0.08f, 0.09f, 0.10f, 1f);

        // The icon fills the circle like the map picker's map images do, and is clipped to the circle by a mask.
        var iconDiameter = IconSize * 0.86f;
        var maskObject = new GameObject("IconMask") { layer = layer };
        maskObject.transform.SetParent(buttonObject.transform, false);
        maskObject.transform.localPosition = new Vector3(0f, 0f, -0.02f);
        maskObject.transform.localScale = Vector3.one * iconDiameter;
        maskObject.AddComponent<SpriteMask>().sprite = circle;

        var icon = CreateSprite(buttonObject, "Icon", layer, new Vector3(0f, 0f, -0.03f), PickerSortingOrder + 3);
        var iconSprite = IconFor(category).LoadAsset();
        if (iconSprite)
        {
            icon.sprite = iconSprite;
            var iconSize = iconSprite.bounds.size;
            var iconScale = iconDiameter / Math.Max(0.001f, Math.Max(iconSize.x, iconSize.y));
            icon.transform.localScale = new Vector3(iconScale, iconScale, 1f);
        }

        icon.maskInteraction = SpriteMaskInteraction.VisibleInsideMask;

        // "Selected" badge: a white circle with a teal tick, in the top right corner of the button.
        var check = CreateSprite(buttonObject, "SelectedCheck", layer,
            new Vector3(IconSize * 0.38f, IconSize * 0.38f, -0.05f), PickerSortingOrder + 4);
        check.sprite = GetCheckBadgeSprite();
        check.transform.localScale = Vector3.one * (IconSize * 0.42f);
        check.enabled = false;

        PickerButtons.Add(new PickerButton
        {
            Category = category,
            Object = buttonObject,
            Button = button,
            Fill = fill,
            Check = check,
            Icon = icon,
            BaseScale = buttonObject.transform.localScale,
        });
    }

    private static void WireButton(PassiveButton button, Category category, RolesSettingsMenu menu, AudioClip? clickSound)
    {
        button.ClickSound = clickSound;
        button.OnClick = new UnityEngine.UI.Button.ButtonClickedEvent();
        button.OnMouseOver = new UnityEvent();
        button.OnMouseOut = new UnityEvent();
        button.SetMaskLayer(0);

        var captured = category;
        button.OnMouseOver.AddListener((UnityAction)(() => SetHovered(captured)));
        button.OnMouseOut.AddListener((UnityAction)(() => SetHovered(null)));
        button.OnClick.AddListener((UnityAction)(() => OnCategoryClicked(menu, captured)));
        button.SetButtonEnableState(true);
    }

    private static TextMeshPro? CreateLabel(RolesSettingsMenu menu, GameObject root, int layer, Vector3 position)
    {
        var template = menu.GetComponentsInChildren<TextMeshPro>(true).FirstOrDefault(text => text && text.font);
        if (!template && GameSettingMenu.Instance)
        {
            template = GameSettingMenu.Instance.GetComponentsInChildren<TextMeshPro>(true)
                .FirstOrDefault(text => text && text.font);
        }

        if (!template)
        {
            return null;
        }

        var text = Object.Instantiate(template!, root.transform, false);
        var label = text.gameObject;
        label.name = "RoleCategoryLabel";
        label.layer = layer;
        label.SetActive(true);
        text.transform.localPosition = position;
        text.transform.localRotation = Quaternion.identity;
        text.transform.localScale = Vector3.one;
        foreach (var child in label.GetComponentsInChildren<Transform>(true))
        {
            child.gameObject.layer = layer;
        }

        text.GetComponent<TextTranslatorTMP>()?.Destroy();
        text.text = MiraLocaleManager.Get("launchpad.rolecategory.label");
        text.fontSize = LabelFontSize;
        text.color = Color.white;
        text.alignment = TextAlignmentOptions.Center;
        text.fontStyle = FontStyles.Bold;
        text.rectTransform.sizeDelta = new Vector2(LabelWidth, IconSize);

        var renderer = label.GetComponent<MeshRenderer>();
        if (renderer)
        {
            renderer.sortingOrder = PickerSortingOrder + 1;
        }

        return text;
    }

    private static SpriteRenderer CreateSprite(GameObject parent, string name, int layer, Vector3 localPosition, int sortingOrder)
    {
        var sprite = new GameObject(name) { layer = layer };
        sprite.transform.SetParent(parent.transform, false);
        sprite.transform.localPosition = localPosition;
        var renderer = sprite.AddComponent<SpriteRenderer>();
        renderer.sortingOrder = sortingOrder;
        return renderer;
    }

    private static void SetPreview(Category category)
    {
        if (_clonedPicker)
        {
            if (_banner)
            {
                FitBox(_banner!, BannerFor(category).LoadAsset(), _bannerBox, false);
            }

            if (_name)
            {
                FitBox(_name!, TextFor(category).LoadAsset(), _nameBox, true);
            }

            return;
        }

        if (_banner)
        {
            FitWidth(_banner!, BannerFor(category).LoadAsset(), PickerWidth);
        }

        if (_name)
        {
            FitHeight(_name!, TextFor(category).LoadAsset(), _bannerHeight * NameHeightRatio);
        }
    }

    private static void FitWidth(SpriteRenderer renderer, Sprite sprite, float width)
    {
        renderer.sprite = sprite;
        var scale = width / Math.Max(0.001f, sprite.bounds.size.x);
        renderer.transform.localScale = new Vector3(scale, scale, 1f);
    }

    private static void FitHeight(SpriteRenderer renderer, Sprite sprite, float height)
    {
        renderer.sprite = sprite;
        var scale = height / Math.Max(0.001f, sprite.bounds.size.y);
        renderer.transform.localScale = new Vector3(scale, scale, 1f);
    }

    private static Sprite GetCircleSprite()
    {
        if (_circleSprite)
        {
            return _circleSprite!;
        }

        const int size = 64;
        var texture = new Texture2D(size, size, TextureFormat.RGBA32, false) { filterMode = FilterMode.Bilinear };
        var centre = (size - 1) / 2f;
        for (var y = 0; y < size; y++)
        {
            for (var x = 0; x < size; x++)
            {
                var distance = Mathf.Sqrt((x - centre) * (x - centre) + (y - centre) * (y - centre));
                var alpha = Mathf.Clamp01(size / 2f - distance);
                texture.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
            }
        }

        texture.Apply();
        _circleSprite = Sprite.Create(texture, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f), size);
        _circleSprite.hideFlags = HideFlags.HideAndDontSave | HideFlags.DontUnloadUnusedAsset;
        return _circleSprite;
    }

    private static Sprite GetCheckSprite()
    {
        if (_checkSprite)
        {
            return _checkSprite!;
        }

        const int size = 64;
        var texture = new Texture2D(size, size, TextureFormat.RGBA32, false) { filterMode = FilterMode.Bilinear };
        for (var y = 0; y < size; y++)
        {
            for (var x = 0; x < size; x++)
            {
                var first = DistanceToSegment(x, y, 14f, 34f, 26f, 22f);
                var second = DistanceToSegment(x, y, 26f, 22f, 49f, 46f);
                var alpha = Mathf.Clamp01(4f - Math.Min(first, second));
                texture.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
            }
        }

        texture.Apply();
        _checkSprite = Sprite.Create(texture, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f), size);
        _checkSprite.hideFlags = HideFlags.HideAndDontSave | HideFlags.DontUnloadUnusedAsset;
        return _checkSprite;
    }

    private static Sprite? _checkBadgeSprite;

    /// <summary>
    /// A green circle with a white rim and a white tick, drawn into one sprite (one unit across).
    /// </summary>
    private static Sprite GetCheckBadgeSprite()
    {
        if (_checkBadgeSprite)
        {
            return _checkBadgeSprite!;
        }

        const int size = 128;
        const float radius = size / 2f;
        var texture = new Texture2D(size, size, TextureFormat.RGBA32, false) { filterMode = FilterMode.Bilinear };
        var centre = (size - 1) / 2f;
        var outline = new Color(0.07f, 0.08f, 0.09f, 1f);
        for (var y = 0; y < size; y++)
        {
            for (var x = 0; x < size; x++)
            {
                var distance = Mathf.Sqrt((x - centre) * (x - centre) + (y - centre) * (y - centre));
                var outer = Mathf.Clamp01(radius - distance);
                var rim = Mathf.Clamp01(distance - radius * 0.82f + 0.5f);
                var tick = Mathf.Clamp01(8f - Math.Min(
                    DistanceToSegment(x, y, 34f, 66f, 56f, 44f),
                    DistanceToSegment(x, y, 56f, 44f, 96f, 88f)));
                var colour = Color.Lerp(Color.white, outline, rim);
                colour = Color.Lerp(colour, SelectedColor, tick * (1f - rim));
                texture.SetPixel(x, y, new Color(colour.r, colour.g, colour.b, outer));
            }
        }

        texture.Apply();
        _checkBadgeSprite = Sprite.Create(texture, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f), size);
        _checkBadgeSprite.hideFlags = HideFlags.HideAndDontSave | HideFlags.DontUnloadUnusedAsset;
        return _checkBadgeSprite;
    }

    private static readonly Dictionary<string, Sprite> RoundedSprites = new();

    /// <summary>
    /// A rounded rectangle with an optional border, sized so that it is exactly <paramref name="units" /> wide in the world.
    /// </summary>
    private static Sprite GetRoundedRectSprite(string key, float units, float unitsHeight, int pixelWidth, float radius,
        float border, Color fill, Color borderColor)
    {
        if (RoundedSprites.TryGetValue(key, out var cached) && cached)
        {
            return cached;
        }

        var pixelHeight = Math.Max(2, Mathf.RoundToInt(pixelWidth * unitsHeight / units));
        var texture = new Texture2D(pixelWidth, pixelHeight, TextureFormat.RGBA32, false) { filterMode = FilterMode.Bilinear };
        var halfWidth = pixelWidth / 2f;
        var halfHeight = pixelHeight / 2f;
        var corner = Math.Min(radius, Math.Min(halfWidth, halfHeight));
        for (var y = 0; y < pixelHeight; y++)
        {
            for (var x = 0; x < pixelWidth; x++)
            {
                var dx = Math.Max(Math.Abs(x + 0.5f - halfWidth) - (halfWidth - corner), 0f);
                var dy = Math.Max(Math.Abs(y + 0.5f - halfHeight) - (halfHeight - corner), 0f);
                var distance = Mathf.Sqrt(dx * dx + dy * dy) - corner;
                var alpha = Mathf.Clamp01(0.5f - distance);
                var edge = border > 0f ? Mathf.Clamp01(distance + border + 0.5f) : 0f;
                var colour = Color.Lerp(fill, borderColor, edge);
                texture.SetPixel(x, y, new Color(colour.r, colour.g, colour.b, colour.a * alpha));
            }
        }

        texture.Apply();
        var sprite = Sprite.Create(texture, new Rect(0f, 0f, pixelWidth, pixelHeight), new Vector2(0.5f, 0.5f), pixelWidth / units);
        sprite.hideFlags = HideFlags.HideAndDontSave | HideFlags.DontUnloadUnusedAsset;
        RoundedSprites[key] = sprite;
        return sprite;
    }

    private static float DistanceToSegment(float px, float py, float ax, float ay, float bx, float by)
    {
        var abx = bx - ax;
        var aby = by - ay;
        var t = Mathf.Clamp01(((px - ax) * abx + (py - ay) * aby) / (abx * abx + aby * aby));
        var dx = px - (ax + t * abx);
        var dy = py - (ay + t * aby);
        return Mathf.Sqrt(dx * dx + dy * dy);
    }

    private static void ClearPickerMasks(GameObject root)
    {
        foreach (var renderer in root.GetComponentsInChildren<SpriteRenderer>(true))
        {
            renderer.maskInteraction = SpriteMaskInteraction.None;
            renderer.material.SetInt(PlayerMaterial.MaskLayer, 0);
            renderer.sortingOrder += PickerSortingOrder;
        }

        foreach (var text in root.GetComponentsInChildren<TextMeshPro>(true))
        {
            var material = text.fontMaterial;
            if (material)
            {
                material.SetFloat(ShaderID.StencilComp, 8f);
                material.SetFloat(ShaderID.Stencil, 0f);
            }

            var mesh = text.GetComponent<MeshRenderer>();
            if (mesh)
            {
                mesh!.sortingOrder += PickerSortingOrder;
            }
        }
    }

    private static void SetHovered(Category? category)
    {
        _hovered = category;
        _visualsDirty = true;
    }

    private static void ApplyGeometry(RolesSettingsMenu menu, bool withPicker)
    {
        var scroller = menu.scrollBar.transform;
        var maskBg = scroller.Find("MaskBg");
        var hitbox = scroller.Find("Hitbox");
        if (maskBg == null || hitbox == null)
        {
            return;
        }

        scroller.localPosition = withPicker ? PickerScrollerPosition : ModScrollerPosition;
        maskBg.localPosition = withPicker ? PickerMaskPosition : ModMaskPosition;
        maskBg.localScale = withPicker ? PickerMaskScale : ModMaskScale;
        hitbox.localPosition = withPicker ? PickerHitboxPosition : ModHitboxPosition;
        hitbox.localScale = withPicker ? PickerHitboxScale : ModHitboxScale;

        if (withPicker)
        {
            FitHitboxToMask(maskBg, hitbox);
        }
    }

    /// <summary>
    /// The scroller's click area is what lets list rows be clicked. MiraAPI's one extends above the visible list, which
    /// would leave scrolled-away rows clickable underneath the picker, so it is resized to match the mask exactly.
    /// </summary>
    private static void FitHitboxToMask(Transform maskBg, Transform hitbox)
    {
        var maskRenderer = maskBg.GetComponent<Renderer>();
        var box = hitbox.GetComponent<BoxCollider2D>();
        if (!maskRenderer || !box || box.size.y <= 0f || !hitbox.parent)
        {
            return;
        }

        var mask = maskRenderer.bounds;
        var parentScale = hitbox.parent.lossyScale.y;
        if (parentScale <= 0f)
        {
            return;
        }

        var scale = hitbox.localScale;
        hitbox.localScale = new Vector3(scale.x, mask.size.y / (box.size.y * parentScale), scale.z);

        var centre = hitbox.position.y + box.offset.y * hitbox.lossyScale.y;
        hitbox.position += new Vector3(0f, mask.center.y - centre, 0f);
    }

    private static void ApplyVisuals()
    {
        _visualsDirty = false;

        SetPreview(_hovered ?? _selected ?? Category.Crewmate);
        foreach (var button in PickerButtons.Where(button => button.Object && button.Button))
        {
            var selected = _selected == button.Category;
            var scale = button.BaseScale * (_hovered == button.Category ? 1.06f : 1f);
            button.Object.transform.localScale = scale;
            button.Fill.color = selected ? SelectedColor : button.BaseColor;
            button.Check.enabled = selected;
        }
    }

    /// <summary>
    /// The first time the list is shown, selects the first team that actually has roles in this mod.
    /// </summary>
    private static void TryInitialSelect(RolesSettingsMenu menu)
    {
        var hidden = GetGroupHidden();
        var finished = GetMenuDictionary("FinishedRoleMenus");
        var modIdx = MenuState.Instance.CurrentModIdx;
        if (hidden is null || finished is null || !finished.TryGetValue(modIdx, out var done) || !done)
        {
            return;
        }

        _needsInitialSelect = false;

        foreach (var category in TeamCategories)
        {
            if (!hidden.ContainsKey(TeamGroup(category)))
            {
                continue;
            }

            _selected = category;
            _visualsDirty = true;
            Rebuild(menu);
            return;
        }
    }

    private static void OnCategoryClicked(RolesSettingsMenu menu, Category category)
    {
        _visualsDirty = true;
        ClearDescription();

        if (_selected == category)
        {
            return;
        }

        var previous = _selected;
        _selected = category;
        if (!Rebuild(menu))
        {
            _selected = previous;
        }
    }

    /// <summary>
    /// Does what MiraAPI does when a group header is clicked, with the open/closed state chosen by the active category.
    /// </summary>
    private static bool Rebuild(RolesSettingsMenu menu)
    {
        var hidden = GetGroupHidden();
        var finished = GetMenuDictionary("FinishedRoleMenus");
        var queued = GetMenuDictionary("QueuedRoleMenuRefresh");
        if (hidden is null || finished is null || queued is null)
        {
            Fail(menu, "Could not reach MiraAPI's role menu state, disabling the role category picker.");
            return false;
        }

        if (_selected is not { } selected || !hidden.ContainsKey(TeamGroup(selected)))
        {
            // This mod has no roles on that team, so there is nothing to show.
            return false;
        }

        foreach (var group in hidden.Keys.ToArray())
        {
            hidden[group] = group != TeamGroup(selected);
        }

        var modIdx = MenuState.Instance.CurrentModIdx;
        queued[modIdx] = false;
        finished[modIdx] = false;

        foreach (var child in menu.RoleChancesSettings.transform)
        {
            Object.Destroy(child.Cast<Transform>().gameObject);
        }

        HideList(menu);
        _nextFilterCheck = 0f;
        menu.OpenChancesTab();
        return true;
    }

    /// <summary>
    /// Once MiraAPI has finished rebuilding the list, hides the collapsed group headers and closes the gap they leave.
    /// </summary>
    private static void ApplyFilter(RolesSettingsMenu menu)
    {
        if (_selected is null)
        {
            if (!_needsInitialSelect)
            {
                // This mod has no roles to select, so there is nothing to filter.
                ShowList();
            }

            return;
        }

        var finished = GetMenuDictionary("FinishedRoleMenus");
        var modIdx = MenuState.Instance.CurrentModIdx;
        if (finished is null || !finished.TryGetValue(modIdx, out var done) || !done)
        {
            // Still rebuilding. Filtering a half-built list would hide headers whose rows haven't been created yet.
            return;
        }

        var container = menu.RoleChancesSettings.transform;
        var headers = container.GetComponentsInChildren<CategoryHeaderMasked>(true)
            .OrderByDescending(header => header.transform.localPosition.y)
            .ToList();
        if (headers.Count == 0)
        {
            ShowList();
            return;
        }

        // MiraAPI creates every header active and we deactivate the collapsed ones, so an inactive header means this
        // layout has already been filtered.
        if (headers.Any(header => !header.gameObject.activeSelf))
        {
            ShowList();
            return;
        }

        var rows = container.GetComponentsInChildren<RoleOptionSetting>(true);
        var collapsed = new List<CategoryHeaderMasked>();
        for (var i = 0; i < headers.Count; i++)
        {
            var top = headers[i].transform.localPosition.y;
            var bottom = i + 1 < headers.Count ? headers[i + 1].transform.localPosition.y : float.NegativeInfinity;
            var hasRows = rows.Any(row =>
            {
                var y = row.transform.localPosition.y;
                return y < top && y > bottom;
            });

            if (!hasRows)
            {
                collapsed.Add(headers[i]);
            }
        }

        // Nothing to hide, or everything is collapsed (keep the headers so the user can still open one).
        if (collapsed.Count == 0 || collapsed.Count == headers.Count)
        {
            ShowList();
            return;
        }

        var hiddenHeights = collapsed.Select(header => header.transform.localPosition.y).ToList();
        foreach (var header in collapsed)
        {
            header.gameObject.SetActive(false);
        }

        var remaining = headers.Where(header => !collapsed.Contains(header)).Select(header => header.transform)
            .Concat(rows.Select(row => row.transform));
        foreach (var child in remaining)
        {
            var position = child.localPosition;
            var gaps = hiddenHeights.Count(y => y > position.y);
            if (gaps > 0)
            {
                child.localPosition = new Vector3(position.x, position.y + gaps * CollapsedHeaderHeight, position.z);
            }
        }

        menu.scrollBar.ScrollToTop();
        ShowList();
    }

    /// <summary>
    /// The list's text and icons are not all clipped by the scroller's mask, so things scrolled above the top of the list
    /// would show up behind the picker. Anything that has left the top of the list is switched off until it scrolls back.
    /// </summary>
    private static void HideLeaks(RolesSettingsMenu menu)
    {
        var maskBg = menu.scrollBar.transform.Find("MaskBg");
        var maskRenderer = maskBg ? maskBg.GetComponent<Renderer>() : null;
        if (!maskRenderer)
        {
            return;
        }

        var top = maskRenderer!.bounds.max.y;
        var scrolled = menu.scrollBar.Inner ? menu.scrollBar.Inner : menu.RoleChancesSettings.transform;
        foreach (var renderer in scrolled.GetComponentsInChildren<Renderer>(false))
        {
            if (renderer.TryCast<SpriteMask>())
            {
                continue;
            }

            // Sprites that the mask does clip only need hiding once they are completely above it, everything else
            // as soon as it starts to stick out of the top.
            var sprite = renderer.TryCast<SpriteRenderer>();
            var clipped = sprite && sprite!.maskInteraction != SpriteMaskInteraction.None;
            var bounds = renderer.bounds;
            var outside = clipped ? bounds.min.y > top : bounds.max.y > top + LeakMargin;

            var id = renderer.GetInstanceID();
            if (outside)
            {
                if (renderer.enabled)
                {
                    renderer.enabled = false;
                    LeakHidden[id] = renderer;
                }
            }
            else if (LeakHidden.Remove(id))
            {
                renderer.enabled = true;
            }
        }
    }

    private static void RestoreLeaks()
    {
        foreach (var renderer in LeakHidden.Values)
        {
            if (renderer)
            {
                renderer.enabled = true;
            }
        }

        LeakHidden.Clear();
    }

    private static void HideList(RolesSettingsMenu menu)
    {
        _hideDeadline = Time.unscaledTime + HideTimeout;
        if (_hiddenList)
        {
            return;
        }

        var list = menu.RoleChancesSettings.transform;
        _hiddenListScale = list.localScale;
        _hiddenList = list;
        list.localScale = new Vector3(0.001f, 0.001f, _hiddenListScale.z);
    }

    private static void ShowList()
    {
        if (_hiddenList)
        {
            _hiddenList!.localScale = _hiddenListScale;
        }

        _hiddenList = null;
    }

    /// <summary>
    /// Sizes the scroll range from the rows that are actually shown, so that the last row can always be scrolled into view.
    /// </summary>
    private static void EnsureScrollBounds(RolesSettingsMenu menu)
    {
        var finished = GetMenuDictionary("FinishedRoleMenus");
        var modIdx = MenuState.Instance.CurrentModIdx;
        if (finished is null || !finished.TryGetValue(modIdx, out var done) || !done)
        {
            return;
        }

        var container = menu.RoleChancesSettings.transform;
        var lowest = float.MaxValue;
        foreach (var row in container.GetComponentsInChildren<RoleOptionSetting>(false))
        {
            lowest = Math.Min(lowest, row.transform.localPosition.y);
        }

        foreach (var header in container.GetComponentsInChildren<CategoryHeaderMasked>(false))
        {
            lowest = Math.Min(lowest, header.transform.localPosition.y);
        }

        if (lowest == float.MaxValue)
        {
            return;
        }

        // How far the list has to scroll for its last row to reach the bottom of the visible window, measured from the
        // real mask. Short lists don't scroll at all.
        var maskBg = menu.scrollBar.transform.Find("MaskBg");
        var maskRenderer = maskBg ? maskBg.GetComponent<Renderer>() : null;
        if (!maskRenderer || !container.parent)
        {
            return;
        }

        var window = maskRenderer!.bounds;
        var windowBottom = container.parent.InverseTransformPoint(new Vector3(window.center.x, window.min.y, window.center.z)).y;
        var contentBottom = lowest - RowBottomOffset;
        var needed = windowBottom - contentBottom + ScrollBottomPad;
        menu.scrollBar.SetYBoundsMax(needed < MinScroll ? 0f : needed);
    }

    // ---------------------------------------------------------------------------------------------------------------------
    // TOHO-style role list.
    //
    // MiraAPI still builds the list and owns the data: every role has a RoleOptionSetting row holding its count and chance,
    // and changing one goes through MiraAPI's own value-changed handler (which saves it and syncs it to the other players).
    // Here each row is only re-dressed: its original parts are switched off and replaced by a checkbox with the role's name,
    // a "?" button (description) and a "!" button (role settings, which gets Role Count and Chance rows added).
    // ---------------------------------------------------------------------------------------------------------------------

    private const string ToggleName = "LaunchpadRoleToggle";
    private const float RowLabelWidth = 2.4f;  // width of the grey box with the role's name
    private const float RowBoxDarkness = 0.45f; // how dark the faction colour is made for the name boxes
    private const float RowLeftPad = 0.05f;    // gap between the left edge of the list and that box
    private const float InfoButtonOffset = 0.42f;     // distance from the checkbox to the "?" button
    private const float SettingsButtonOffset = 0.80f; // distance from the checkbox to the "!" button
    private const float DetailRowHeight = 0.45f;

    private sealed class SkinnedRow
    {
        public RoleOptionSetting Row = null!;
        public ICustomRole Role = null!;
        public ToggleOption Toggle = null!;
        public Color NameColor;
        public string Name = string.Empty;
    }

    private static readonly List<SkinnedRow> SkinnedRows = [];
    private static readonly HashSet<int> SkinnedHeaders = [];
    private static bool _descriptionShown;
    private static ICustomRole? _descriptionRole;

    // The game's option widgets look their text up by string id, and show "STRMISS" for an id that has no text, so every
    // text used here gets a registered id of its own.
    private static readonly Dictionary<string, StringNames> LiteralStrings = [];

    private static StringNames Literal(string text)
    {
        if (!LiteralStrings.TryGetValue(text, out var id))
        {
            id = CustomStringName.CreateAndRegister(text);
            LiteralStrings[text] = id;
        }

        return id;
    }

    private static readonly MethodInfo? ChangeTabMethod = AccessTools.Method(
        typeof(RoleSettingMenuPatches), "ChangeTab", [typeof(RoleBehaviour), typeof(RolesSettingsMenu)]);
    private static readonly MethodInfo? ValueChangedMethod = AccessTools.Method(
        typeof(RoleSettingMenuPatches), "ValueChanged", [typeof(OptionBehaviour)]);
    private static readonly PropertyInfo? CurrentRoleProperty = AccessTools.Property(
        typeof(RoleSettingMenuPatches), "CurrentRole");

    // The Role Count / Chance rows on the settings page of a role.
    private static NumberOption? _detailCount;
    private static NumberOption? _detailChance;
    private static RoleOptionSetting? _detailRow;

    private static void SkinList(RolesSettingsMenu menu)
    {
        var finished = GetMenuDictionary("FinishedRoleMenus");
        if (finished is null || !finished.TryGetValue(MenuState.Instance.CurrentModIdx, out var done) || !done)
        {
            return;
        }

        SkinnedRows.RemoveAll(skinned => !skinned.Row || !skinned.Toggle);

        var list = menu.RoleChancesSettings.transform;

        // Positions are measured in world space, so the list has to be at its real size while it is restyled (it is
        // shrunk while hidden). It is shrunk again before anything is drawn.
        var hidden = _hiddenList;
        if (hidden)
        {
            hidden!.localScale = _hiddenListScale;
        }

        try
        {
            foreach (var header in list.GetComponentsInChildren<CategoryHeaderMasked>(true))
            {
                SkinHeader(header);
            }

            var restyled = 0;
            foreach (var row in list.GetComponentsInChildren<RoleOptionSetting>(true))
            {
                try
                {
                    if (SkinRow(menu, row))
                    {
                        restyled++;
                    }
                }
                catch (Exception e)
                {
                    // A row that can't be restyled simply keeps MiraAPI's look.
                    Warning($"[RoleCategorySwitcher] Could not restyle a role row: {e}");
                }
            }

            if (restyled > 0)
            {
                Warning($"[RoleCategorySwitcher] Restyled {restyled} role rows.");
            }
        }
        finally
        {
            if (hidden)
            {
                hidden!.localScale = new Vector3(0.001f, 0.001f, _hiddenListScale.z);
            }
        }
    }

    /// <summary>
    /// Group headers lose MiraAPI's "# / chance" labels and the "(click to close)" text, and stop collapsing when clicked
    /// (the picker is what chooses the group now).
    /// </summary>
    private static void SkinHeader(CategoryHeaderMasked header)
    {
        if (!SkinnedHeaders.Add(header.GetInstanceID()))
        {
            return;
        }

        header.GetComponent<PassiveButton>()?.Destroy();
        header.GetComponent<BoxCollider2D>()?.Destroy();

        var children = new List<GameObject>();
        foreach (var child in header.transform)
        {
            children.Add(child.Cast<Transform>().gameObject);
        }

        foreach (var child in children)
        {
            if (child == header.Title.gameObject)
            {
                continue;
            }

            if (child.name.StartsWith("QuotaHeader") || child.GetComponent<TextMeshPro>())
            {
                child.SetActive(false);
            }
        }
    }

    private static bool SkinRow(RolesSettingsMenu menu, RoleOptionSetting row)
    {
        var role = row.Role as ICustomRole;
        if (role is null || role.Configuration.HideSettings || row.transform.Find(ToggleName))
        {
            return false;
        }

        // Where MiraAPI's own row is: the new row takes its place.
        var bar = row.labelSprite.bounds;
        var template = row.buttons[0];

        var originals = new List<GameObject>();
        foreach (var child in row.transform)
        {
            originals.Add(child.Cast<Transform>().gameObject);
        }

        var skinned = new SkinnedRow
        {
            Row = row,
            Role = role,
            Name = row.titleText.text,
            NameColor = row.titleText.color,
        };

        // The checkbox with the grey name box.
        var data = ScriptableObject.CreateInstance<CheckboxGameSetting>();
        data.Type = global::OptionTypes.Checkbox;
        data.Title = Literal(skinned.Name);

        var toggle = Object.Instantiate(menu.checkboxOrigin, row.transform);
        toggle.name = ToggleName;
        toggle.SetUpFromData(data, 20);
        toggle.Title = Literal(skinned.Name);
        toggle.GetComponentInChildren<TextTranslatorTMP>(true)?.Destroy();
        toggle.TitleText.text = skinned.Name;
        toggle.TitleText.color = Color.white;
        toggle.TitleText.fontStyle = FontStyles.Bold;
        toggle.SetClickMask(menu.ButtonClickMask);
        toggle.transform.localPosition = new Vector3(0f, 0f, -0.5f);
        toggle.gameObject.SetActive(true);

        var label = toggle.LabelBackground;
        if (label)
        {
            label.enabled = true;

            // The box is brought to the same width for every row, keeping its right edge (where the name is aligned to)
            // where it was.
            if (label.drawMode != SpriteDrawMode.Simple)
            {
                var delta = label.size.x - RowLabelWidth;
                label.size = new Vector2(RowLabelWidth, label.size.y);
                label.transform.localPosition += new Vector3(delta / 2f, 0f, 0f);
            }
            else if (label.sprite)
            {
                var scale = label.transform.localScale;
                var width = label.sprite.bounds.size.x * scale.x;
                if (width > 0f)
                {
                    label.transform.localScale = new Vector3(scale.x * RowLabelWidth / width, scale.y, scale.z);
                    label.transform.localPosition += new Vector3((width - RowLabelWidth) / 2f, 0f, 0f);
                }
            }

            label.color = BoxColor(label.color, role);

            // Line the box up with the left edge and the middle of MiraAPI's bar.
            var box = label.bounds;
            toggle.transform.position += new Vector3(bar.min.x - box.min.x + RowLeftPad, bar.center.y - box.center.y, 0f);
        }

        ApplyMasks(toggle);
        toggle.CheckMark.enabled = IsRoleActive(row, role);
        toggle.OnValueChanged = (Il2CppSystem.Action<OptionBehaviour>)OnToggleChanged;
        skinned.Toggle = toggle;

        // The "?" and "!" buttons, to the right of the checkbox.
        var checkbox = row.transform.InverseTransformPoint(toggle.CheckMark.transform.position);
        var middle = row.transform.InverseTransformPoint(label ? label.bounds.center : toggle.transform.position).y;
        CreateRowButton(template, row.transform, "?", new Vector3(checkbox.x + InfoButtonOffset, middle, -0.5f), skinned, true);
        CreateRowButton(template, row.transform, "!", new Vector3(checkbox.x + SettingsButtonOffset, middle, -0.5f), skinned, false);

        foreach (var original in originals)
        {
            original.SetActive(false);
        }

        SkinnedRows.Add(skinned);
        return true;
    }

    /// <summary>
    /// The name box takes a darker shade of its faction's colour (the same colours as the group headers).
    /// </summary>
    private static Color BoxColor(Color current, ICustomRole role)
    {
        var faction = role.Team switch
        {
            ModdedRoleTeams.Crewmate => Palette.CrewmateRoleHeaderBlue,
            ModdedRoleTeams.Impostor => Palette.ImpostorRoleHeaderRed,
            _ => role.RoleOptionsGroup.Color,
        };

        var dark = new Color(faction.r * RowBoxDarkness, faction.g * RowBoxDarkness, faction.b * RowBoxDarkness, 1f);

        // If the box is a grey sprite tinted white, the tint has to make up for the sprite's own grey (about 0.28).
        const float spriteGrey = 0.28f;
        var tintedWhite = current.r > 0.9f && current.g > 0.9f && current.b > 0.9f;
        return tintedWhite
            ? new Color(Mathf.Min(1f, dark.r / spriteGrey), Mathf.Min(1f, dark.g / spriteGrey), Mathf.Min(1f, dark.b / spriteGrey), 1f)
            : dark;
    }

    /// <summary>
    /// Makes sprites and text clip against the scroller's mask, the same way MiraAPI sets up the options on the role pages.
    /// Without this the checkbox, name and buttons are not cut off at the edges of the list while scrolling.
    /// </summary>
    private static void ApplyMasks(Component root)
    {
        foreach (var renderer in root.GetComponentsInChildren<SpriteRenderer>(true))
        {
            renderer.material.SetInt(PlayerMaterial.MaskLayer, 20);
        }

        foreach (var text in root.GetComponentsInChildren<TextMeshPro>(true))
        {
            var material = text.fontMaterial;
            material.SetFloat(ShaderID.StencilComp, 3f);
            material.SetFloat(ShaderID.Stencil, 20);
        }
    }

    private static void CreateRowButton(PassiveButton template, Transform parent, string text, Vector3 position, SkinnedRow skinned, bool isInfo)
    {
        var button = Object.Instantiate(template, parent);
        button.name = isInfo ? "LaunchpadRoleInfo" : "LaunchpadRoleSettings";
        button.transform.localPosition = position;
        button.gameObject.SetActive(true);

        var caption = button.transform.Find("Text_TMP");
        if (caption)
        {
            caption.GetComponent<TextTranslatorTMP>()?.Destroy();
            var label = caption.GetComponent<TextMeshPro>();
            if (label)
            {
                label.text = text;
            }

            caption.gameObject.SetActive(true);
        }

        ApplyMasks(button);
        button.GetComponent<GameOptionButton>()?.SetInteractable(true);
        button.OnClick = new UnityEngine.UI.Button.ButtonClickedEvent();
        if (isInfo)
        {
            button.OnClick.AddListener((UnityAction)(() => ShowDescription(skinned)));
        }
        else
        {
            button.OnClick.AddListener((UnityAction)(() => OpenRoleSettings(skinned)));
        }
    }

    private static void ShowDescription(SkinnedRow skinned)
    {
        var settings = GameSettingMenu.Instance;
        if (!settings || !settings.MenuDescriptionText)
        {
            return;
        }

        // Pressing "?" again on the same role hides the description.
        if (_descriptionShown && _descriptionRole == skinned.Role)
        {
            ClearDescription();
            return;
        }

        _descriptionShown = true;
        _descriptionRole = skinned.Role;
        settings.MenuDescriptionText.text = $"<color=#FFFFFF><b>{skinned.Name}</b></color>: {MenuDescription(skinned.Role)}";
    }

    /// <summary>
    /// The text for the "?" button. Each role can have a description of its own for this menu, in the locale file under
    /// the role's key with the suffix ".MenuRoleDesc" (like ".MedDescription"). Roles without one use their MedDescription.
    /// </summary>
    private static string MenuDescription(ICustomRole role)
    {
        var key = MiraLocaleManager.BuildTranslationId(role.IdPrefix, role.IdPart, "MenuRoleDesc");
        var text = MiraLocaleManager.Get(key, string.Empty);
        return string.IsNullOrWhiteSpace(text) || text == key ? role.RoleMedDescription : text;
    }

    private static void ClearDescription()
    {
        var settings = GameSettingMenu.Instance;
        if (!_descriptionShown || !settings || !settings.MenuDescriptionText)
        {
            return;
        }

        _descriptionShown = false;
        _descriptionRole = null;
        settings.MenuDescriptionText.text = TranslationController.Instance.GetString(StringNames.RoleSettingsDescription);
    }

    private static void OpenRoleSettings(SkinnedRow skinned)
    {
        var menu = GameSettingMenu.Instance ? GameSettingMenu.Instance.RoleSettingsTab : null;
        if (!menu || ChangeTabMethod is null)
        {
            return;
        }

        try
        {
            ChangeTabMethod.Invoke(null, [skinned.Row.Role, menu]);
        }
        catch (Exception e)
        {
            Warning($"[RoleCategorySwitcher] Could not open the role settings: {e}");
        }
    }

    private static bool IsRoleActive(RoleOptionSetting row, ICustomRole role)
    {
        var configuration = role.Configuration;
        var hasCount = configuration.MaxRoleCount == 0 || row.RoleMaxCount > 0;
        var hasChance = !configuration.CanModifyChance || row.RoleChance > 0;
        return hasCount && hasChance;
    }

    /// <summary>
    /// The checkbox switches a role on or off: on gives it a count of at least 1 and a chance above 0 (100 if it had none),
    /// off sets its count to 0. The chance itself is edited on the role's settings page.
    /// </summary>
    private static void OnToggleChanged(OptionBehaviour option)
    {
        var skinned = SkinnedRows.FirstOrDefault(entry => entry.Toggle && entry.Toggle.Pointer == option.Pointer);
        if (skinned is null)
        {
            return;
        }

        if (AmongUsClient.Instance && !AmongUsClient.Instance.AmHost)
        {
            RefreshToggle(skinned);
            return;
        }

        var row = skinned.Row;
        var configuration = skinned.Role.Configuration;
        var countEditable = configuration.MaxRoleCount != 0;
        var chanceEditable = configuration.CanModifyChance;

        if (skinned.Toggle.GetBool())
        {
            if (countEditable && row.RoleMaxCount < 1)
            {
                row.roleMaxCount = 1;
            }

            if (chanceEditable && row.RoleChance < 1)
            {
                row.roleChance = 100;
            }
        }
        else if (countEditable)
        {
            row.roleMaxCount = 0;
        }
        else if (chanceEditable)
        {
            row.roleChance = 0;
        }

        NotifyRoleChanged(row);
        RefreshToggle(skinned);
    }

    /// <summary>
    /// Hands a changed count or chance to MiraAPI, which saves it, syncs it to the other players and refreshes the row.
    /// </summary>
    private static void NotifyRoleChanged(RoleOptionSetting row)
    {
        try
        {
            ValueChangedMethod?.Invoke(null, [row]);
        }
        catch (Exception e)
        {
            Warning($"[RoleCategorySwitcher] Could not apply a role change: {e}");
        }
    }

    private static void RefreshToggles()
    {
        foreach (var skinned in SkinnedRows.Where(entry => entry.Row && entry.Toggle))
        {
            RefreshToggle(skinned);
        }
    }

    private static void RefreshToggle(SkinnedRow skinned)
    {
        var checkMark = skinned.Toggle.CheckMark;
        if (checkMark && !LeakHidden.ContainsKey(checkMark.GetInstanceID()))
        {
            checkMark.enabled = IsRoleActive(skinned.Row, skinned.Role);
        }
    }

    // ---- The settings page of a role: Role Count and Chance ------------------------------------------------------------

    private const float ScreenshotInset = 0.08f;   // gap between the screenshot and its frame
    private const float MaxFrameWidth = 4.5f;      // a "frame" wider than this is not really the frame, so it is left alone
    private static bool _screenshotLogged;

    /// <summary>
    /// MiraAPI shows the role's screenshot at the size of the picture, not the size of the frame behind it, so a larger
    /// picture covers the frame and runs off the left edge of the page. It is scaled down to fit inside the frame.
    /// </summary>
    private static void FitScreenshot(RolesSettingsMenu menu)
    {
        var shot = menu.roleScreenshot;
        if (!shot || !shot.sprite || !shot.gameObject.activeInHierarchy)
        {
            return;
        }

        var frameObject = menu.AdvancedRolesSettings.transform.Find("Imagebackground");
        var frame = frameObject ? frameObject.GetComponent<Renderer>() : null;
        if (!frame || !frame!.gameObject.activeInHierarchy)
        {
            return;
        }

        var area = frame.bounds;
        if (!_screenshotLogged)
        {
            _screenshotLogged = true;
            Warning($"[RoleCategorySwitcher] Screenshot frame: size={area.size} centre={area.center}, picture size={shot.bounds.size}");
        }

        if (area.size.x > MaxFrameWidth || area.size.x <= 0.1f || area.size.y <= 0.1f)
        {
            return;
        }

        var picture = shot.bounds.size;
        if (picture.x <= 0f || picture.y <= 0f)
        {
            return;
        }

        var factor = Mathf.Min((area.size.x - 2f * ScreenshotInset) / picture.x, (area.size.y - 2f * ScreenshotInset) / picture.y);
        if (Mathf.Abs(factor - 1f) > 0.001f)
        {
            var scale = shot.transform.localScale;
            shot.transform.localScale = new Vector3(scale.x * factor, scale.y * factor, scale.z);
        }

        var offset = area.center - shot.bounds.center;
        shot.transform.position += new Vector3(offset.x, offset.y, 0f);
    }

    private static void UpdateDetails(RolesSettingsMenu menu)
    {
        FitScreenshot(menu);

        var role = CurrentRoleProperty?.GetValue(null) as RoleBehaviour;
        var customRole = role as ICustomRole;
        if (!role || customRole is null)
        {
            return;
        }

        var row = _detailRow && _detailRow!.Role == role
            ? _detailRow
            : menu.RoleChancesSettings.transform.GetComponentsInChildren<RoleOptionSetting>(true)
                .FirstOrDefault(entry => entry.Role == role);
        if (!row)
        {
            return;
        }

        _detailRow = row;

        var configuration = customRole.Configuration;
        var countEditable = configuration.MaxRoleCount != 0;
        var chanceEditable = configuration.CanModifyChance;

        // MiraAPI destroys the options of the page whenever it opens a role, so these are simply made again when missing.
        if (countEditable && !_detailCount)
        {
            _detailCount = CreateNumberRow(menu, "Role Count", row!.RoleMaxCount, 0f, configuration.MaxRoleCount, 1f, false);
        }

        if (chanceEditable && !_detailChance)
        {
            _detailChance = CreateNumberRow(menu, "Chance", row!.RoleChance, 0f, 100f, 10f, true);
        }

        var hasImage = role!.RoleScreenshot != null;
        var start = hasImage ? -0.872f : -1f;
        var x = hasImage ? 2.17f : 1.1f;
        var y = start;
        var extra = 0;

        foreach (var option in new[] { countEditable ? _detailCount : null, chanceEditable ? _detailChance : null })
        {
            if (!option)
            {
                continue;
            }

            option!.transform.localPosition = new Vector3(x, y, -2f);
            option.gameObject.SetActive(true);
            y -= DetailRowHeight;
            extra++;
        }

        // MiraAPI has already placed the role's own options from the top, so they are moved down below these rows.
        var visible = 0;
        foreach (var option in menu.advancedSettingChildren)
        {
            if (!option || !option.gameObject.activeSelf)
            {
                continue;
            }

            option.transform.localPosition += new Vector3(0f, -DetailRowHeight * extra, 0f);
            visible++;
        }

        menu.scrollBar.SetYBoundsMax(-(start - DetailRowHeight * (visible + extra)) - 3f);
    }

    private static NumberOption CreateNumberRow(RolesSettingsMenu menu, string title, float value, float min, float max, float increment, bool percent)
    {
        var data = ScriptableObject.CreateInstance<FloatGameSetting>();
        data.Type = global::OptionTypes.Float;
        data.Title = Literal(title);
        data.Value = value;
        data.Increment = increment;
        data.ValidRange = new FloatRange(min, max);
        data.FormatString = "0";
        data.ZeroIsInfinity = false;
        data.SuffixType = (NumberSuffixes)(percent ? MiraNumberSuffixes.Percent : MiraNumberSuffixes.None);
        data.OptionName = FloatOptionNames.Invalid;

        var option = Object.Instantiate(menu.numberOptionOrigin, Vector3.zero, Quaternion.identity, menu.AdvancedRolesSettings.transform);
        option.name = $"LaunchpadRole.{title}";
        option.SetUpFromData(data, 20);
        option.Title = Literal(title);
        option.Value = value;
        option.Increment = increment;
        option.ValidRange = new FloatRange(min, max);
        option.FormatString = "0";
        option.ZeroIsInfinity = false;
        option.SuffixType = data.SuffixType;
        option.floatOptionName = FloatOptionNames.Invalid;
        option.TitleText.GetComponent<TextTranslatorTMP>()?.Destroy();
        option.TitleText.text = title;
        option.OnValueChanged = (Il2CppSystem.Action<OptionBehaviour>)OnDetailValueChanged;
        option.SetClickMask(menu.ButtonClickMask);

        // Same masking as MiraAPI gives the other options on this page.
        foreach (var renderer in option.GetComponentsInChildren<SpriteRenderer>(true))
        {
            renderer.material.SetInt(PlayerMaterial.MaskLayer, 20);
        }

        foreach (var material in option.GetComponentsInChildren<TextMeshPro>(true).Select(text => text.fontMaterial))
        {
            material.SetFloat(ShaderID.StencilComp, 3f);
            material.SetFloat(ShaderID.Stencil, 20);
        }

        option.LabelBackground.enabled = false;
        option.Initialize();
        return option;
    }

    private static void OnDetailValueChanged(OptionBehaviour option)
    {
        if (!_detailRow)
        {
            return;
        }

        if (AmongUsClient.Instance && !AmongUsClient.Instance.AmHost)
        {
            return;
        }

        var number = option.Cast<NumberOption>();
        var value = Mathf.RoundToInt(number.Value);
        if (_detailCount && number.Pointer == _detailCount!.Pointer)
        {
            _detailRow!.roleMaxCount = value;
        }
        else if (_detailChance && number.Pointer == _detailChance!.Pointer)
        {
            _detailRow!.roleChance = value;
        }
        else
        {
            return;
        }

        NotifyRoleChanged(_detailRow!);
    }

    private static RoleOptionsGroup TeamGroup(Category category)
    {
        return category switch
        {
            Category.Crewmate => RoleOptionsGroup.Crewmate,
            Category.Impostor => RoleOptionsGroup.Impostor,
            Category.Coven => LaunchpadRoleGroups.Coven,
            _ => LaunchpadRoleGroups.Neutral,
        };
    }

    private static MiraAPI.Utilities.Assets.LoadableAsset<Sprite> IconFor(Category category)
    {
        return category switch
        {
            Category.Crewmate => LaunchpadAssets.CategoryIconCrewmate,
            Category.Impostor => LaunchpadAssets.CategoryIconImpostor,
            Category.Neutral => LaunchpadAssets.CategoryIconNeutral,
            Category.Coven => LaunchpadAssets.CovenIcon,
            _ => LaunchpadAssets.CategoryIconModifiers,
        };
    }

    private static MiraAPI.Utilities.Assets.LoadableAsset<Sprite> BannerFor(Category category)
    {
        return category switch
        {
            Category.Crewmate => LaunchpadAssets.CategoryBannerCrewmate,
            Category.Impostor => LaunchpadAssets.CategoryBannerImpostor,
            Category.Neutral => LaunchpadAssets.CategoryBannerNeutral,
            Category.Coven => LaunchpadAssets.CategoryBannerCoven,
            _ => LaunchpadAssets.CategoryBannerModifiers,
        };
    }

    private static MiraAPI.Utilities.Assets.LoadableAsset<Sprite> TextFor(Category category)
    {
        return category switch
        {
            Category.Crewmate => LaunchpadAssets.CategoryTextCrewmate,
            Category.Impostor => LaunchpadAssets.CategoryTextImpostor,
            Category.Neutral => LaunchpadAssets.CategoryTextNeutral,
            Category.Coven => LaunchpadAssets.CategoryTextCoven,
            _ => LaunchpadAssets.CategoryTextModifiers,
        };
    }

    // MiraAPI keeps these private/internal, so they are read through reflection (and the feature disables itself if
    // they ever stop being there).
    private static Dictionary<RoleOptionsGroup, bool>? GetGroupHidden()
    {
        return AccessTools.Property(typeof(RoleSettingMenuPatches), "RoleGroupHidden")?.GetValue(null) as Dictionary<RoleOptionsGroup, bool>;
    }

    private static Dictionary<int, bool>? GetMenuDictionary(string propertyName)
    {
        return AccessTools.Property(typeof(MenuState), propertyName)?.GetValue(MenuState.Instance) as Dictionary<int, bool>;
    }
}
