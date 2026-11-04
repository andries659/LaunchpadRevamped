using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;
using Debug = UnityEngine.Debug;
using Il2CppInterop.Runtime;

namespace LaunchpadReloaded.Wiki; // <-- change to your mod's namespace

/// <summary>
/// Loads the wiki prefab from an AssetBundle and drives it.
/// Input is handled manually (mouse/touch hit-tests), so it does NOT need a uGUI EventSystem.
/// </summary>
public static class WikiPanel
{
    // End of the embedded resource name of your bundle file (see README steps).
    private const string BundleResourceSuffix = "wiki.bundle";

    private const float ScrollSpeed = 70f;
    private const float DragThreshold = 14f;

    private static readonly Color RowNormal = new(0.15f, 0.17f, 0.22f, 1f);
    private static readonly Color RowHover = new(0.21f, 0.24f, 0.31f, 1f);
    private static readonly Color CloseNormal = new(0.55f, 0.16f, 0.16f, 1f);
    private static readonly Color CloseHover = new(0.78f, 0.22f, 0.22f, 1f);
    private static readonly Color IconPlaceholder = new(1f, 1f, 1f, 0.12f);

    private class RoleView
    {
        public WikiRole Role;
        public RectTransform Rect;
        public Image Bg;
        public GameObject Highlight;
    }

    private static AssetBundle _bundle;
    private static GameObject _root, _roleButtonPrefab, _abilityRowPrefab;
    private static Canvas _canvas;

    private static RectTransform _closeRect, _listViewport, _listContent, _detailViewport, _detailContent, _abilityContainer;
    private static Image _closeBg, _detailIcon;
    private static GameObject _emptyState, _detailScrollObj, _abilitiesHeader;
    private static TextMeshProUGUI _detailName, _detailTeam, _detailDesc;

    private static readonly List<RoleView> Views = new();
    private static WikiRole _selected;

    private static int _pressArea;          // 0 none, 1 list, 2 detail
    private static bool _pressedClose, _dragged;
    private static Vector2 _pressStart, _lastMouse;

    public static AssetBundle Bundle => _bundle;
    public static bool IsOpen => _root != null && _root.activeSelf;

    // ------------------------------------------------------------------ public API

    public static void Open()
    {
        if (!EnsureLoaded()) return;
        _root.SetActive(true);
        Rebuild();
    }

    public static void Close()
    {
        if (_root != null) _root.SetActive(false);
    }

    public static void Toggle()
    {
        if (IsOpen) Close(); else Open();
    }

    // ------------------------------------------------------------------ loading

    private static bool EnsureLoaded()
    {
        if (_root != null) return true;

        try
        {
            var bytes = ReadResource(BundleResourceSuffix);
            if (bytes == null) return false;

            _bundle = AssetBundle.LoadFromMemory(bytes);
            if (_bundle == null)
            {
                Debug.LogError("[Wiki] AssetBundle.LoadFromMemory returned null");
                return false;
            }

            var prefab = LoadAsset<GameObject>("LaunchpadWiki");
            _roleButtonPrefab = LoadAsset<GameObject>("WikiRoleButton");
            _abilityRowPrefab = LoadAsset<GameObject>("WikiAbilityRow");

            _root = Object.Instantiate(prefab);
            _root.name = "LaunchpadWiki";
            Object.DontDestroyOnLoad(_root);
            _canvas = _root.GetComponent<Canvas>();

            // Close button
            _closeRect = Find("WindowRoot/Window/TitleBar/CloseButton").GetComponent<RectTransform>();
            _closeBg = _closeRect.GetComponent<Image>();
            DestroyButton(_closeRect.gameObject);

            // Role list (left)
            var listScroll = Find("WindowRoot/Window/Body/RoleList/RoleScroll");
            DisableScrollRect(listScroll);
            _listViewport = Find("WindowRoot/Window/Body/RoleList/RoleScroll/Viewport").GetComponent<RectTransform>();
            _listContent = Find("WindowRoot/Window/Body/RoleList/RoleScroll/Viewport/Content").GetComponent<RectTransform>();

            // Detail pane (right)
            _emptyState = Find("WindowRoot/Window/Body/Detail/EmptyState").gameObject;
            var detailScroll = Find("WindowRoot/Window/Body/Detail/DetailScroll");
            _detailScrollObj = detailScroll.gameObject;
            DisableScrollRect(detailScroll);
            _detailViewport = Find("WindowRoot/Window/Body/Detail/DetailScroll/Viewport").GetComponent<RectTransform>();
            const string dc = "WindowRoot/Window/Body/Detail/DetailScroll/Viewport/Content/";
            _detailContent = Find("WindowRoot/Window/Body/Detail/DetailScroll/Viewport/Content").GetComponent<RectTransform>();
            _detailIcon = Find(dc + "Header/Icon").GetComponent<Image>();
            _detailName = Find(dc + "Header/TextBlock/RoleName").GetComponent<TextMeshProUGUI>();
            _detailTeam = Find(dc + "Header/TextBlock/RoleTeam").GetComponent<TextMeshProUGUI>();
            _detailDesc = Find(dc + "Description").GetComponent<TextMeshProUGUI>();
            _abilitiesHeader = Find(dc + "AbilitiesHeader").gameObject;
            _abilityContainer = Find(dc + "AbilityContainer").GetComponent<RectTransform>();

            _root.SetActive(false);
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError("[Wiki] Failed to load wiki UI: " + e);
            if (_root != null) Object.Destroy(_root);
            _root = null;
            return false;
        }
    }

