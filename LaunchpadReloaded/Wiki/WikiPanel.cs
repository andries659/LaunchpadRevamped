using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
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

    private static readonly Color RowNormal = new(0.165f, 0.168f, 0.198f, 1f);
    private static readonly Color RowHover = new(0.215f, 0.220f, 0.255f, 1f);
    private static readonly Color RowSelected = new(0.195f, 0.200f, 0.235f, 1f);
    private static readonly Color CloseNormal = new(0.30f, 0.09f, 0.16f, 1f);
    private static readonly Color CloseHover = new(0.86f, 0.20f, 0.36f, 1f);
    private static readonly Color IconPlaceholder = new(1f, 1f, 1f, 0.12f);
    private static readonly Color SearchNormal = new(0.092f, 0.095f, 0.118f, 1f);
    private static readonly Color SearchFocused = new(0.075f, 0.078f, 0.098f, 1f);
    private static readonly Color SearchHint = new(0.55f, 0.57f, 0.64f, 1f);

    private const float SearchBarHeight = 52f;
    private const float SearchMargin = 12f;
    private const float SearchPadLeft = 46f;   // room for the magnifier icon
    private const float SearchPadRight = 52f;   // room for the clear (X) button
    private const int SearchMaxLength = 40;

    // home screen + back button
    private static readonly Color BackNormal = new(0.19f, 0.195f, 0.23f, 1f);
    private static readonly Color BackHover = new(0.26f, 0.265f, 0.31f, 1f);

    private enum Section { Home, Roles, Modifiers, Settings }

    private class HomeCard
    {
        public Section Target;
        public RectTransform Rect;
        public Image Bg, Stroke;
        public Image[] SlotBg, SlotIcon;
        public TextMeshProUGUI Preview, Count;
        public Color Accent;
    }

    private class RoleView
    {
        public WikiRole Role;
        public RectTransform Rect;
        public Image Bg;
        public GameObject Highlight;
        public bool Visible = true;   // false = filtered out by the search
        public string Haystack;       // lower-case text the search matches against
    }

    private static AssetBundle _bundle;
    private static GameObject _root, _roleButtonPrefab, _abilityRowPrefab;
    private static Canvas _canvas;

    private static RectTransform _closeRect, _listViewport, _listContent, _detailViewport, _detailContent, _abilityContainer;
    private static Image _closeBg, _detailIcon;
    private static GameObject _emptyState, _detailScrollObj, _abilitiesHeader;
    private static TextMeshProUGUI _detailName, _detailTeam, _detailDesc;
    private static RectTransform _listBar, _detailBar;   // scroll indicators
    private static Image _searchRing;

    // search bar
    private static RectTransform _searchRect, _clearRect, _caretRect;
    private static Image _searchBg;
    private static TextMeshProUGUI _searchText, _noResultsText;
    private static string _query = "";
    private static bool _searchFocused;
    private static string _placeholder = "Search roles...";

    // sections
    private static Section _section = Section.Home;
    private static bool _homeBuilt;
    private static RectTransform _homeRoot, _backRect;
    private static Image _backBg;
    private static GameObject _roleListGo, _detailGo;
    private static TextMeshProUGUI _titleText, _abilitiesHeaderText;
    private static string _abilitiesHeaderDefault = "";
    private static readonly List<HomeCard> Cards = new();
    private static HomeCard _pressedCard;
    private static bool _pressedBack;

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
        if (_homeBuilt) ShowHome();
        else OpenSection(Section.Roles); // home screen failed to build: behave like the old single page
        WikiTheme.OnOpen();
    }

    public static void Close()
    {
        _searchFocused = false;
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

            _roleListGo = Find("WindowRoot/Window/Body/RoleList").gameObject;
            _detailGo = Find("WindowRoot/Window/Body/Detail").gameObject;
            _titleText = Find("WindowRoot/Window/TitleBar/Title").GetComponent<TextMeshProUGUI>();
            _abilitiesHeaderText = _abilitiesHeader.GetComponentInChildren<TextMeshProUGUI>(true);
            if (_abilitiesHeaderText != null)
            {
                _abilitiesHeaderDefault = _abilitiesHeaderText.text;
                _abilitiesHeaderText.characterSpacing = 0f;
            }

            BuildSearchBar();

            // visual theme (see WikiTheme.cs). If it fails the wiki still works with the plain bundle look.
            try
            {
                WikiTheme.Install(_root.transform, _detailTeam);
                WikiTheme.BuildStatChips(_detailTeam.transform.parent, _detailTeam);
                StyleSearchBar();
                _listBar = WikiTheme.CreateScrollbar(listScroll);
                _detailBar = WikiTheme.CreateScrollbar(detailScroll);
            }
            catch (Exception e)
            {
                Debug.LogError("[Wiki] Theme failed, using the default look: " + e);
            }

            try
            {
                BuildHome();
                BuildBackButton();
                _homeBuilt = true;
            }
            catch (Exception e)
            {
                Debug.LogError("[Wiki] Home screen failed, showing the roles page directly: " + e);
            }

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

    // ------------------------------------------------------------------ extra UI built in code
    // (done here so the existing AssetBundle keeps working without being rebuilt)

    private static RectTransform NewRect(string name, Transform parent)
    {
        var go = new GameObject(name);
        go.layer = parent.gameObject.layer;
        var rt = go.AddComponent<RectTransform>();
        rt.SetParent(parent, false);
        return rt;
    }

    private static void BuildSearchBar()
    {
        var listRoot = Find("WindowRoot/Window/Body/RoleList");
        var scroll = Find("WindowRoot/Window/Body/RoleList/RoleScroll").GetComponent<RectTransform>();

        // push the role list down to make room for the bar
        scroll.offsetMax = new Vector2(scroll.offsetMax.x, -(SearchBarHeight + SearchMargin * 2f));

        _searchRect = NewRect("SearchBar", listRoot);
        _searchRect.anchorMin = new Vector2(0f, 1f);
        _searchRect.anchorMax = new Vector2(1f, 1f);
        _searchRect.pivot = new Vector2(0.5f, 1f);
        _searchRect.anchoredPosition = new Vector2(0f, -SearchMargin);
        _searchRect.sizeDelta = new Vector2(-SearchMargin * 2f, SearchBarHeight);
        _searchBg = _searchRect.gameObject.AddComponent<Image>();
        _searchBg.color = SearchNormal;
        _searchBg.raycastTarget = false;

        // text (also shows the placeholder when empty)
        var textGo = Object.Instantiate(_detailTeam.gameObject, _searchRect);
        textGo.name = "SearchText";
        _searchText = textGo.GetComponent<TextMeshProUGUI>();
        var trt = _searchText.rectTransform;
        trt.anchorMin = Vector2.zero;
        trt.anchorMax = Vector2.one;
        trt.pivot = new Vector2(0f, 0.5f);
        trt.offsetMin = new Vector2(SearchPadLeft, 0f);
        trt.offsetMax = new Vector2(-SearchPadRight, 0f);
        _searchText.fontSize = 24f;
        _searchText.fontStyle = FontStyles.Normal;
        _searchText.alignment = TextAlignmentOptions.MidlineLeft;
        _searchText.richText = false;
        _searchText.enableWordWrapping = false;
        _searchText.overflowMode = TextOverflowModes.Overflow;
        _searchText.raycastTarget = false;

        // caret
        _caretRect = NewRect("Caret", _searchRect);
        _caretRect.anchorMin = _caretRect.anchorMax = new Vector2(0f, 0.5f);
        _caretRect.pivot = new Vector2(0f, 0.5f);
        _caretRect.sizeDelta = new Vector2(2f, SearchBarHeight - 22f);
        var caretImg = _caretRect.gameObject.AddComponent<Image>();
        caretImg.color = Color.white;
        caretImg.raycastTarget = false;

        // clear (X) button - visual only, clicks are hit-tested in Tick()
        _clearRect = NewRect("Clear", _searchRect);
        _clearRect.anchorMin = _clearRect.anchorMax = new Vector2(1f, 0.5f);
        _clearRect.pivot = new Vector2(1f, 0.5f);
        _clearRect.anchoredPosition = new Vector2(-8f, 0f);
        _clearRect.sizeDelta = new Vector2(36f, 36f);
        var clearX = Object.Instantiate(_detailTeam.gameObject, _clearRect);
        clearX.name = "X";
        var xt = clearX.GetComponent<TextMeshProUGUI>();
        xt.rectTransform.anchorMin = Vector2.zero;
        xt.rectTransform.anchorMax = Vector2.one;
        xt.rectTransform.offsetMin = Vector2.zero;
        xt.rectTransform.offsetMax = Vector2.zero;
        xt.fontSize = 26f;
        xt.fontStyle = FontStyles.Bold;
        xt.alignment = TextAlignmentOptions.Center;
        xt.color = SearchHint;
        xt.raycastTarget = false;
        xt.text = "X";

        // "no results" label, sits over the (empty) role list
        var nrGo = Object.Instantiate(_detailTeam.gameObject, listRoot);
        nrGo.name = "NoResults";
        _noResultsText = nrGo.GetComponent<TextMeshProUGUI>();
        var nrt = _noResultsText.rectTransform;
        nrt.anchorMin = Vector2.zero;
        nrt.anchorMax = Vector2.one;
        nrt.offsetMin = Vector2.zero;
        nrt.offsetMax = new Vector2(0f, -(SearchBarHeight + SearchMargin * 2f));
        _noResultsText.fontSize = 24f;
        _noResultsText.fontStyle = FontStyles.Italic;
        _noResultsText.alignment = TextAlignmentOptions.Center;
        _noResultsText.color = SearchHint;
        _noResultsText.raycastTarget = false;
        _noResultsText.text = "Nothing found";
        nrGo.SetActive(false);

        RefreshSearchVisual();
    }

    private static void StyleSearchBar()
    {
        _searchBg.sprite = WikiTheme.Pill();
        _searchBg.type = Image.Type.Sliced;

        var mag = NewRect("Icon", _searchRect);
        mag.anchorMin = mag.anchorMax = new Vector2(0f, 0.5f);
        mag.pivot = new Vector2(0f, 0.5f);
        mag.anchoredPosition = new Vector2(16f, 0f);
        mag.sizeDelta = new Vector2(24f, 24f);
        var magImg = mag.gameObject.AddComponent<Image>();
        magImg.sprite = WikiTheme.Magnifier();
        magImg.color = SearchHint;
        magImg.raycastTarget = false;

        var ring = NewRect("Ring", _searchRect);
        ring.anchorMin = Vector2.zero;
        ring.anchorMax = Vector2.one;
        ring.offsetMin = Vector2.zero;
        ring.offsetMax = Vector2.zero;
        _searchRing = ring.gameObject.AddComponent<Image>();
        _searchRing.sprite = WikiTheme.PillStroke(2f);
        _searchRing.type = Image.Type.Sliced;
        _searchRing.raycastTarget = false;
        _searchRing.enabled = false;
    }

    // ------------------------------------------------------------------ home screen + navigation

    private static TextMeshProUGUI MakeText(string name, Transform parent, float size, FontStyles style,
        TextAlignmentOptions align, Color color)
    {
        var go = Object.Instantiate(_detailTeam.gameObject, parent);
        go.name = name;
        var t = go.GetComponent<TextMeshProUGUI>();
        var rt = t.rectTransform;
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        t.fontSize = size;
        t.fontStyle = style;
        t.alignment = align;
        t.color = color;
        t.characterSpacing = 0f;
        t.richText = true;
        t.enableWordWrapping = true;
        t.overflowMode = TextOverflowModes.Overflow;
        t.raycastTarget = false;
        return t;
    }

    private static void SetAnchors(RectTransform rt, float minX, float minY, float maxX, float maxY)
    {
        rt.anchorMin = new Vector2(minX, minY);
        rt.anchorMax = new Vector2(maxX, maxY);
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    private static void BuildHome()
    {
        var body = Find("WindowRoot/Window/Body");
        _homeRoot = NewRect("Home", body);
        SetAnchors(_homeRoot, 0f, 0f, 1f, 1f);
        // Body lays its children out in a row: keep Home out of that so it covers the whole body
        _homeRoot.gameObject.AddComponent<LayoutElement>().ignoreLayout = true;

        var defs = new (Section target, string title, string blurb, Color accent)[]
        {
            (Section.Roles, "Roles", "Every role, what its buttons do and how often it shows up.", new Color(0.55f, 0.80f, 1f)),
            (Section.Modifiers, "Modifiers", "Extra perks and quirks that get added on top of a player's role.", new Color(0.74f, 0.52f, 1f)),
            (Section.Settings, "Settings", "What the game options do and how they change a match.", new Color(1f, 0.74f, 0.30f)),
        };

        const float margin = 0.05f, width = 0.275f, gap = 0.0375f;
        for (int i = 0; i < defs.Length; i++)
        {
            var d = defs[i];
            float x = margin + i * (width + gap);

            var rect = NewRect("Card_" + d.title, _homeRoot);
            SetAnchors(rect, x, 0.14f, x + width, 0.86f);
            var bg = rect.gameObject.AddComponent<Image>();
            bg.sprite = WikiTheme.Round(12);
            bg.type = Image.Type.Sliced;
            bg.color = RowNormal;
            bg.raycastTarget = false;

            var strokeRt = NewRect("Outline", rect);
            SetAnchors(strokeRt, 0f, 0f, 1f, 1f);
            var stroke = strokeRt.gameObject.AddComponent<Image>();
            stroke.sprite = WikiTheme.RoundStroke(12, 2f);
            stroke.type = Image.Type.Sliced;
            stroke.raycastTarget = false;

            // a row of real icons from the section (or a short text preview when there are none)
            var slotBg = new Image[4];
            var slotIcon = new Image[4];
            for (int k = 0; k < 4; k++)
            {
                var slot = NewRect("Slot" + k, rect);
                slot.anchorMin = slot.anchorMax = new Vector2(0.5f, 0.70f);
                slot.pivot = new Vector2(0.5f, 0.5f);
                slot.anchoredPosition = new Vector2((k - 1.5f) * 104f, 0f);
                slot.sizeDelta = new Vector2(88f, 88f);
                slotBg[k] = slot.gameObject.AddComponent<Image>();
                slotBg[k].sprite = WikiTheme.Round(10);
                slotBg[k].type = Image.Type.Sliced;
                slotBg[k].color = new Color(0f, 0f, 0f, 0.28f);
                slotBg[k].raycastTarget = false;

                var icon = NewRect("Icon", slot);
                SetAnchors(icon, 0f, 0f, 1f, 1f);
                icon.offsetMin = new Vector2(8f, 8f);
                icon.offsetMax = new Vector2(-8f, -8f);
                slotIcon[k] = icon.gameObject.AddComponent<Image>();
                slotIcon[k].preserveAspect = true;
                slotIcon[k].raycastTarget = false;
            }

            var preview = MakeText("Preview", rect, 28f, FontStyles.Normal, TextAlignmentOptions.Center, WikiTheme.Muted);
            SetAnchors(preview.rectTransform, 0.08f, 0.52f, 0.92f, 0.88f);

            var title = MakeText("Title", rect, 44f, FontStyles.Bold, TextAlignmentOptions.Center, WikiTheme.Text);
            SetAnchors(title.rectTransform, 0f, 0.31f, 1f, 0.46f);
            title.text = d.title;

            var blurb = MakeText("Blurb", rect, 25f, FontStyles.Normal, TextAlignmentOptions.Top, WikiTheme.Muted);
            SetAnchors(blurb.rectTransform, 0.09f, 0.13f, 0.91f, 0.30f);
            blurb.text = d.blurb;

            var count = MakeText("Count", rect, 22f, FontStyles.Normal, TextAlignmentOptions.Center, WikiTheme.Muted);
            SetAnchors(count.rectTransform, 0f, 0.03f, 1f, 0.10f);

            Cards.Add(new HomeCard
            {
                Target = d.target, Rect = rect, Bg = bg, Stroke = stroke, Accent = d.accent,
                SlotBg = slotBg, SlotIcon = slotIcon, Preview = preview, Count = count,
            });
        }

        _homeRoot.gameObject.SetActive(false);
    }

    private static void BuildBackButton()
    {
        var bar = Find("WindowRoot/Window/TitleBar");
        _backRect = NewRect("BackButton", bar);

        // sit just left of the close button, on the same anchors
        _backRect.anchorMin = _closeRect.anchorMin;
        _backRect.anchorMax = _closeRect.anchorMax;
        _backRect.pivot = new Vector2(1f, _closeRect.pivot.y);
        float closeLeft = _closeRect.anchoredPosition.x - _closeRect.pivot.x * _closeRect.rect.width;
        _backRect.anchoredPosition = new Vector2(closeLeft - 18f, _closeRect.anchoredPosition.y);
        _backRect.sizeDelta = new Vector2(190f, Mathf.Max(40f, _closeRect.rect.height));

        _backBg = _backRect.gameObject.AddComponent<Image>();
        _backBg.sprite = WikiTheme.Round(8);
        _backBg.type = Image.Type.Sliced;
        _backBg.color = BackNormal;
        _backBg.raycastTarget = false;

        var label = MakeText("Label", _backRect, 26f, FontStyles.Bold, TextAlignmentOptions.Center, WikiTheme.Text);
        label.text = "<  Back";

        _backRect.gameObject.SetActive(false);
    }

    private static void SetTitle(string section)
    {
        if (_titleText == null) return;
        const string wiki = "Launchpad Wiki";
        _titleText.text = string.IsNullOrEmpty(section)
            ? wiki
            : wiki + "  <size=70%><color=#" + ColorUtility.ToHtmlStringRGB(WikiTheme.Muted) + ">/ " + section + "</color></size>";
    }

    private static void ShowHome()
    {
        _section = Section.Home;
        _searchFocused = false;
        _pressedCard = null;
        _pressedBack = false;

        _roleListGo.SetActive(false);
        _detailGo.SetActive(false);
        _backRect.gameObject.SetActive(false);
        _homeRoot.gameObject.SetActive(true);

        SetTitle(null);
        WikiTheme.ResetAccent();

        foreach (var c in Cards)
        {
            var entries = EntriesFor(c.Target);
            string noun = c.Target == Section.Roles ? "roles" : c.Target == Section.Modifiers ? "modifiers" : "setting groups";
            c.Count.text = entries.Count + " " + noun;

            int shown = 0;
            foreach (var e in entries)
            {
                if (e.Icon == null || shown >= c.SlotIcon.Length) continue;
                c.SlotIcon[shown].sprite = e.Icon;
                shown++;
            }
            for (int k = 0; k < c.SlotIcon.Length; k++)
            {
                c.SlotBg[k].gameObject.SetActive(k < shown);
            }

            // no icons in this section: list the first few names instead
            c.Preview.gameObject.SetActive(shown == 0);
            if (shown == 0)
                c.Preview.text = string.Join("\n", entries.Take(4).Select(e => e.Name));

            c.Bg.color = RowNormal;
            c.Stroke.color = new Color(1f, 1f, 1f, 0.10f);
        }
    }

    private static void OpenSection(Section section)
    {
        _section = section;
        _placeholder = section == Section.Modifiers ? "Search modifiers..."
            : section == Section.Settings ? "Search settings..." : "Search roles...";

        if (_homeBuilt)
        {
            _homeRoot.gameObject.SetActive(false);
            _roleListGo.SetActive(true);
            _detailGo.SetActive(true);
            _backRect.gameObject.SetActive(true);
            SetTitle(section.ToString());
        }

        Rebuild();
    }

    private static List<WikiRole> CurrentEntries() => EntriesFor(_section);

    private static List<WikiRole> EntriesFor(Section section)
    {
        switch (section)
        {
            case Section.Modifiers: return WikiRegistry.Modifiers;
            case Section.Settings: return WikiRegistry.Settings;
            default: return WikiRegistry.Roles;
        }
    }

    // ------------------------------------------------------------------ search

    private static void SetQuery(string q)
    {
        if (q == _query) return;
        _query = q;
        RefreshSearchVisual();
        ApplyFilter();
    }

    private static void RefreshSearchVisual()
    {
        if (_searchText == null) return;

        bool empty = _query.Length == 0;
        _clearRect.gameObject.SetActive(!empty);
        _searchBg.color = _searchFocused ? SearchFocused : SearchNormal;
        if (_searchRing != null)
        {
            _searchRing.enabled = _searchFocused;
            _searchRing.color = WikiTheme.Accent;
        }

        if (empty)
        {
            _searchText.text = _placeholder;
            _searchText.color = SearchHint;
            _caretRect.anchoredPosition = new Vector2(SearchPadLeft, 0f);
            return;
        }

        // keep the end of the text (where you're typing) in view if it doesn't fit
        float avail = _searchRect.rect.width - SearchPadLeft - SearchPadRight;
        string shown = _query;
        while (shown.Length > 1 && Measure(shown) > avail) shown = shown.Substring(1);

        _searchText.text = shown;
        _searchText.color = Color.white;
        _caretRect.anchoredPosition = new Vector2(SearchPadLeft + Measure(shown) + 2f, 0f);
    }

    // width of a string in canvas units; the "|" trick keeps trailing spaces counted
    private static float Measure(string text)
    {
        return _searchText.GetPreferredValues(text + "|").x - _searchText.GetPreferredValues("|").x;
    }

    private static string BuildHaystack(WikiRole role)
    {
        var parts = new List<string> { role.Name, role.Team, role.Description };
        if (role.Abilities != null)
        {
            foreach (var a in role.Abilities)
            {
                parts.Add(a.Name);
                parts.Add(a.Description);
            }
        }
        // drop TMP rich-text tags so typing e.g. "color" doesn't match markup
        return Regex.Replace(string.Join(" ", parts), "<[^>]*>", "").ToLowerInvariant();
    }

    private static void ApplyFilter()
    {
        var tokens = _query.Trim().ToLowerInvariant()
            .Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

        int visible = 0;
        RoleView firstVisible = null;
        foreach (var v in Views)
        {
            bool match = true;
            foreach (var t in tokens)
            {
                if (!v.Haystack.Contains(t)) { match = false; break; }
            }

            v.Visible = match;
            v.Rect.gameObject.SetActive(match); // hidden rows drop out of the layout
            if (match)
            {
                visible++;
                firstVisible ??= v;
            }
        }

        _noResultsText.gameObject.SetActive(visible == 0 && Views.Count > 0);

        LayoutRebuilder.ForceRebuildLayoutImmediate(_listContent);
        _listContent.anchoredPosition = Vector2.zero;

        // if the open role was filtered out, jump to the first match
        if (firstVisible != null && (_selected == null || !Views.Any(v => v.Visible && v.Role == _selected)))
            Select(firstVisible.Role);
    }

    private static void HandleTyping()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            _searchFocused = false;
            RefreshSearchVisual();
            return;
        }

        string q = _query;
        foreach (char c in Input.inputString)
        {
            if (c == '\b')
            {
                if (q.Length > 0) q = q.Substring(0, q.Length - 1);
            }
            else if (c == '\n' || c == '\r')
            {
                _searchFocused = false; // Enter = done typing
                RefreshSearchVisual();
            }
            else if (!char.IsControl(c) && q.Length < SearchMaxLength)
            {
                q += c;
            }
        }

        if (q != _query) SetQuery(q);
    }

    // ------------------------------------------------------------------ content

    private static void Rebuild()
    {
        ClearChildren(_listContent);
        Views.Clear();
        _query = "";
        _searchFocused = false;
        _noResultsText.gameObject.SetActive(false);
        RefreshSearchVisual();

        foreach (var role in CurrentEntries())
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

            WikiTheme.StyleRoleRow(go, color);

            var bg = go.GetComponent<Image>();
            bg.color = RowNormal;

            Views.Add(new RoleView { Role = role, Rect = go.GetComponent<RectTransform>(), Bg = bg, Highlight = highlight, Haystack = BuildHaystack(role) });
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate(_listContent);
        _listContent.anchoredPosition = Vector2.zero;

        if (Views.Count > 0) Select(Views[0].Role);
        else ShowEmpty();
    }

    private static void ShowEmpty()
    {
        _selected = null;
        WikiTheme.ResetAccent();
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
        WikiTheme.StyleDetail(role.Color);
        WikiTheme.SetStats(SafeInt(role.GetAmount), SafeInt(role.GetChance), role.Color);
        _detailDesc.text = role.GetDescription != null ? SafeString(role.GetDescription) : role.Description;
        SetIcon(_detailIcon, role.Icon);

        ClearChildren(_abilityContainer);
        bool has = role.Abilities != null && role.Abilities.Count > 0;
        _abilitiesHeader.SetActive(has);
        if (_abilitiesHeaderText != null)
            _abilitiesHeaderText.text = _section == Section.Settings ? "Options" : "Abilities";
        _abilityContainer.gameObject.SetActive(has);

        if (has)
        {
            foreach (var ability in role.Abilities)
            {
                var row = Object.Instantiate(_abilityRowPrefab, _abilityContainer);
                row.name = "Ability_" + ability.Name;
                WikiTheme.StyleAbilityRow(row, role.Color);

                SetIcon(row.transform.Find("Icon").GetComponent<Image>(), ability.Icon);

                var nameT = row.transform.Find("TextColumn/Header/Name").GetComponent<TextMeshProUGUI>();
                nameT.text = ability.Name;
                nameT.color = role.Color;

                var cd = row.transform.Find("TextColumn/Header/Cooldown").GetComponent<TextMeshProUGUI>();
                bool hasCd = !string.IsNullOrEmpty(ability.Cooldown);
                cd.gameObject.SetActive(hasCd);
                cd.text = ability.Cooldown;

                var descT = row.transform.Find("TextColumn/Description").GetComponent<TextMeshProUGUI>();
                descT.enableWordWrapping = true;
                descT.overflowMode = TextOverflowModes.Overflow;
                descT.text = ability.Description;
            }
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate(_detailContent);
        _detailContent.anchoredPosition = Vector2.zero;
    }

    private static string SafeString(Func<string> get)
    {
        try { return get() ?? ""; }
        catch { return ""; }
    }

    private static int? SafeInt(Func<int?> get)
    {
        try { return get?.Invoke(); }
        catch (Exception e)
        {
            Debug.LogWarning("[Wiki] Could not read role amount/chance: " + e.Message);
            return null;
        }
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

        WikiTheme.Tick();

        if (_section == Section.Home)
        {
            TickHome(Input.mousePosition);
            return;
        }

        WikiTheme.UpdateScrollbar(_listBar, _listViewport, _listContent);
        if (_detailScrollObj.activeSelf) WikiTheme.UpdateScrollbar(_detailBar, _detailViewport, _detailContent);

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
            v.Bg.color = overList && v.Visible && Hit(v.Rect, mouse) ? RowHover : v.Role == _selected ? RowSelected : RowNormal;
        _closeBg.color = Hit(_closeRect, mouse) ? CloseHover : CloseNormal;
        if (_backBg != null) _backBg.color = Hit(_backRect, mouse) ? BackHover : BackNormal;

        // search typing + blinking caret
        if (_searchFocused) HandleTyping();
        _caretRect.gameObject.SetActive(_searchFocused && (int)(Time.unscaledTime * 2f) % 2 == 0);

        // press / drag (touch-friendly) / release
        if (Input.GetMouseButtonDown(0))
        {
            _pressArea = overList ? 1 : overDetail ? 2 : 0;
            _pressedClose = Hit(_closeRect, mouse);
            _pressedBack = _backRect != null && _backRect.gameObject.activeSelf && Hit(_backRect, mouse);

            // search bar: click focuses, X clears, clicking anywhere else unfocuses
            if (_clearRect.gameObject.activeSelf && Hit(_clearRect, mouse))
            {
                SetQuery("");
                _searchFocused = true;
            }
            else
            {
                _searchFocused = Hit(_searchRect, mouse);
            }
            RefreshSearchVisual();
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
            else if (_pressedBack && Hit(_backRect, mouse))
            {
                ShowHome();
            }
            else if (_pressArea == 1 && !_dragged && overList)
            {
                foreach (var v in Views)
                {
                    if (v.Visible && Hit(v.Rect, mouse)) { Select(v.Role); break; }
                }
            }
            _pressArea = 0;
            _pressedClose = false;
            _pressedBack = false;
        }
    }

    private static void TickHome(Vector2 mouse)
    {
        _closeBg.color = Hit(_closeRect, mouse) ? CloseHover : CloseNormal;

        foreach (var c in Cards)
        {
            bool hover = Hit(c.Rect, mouse);
            c.Bg.color = hover ? RowHover : RowNormal;
            c.Stroke.color = hover ? new Color(c.Accent.r, c.Accent.g, c.Accent.b, 0.9f) : new Color(1f, 1f, 1f, 0.10f);
        }

        if (Input.GetMouseButtonDown(0))
        {
            _pressedClose = Hit(_closeRect, mouse);
            _pressedCard = null;
            foreach (var c in Cards)
            {
                if (Hit(c.Rect, mouse)) { _pressedCard = c; break; }
            }
        }
        else if (Input.GetMouseButtonUp(0))
        {
            if (_pressedClose && Hit(_closeRect, mouse)) Close();
            else if (_pressedCard != null && Hit(_pressedCard.Rect, mouse)) OpenSection(_pressedCard.Target);

            _pressedClose = false;
            _pressedCard = null;
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