    private static byte[] ReadResource(string suffix)
    {
        var asm = Assembly.GetExecutingAssembly();
        var name = asm.GetManifestResourceNames()
            .FirstOrDefault(n => n.EndsWith(suffix, StringComparison.OrdinalIgnoreCase));

        if (name == null)
        {
            Debug.LogError("[Wiki] No embedded resource ending in '" + suffix + "'. Found: " +
                           string.Join(", ", asm.GetManifestResourceNames()));
            return null;
        }

        using var stream = asm.GetManifestResourceStream(name);
        using var ms = new MemoryStream();
        stream!.CopyTo(ms);
        return ms.ToArray();
    }

    private static T LoadAsset<T>(string assetName) where T : Object
    {
        var asset = _bundle.LoadAsset(assetName, Il2CppType.Of<T>())?.TryCast<T>();
        if (asset == null) throw new Exception("Asset '" + assetName + "' not found in bundle");
        asset.hideFlags |= HideFlags.DontUnloadUnusedAsset;
        return asset;
    }

    private static Transform Find(string path)
    {
        var t = _root.transform.Find(path);
        if (t == null) throw new Exception("Missing UI object: " + path);
        return t;
    }

    private static void DisableScrollRect(Transform t)
    {
        var sr = t.GetComponent<ScrollRect>();
        if (sr != null) sr.enabled = false; // we scroll manually
    }

    private static void DestroyButton(GameObject go)
    {
        var b = go.GetComponent<Button>();
        if (b != null) Object.Destroy(b); // colours are set manually, no EventSystem needed
    }

    // ------------------------------------------------------------------ content

    private static void Rebuild()
    {
        ClearChildren(_listContent);
        Views.Clear();

        foreach (var role in WikiRegistry.Roles)
        {
            var go = Object.Instantiate(_roleButtonPrefab, _listContent);
            go.name = "Role_" + role.Name;
            DestroyButton(go);

            var color = role.Color;
            go.transform.Find("Accent").GetComponent<Image>().color = color;
            SetIcon(go.transform.Find("Icon").GetComponent<Image>(), role.Icon);
            go.transform.Find("Label").GetComponent<TextMeshProUGUI>().text =
                role.Name + "\n<size=60%><color=#" + ColorUtility.ToHtmlStringRGB(color) + ">" + role.Team + "</color></size>";

            var highlight = go.transform.Find("Highlight").gameObject;
            highlight.SetActive(false);

            var bg = go.GetComponent<Image>();
            bg.color = RowNormal;

            Views.Add(new RoleView { Role = role, Rect = go.GetComponent<RectTransform>(), Bg = bg, Highlight = highlight });
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate(_listContent);
        _listContent.anchoredPosition = Vector2.zero;

        if (Views.Count > 0) Select(Views[0].Role);
        else ShowEmpty();
    }

    private static void ShowEmpty()
    {
        _selected = null;
        _emptyState.SetActive(true);
        _detailScrollObj.SetActive(false);
    }

    private static void Select(WikiRole role)
    {
        _selected = role;
        foreach (var v in Views) v.Highlight.SetActive(v.Role == role);

        _emptyState.SetActive(false);
        _detailScrollObj.SetActive(true);

        _detailName.text = role.Name;
        _detailName.color = role.Color;
        _detailTeam.text = role.Team;
        _detailTeam.color = role.Color;
        _detailDesc.text = role.Description;
        SetIcon(_detailIcon, role.Icon);

        ClearChildren(_abilityContainer);
        bool has = role.Abilities != null && role.Abilities.Count > 0;
        _abilitiesHeader.SetActive(has);
        _abilityContainer.gameObject.SetActive(has);

        if (has)
        {
            foreach (var ability in role.Abilities)
            {
                var row = Object.Instantiate(_abilityRowPrefab, _abilityContainer);
                row.name = "Ability_" + ability.Name;

                SetIcon(row.transform.Find("Icon").GetComponent<Image>(), ability.Icon);

                var nameT = row.transform.Find("TextColumn/Header/Name").GetComponent<TextMeshProUGUI>();
                nameT.text = ability.Name;
                nameT.color = role.Color;

                var cd = row.transform.Find("TextColumn/Header/Cooldown").GetComponent<TextMeshProUGUI>();
                bool hasCd = !string.IsNullOrEmpty(ability.Cooldown);
                cd.gameObject.SetActive(hasCd);
                cd.text = ability.Cooldown;

                row.transform.Find("TextColumn/Description").GetComponent<TextMeshProUGUI>().text = ability.Description;
            }
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate(_detailContent);
        _detailContent.anchoredPosition = Vector2.zero;
    }

    private static void SetIcon(Image img, Sprite sprite)
    {
        img.sprite = sprite;
        img.color = sprite != null ? Color.white : IconPlaceholder;
    }

    private static void ClearChildren(Transform parent)
    {
        for (int i = parent.childCount - 1; i >= 0; i--)
        {
            var child = parent.GetChild(i).gameObject;
            child.SetActive(false); // layout ignores it immediately
            Object.Destroy(child);
        }
    }

    // ------------------------------------------------------------------ input (called every frame by WikiRunner)

    public static void Tick()
    {
        if (!IsOpen) return;

        Vector2 mouse = Input.mousePosition;
        bool overList = Hit(_listViewport, mouse);
        bool overDetail = _detailScrollObj.activeSelf && Hit(_detailViewport, mouse);
        float scale = _canvas != null && _canvas.scaleFactor > 0.01f ? _canvas.scaleFactor : 1f;

        // mouse wheel
        float wheel = Input.mouseScrollDelta.y;
        if (wheel != 0f)
        {
            if (overList) ScrollBy(_listViewport, _listContent, -wheel * ScrollSpeed);
            else if (overDetail) ScrollBy(_detailViewport, _detailContent, -wheel * ScrollSpeed);
        }

        // hover colours
        foreach (var v in Views)
            v.Bg.color = overList && Hit(v.Rect, mouse) ? RowHover : RowNormal;
        _closeBg.color = Hit(_closeRect, mouse) ? CloseHover : CloseNormal;

        // press / drag (touch-friendly) / release
        if (Input.GetMouseButtonDown(0))
        {
            _pressArea = overList ? 1 : overDetail ? 2 : 0;
            _pressedClose = Hit(_closeRect, mouse);
            _pressStart = _lastMouse = mouse;
            _dragged = false;
        }
        else if (Input.GetMouseButton(0) && _pressArea != 0)
        {
            float dy = (mouse.y - _lastMouse.y) / scale;
            _lastMouse = mouse;
            if (!_dragged && Mathf.Abs(mouse.y - _pressStart.y) > DragThreshold) _dragged = true;
            if (_dragged)
            {
                if (_pressArea == 1) ScrollBy(_listViewport, _listContent, dy);
                else ScrollBy(_detailViewport, _detailContent, dy);
            }
        }
        else if (Input.GetMouseButtonUp(0))
        {
            if (_pressedClose && Hit(_closeRect, mouse))
            {
                Close();
            }
            else if (_pressArea == 1 && !_dragged && overList)
            {
                foreach (var v in Views)
                {
                    if (Hit(v.Rect, mouse)) { Select(v.Role); break; }
                }
            }
            _pressArea = 0;
            _pressedClose = false;
        }
    }

    private static bool Hit(RectTransform rect, Vector2 screenPoint)
    {
        // Canvas is Screen Space Overlay, so the camera is null.
        return RectTransformUtility.RectangleContainsScreenPoint(rect, screenPoint, null);
    }

    private static void ScrollBy(RectTransform viewport, RectTransform content, float deltaCanvasUnits)
    {
        float max = Mathf.Max(0f, content.rect.height - viewport.rect.height);
        var p = content.anchoredPosition;
        p.y = Mathf.Clamp(p.y + deltaCanvasUnits, 0f, max);
        content.anchoredPosition = p;
    }
}

/// <summary>Helpers for loading icons you can put on WikiRole / WikiAbility.</summary>
public static class WikiAssets
{
    /// <summary>Sprite that lives in the same AssetBundle (call after the wiki was opened once, or load the bundle yourself).</summary>
    public static Sprite FromBundle(string spriteName)
    {
        var s = WikiPanel.Bundle?.LoadAsset(spriteName, Il2CppType.Of<Sprite>())?.TryCast<Sprite>();
        if (s != null) s.hideFlags |= HideFlags.DontUnloadUnusedAsset;
        return s;
    }

    /// <summary>Sprite from a PNG embedded in your dll, e.g. "Resources.Wiki.sniper.png".</summary>
    public static Sprite FromEmbeddedPng(string resourceSuffix, float pixelsPerUnit = 100f)
    {
        var asm = Assembly.GetExecutingAssembly();
        var name = asm.GetManifestResourceNames().FirstOrDefault(n => n.EndsWith(resourceSuffix, StringComparison.OrdinalIgnoreCase));
        if (name == null) { Debug.LogError("[Wiki] Missing embedded png: " + resourceSuffix); return null; }

        using var stream = asm.GetManifestResourceStream(name);
        using var ms = new MemoryStream();
        stream!.CopyTo(ms);

        var tex = new Texture2D(2, 2, TextureFormat.ARGB32, false);
        ImageConversion.LoadImage(tex, ms.ToArray(), false);
        tex.hideFlags |= HideFlags.DontUnloadUnusedAsset;

        var sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), pixelsPerUnit);
        sprite.hideFlags |= HideFlags.DontUnloadUnusedAsset;
        return sprite;
    }
}
