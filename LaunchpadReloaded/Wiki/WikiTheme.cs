using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;
using Debug = UnityEngine.Debug;

namespace LaunchpadReloaded.Wiki; // <-- change to your mod's namespace

/// <summary>
/// Restyles the wiki at runtime: chamfered "sci-fi" panels, neon frame + glow, role-coloured accents that
/// follow the selected role, stat chips, scroll indicators and an open animation.
/// Every sprite is generated in code, so the existing AssetBundle works unchanged (no Unity rebuild).
/// To retheme, edit the palette below.
/// </summary>
public static class WikiTheme
{
    // ------------------------------------------------------------------ palette
    public static readonly Color Ink = new(0.035f, 0.050f, 0.090f, 1f);      // window
    public static readonly Color BarInk = new(0.024f, 0.034f, 0.066f, 1f);   // title bar
    public static readonly Color ListInk = new(0.045f, 0.063f, 0.110f, 0.92f); // role list column
    public static readonly Color Cyan = new(0.24f, 0.90f, 1f, 1f);           // default accent
    public static readonly Color Muted = new(0.58f, 0.64f, 0.76f, 1f);
    public static readonly Color Text = new(0.90f, 0.93f, 0.98f, 1f);

    private const int WindowCut = 26;   // chamfer size of the window corners (px)
    private const int GlowSpread = 38;  // how far the halo reaches outside the window (px)

    // ------------------------------------------------------------------ state
    public static bool Installed { get; private set; }
    public static Color Accent => _accent;

    private class AccentRef
    {
        public Image Img;
        public float Alpha;
        public bool Pulse;
    }

    private static readonly List<AccentRef> Accents = new();
    private static readonly List<RectTransform> Animated = new();
    private static readonly Dictionary<string, Sprite> Cache = new();

    private static TextMeshProUGUI _template;
    private static CanvasGroup _group;
    private static Color _accent = Cyan, _accentTarget = Cyan;
    private static float _openT = 1f;

    // detail pane pieces
    private static Image _divider, _headerFill, _headerStroke;
    private static RectTransform _statsRow, _fillRt;
    private static Image _amountBg, _chanceBg, _chanceFill;
    private static TextMeshProUGUI _amountText, _chanceText;
    private static GameObject _amountChip, _chanceChip;

    // ------------------------------------------------------------------ public API

    /// <summary>Restyles the window. Call once after the prefab is instantiated and its children are found.</summary>
    public static void Install(Transform root, TextMeshProUGUI template)
    {
        _template = template;

        var windowRoot = root.Find("WindowRoot");
        var window = windowRoot.Find("Window");
        var winRt = window.GetComponent<RectTransform>();
        var size = winRt.sizeDelta;

        _group = windowRoot.gameObject.AddComponent<CanvasGroup>();

        // dim backdrop
        windowRoot.Find("Dim").GetComponent<Image>().color = new Color(0.01f, 0.02f, 0.05f, 0.80f);

        // window body
        Skin(window.GetComponent<Image>(), Cut(WindowCut), Ink);

        // soft halo behind the window
        var glow = NewImage("Glow", windowRoot);
        glow.sprite = Glow(GlowSpread, WindowCut);
        glow.type = Image.Type.Sliced;
        Center(glow.rectTransform, size + new Vector2(GlowSpread * 2f, GlowSpread * 2f));
        glow.transform.SetSiblingIndex(window.GetSiblingIndex());
        TrackAccent(glow, 0.30f, true);

        // neon frame on top of everything in the window
        var frame = NewImage("Frame", windowRoot);
        frame.sprite = CutStroke(WindowCut, 2f);
        frame.type = Image.Type.Sliced;
        Center(frame.rectTransform, size);
        frame.transform.SetSiblingIndex(window.GetSiblingIndex() + 1);
        TrackAccent(frame, 0.85f, true);

        Animated.Clear();
        Animated.Add(glow.rectTransform);
        Animated.Add(winRt);
        Animated.Add(frame.rectTransform);

        // faint dot grid behind the content
        var grid = NewImage("Grid", window);
        Stretch(grid.rectTransform, 0, 0, 0, 0);
        grid.sprite = Dots();
        grid.type = Image.Type.Tiled;
        grid.color = new Color(1f, 1f, 1f, 0.045f);
        grid.transform.SetAsFirstSibling();

        // ---- title bar
        var bar = window.Find("TitleBar");
        Skin(bar.GetComponent<Image>(), Cut(WindowCut, true, false), BarInk);

        var sheen = NewImage("Sheen", bar);
        Stretch(sheen.rectTransform, 0, 0, 0, 0);
        sheen.sprite = VGrad();
        sheen.color = new Color(1f, 1f, 1f, 0.05f);
        sheen.transform.SetAsFirstSibling();

        var line = NewImage("AccentLine", bar);
        var lrt = line.rectTransform;
        lrt.anchorMin = new Vector2(0f, 0f);
        lrt.anchorMax = new Vector2(1f, 0f);
        lrt.pivot = new Vector2(0.5f, 0f);
        lrt.anchoredPosition = Vector2.zero;
        lrt.sizeDelta = new Vector2(0f, 3f);
        line.sprite = HFade();
        TrackAccent(line, 1f, false);
        line.transform.SetSiblingIndex(1);

        var logo = NewImage("Logo", bar);
        var lgrt = logo.rectTransform;
        lgrt.anchorMin = lgrt.anchorMax = new Vector2(0f, 0.5f);
        lgrt.pivot = new Vector2(0f, 0.5f);
        lgrt.anchoredPosition = new Vector2(34f, 0f);
        lgrt.sizeDelta = new Vector2(40f, 40f);
        logo.sprite = LogoSprite();
        TrackAccent(logo, 1f, false);

        var title = bar.Find("Title").GetComponent<TextMeshProUGUI>();
        title.text = "LAUNCHPAD <color=#" + ColorUtility.ToHtmlStringRGB(Cyan) + ">WIKI</color>";
        title.characterSpacing = 10f;
        title.rectTransform.offsetMin = new Vector2(92f, title.rectTransform.offsetMin.y);

        // close button shape (colour is driven by WikiPanel hover logic)
        Skin(bar.Find("CloseButton").GetComponent<Image>(), Cut(10), Color.white);

        // ---- role list column
        var body = window.Find("Body");
        var roleList = body.Find("RoleList");
        roleList.GetComponent<Image>().color = ListInk;

        var edge = NewImage("Edge", roleList);
        var ert = edge.rectTransform;
        ert.anchorMin = new Vector2(1f, 0f);
        ert.anchorMax = new Vector2(1f, 1f);
        ert.pivot = new Vector2(1f, 0.5f);
        ert.anchoredPosition = Vector2.zero;
        ert.sizeDelta = new Vector2(2f, 0f);
        TrackAccent(edge, 0.35f, false);

        // ---- detail pane: soft coloured aura in the top-right corner
        var detail = body.Find("Detail");
        var aura = NewImage("Aura", detail);
        var art = aura.rectTransform;
        art.anchorMin = art.anchorMax = new Vector2(1f, 1f);
        art.pivot = new Vector2(1f, 1f);
        art.anchoredPosition = Vector2.zero;
        art.sizeDelta = new Vector2(820f, 820f);
        aura.sprite = Radial();
        TrackAccent(aura, 0.16f, false);
        aura.transform.SetAsFirstSibling();

        // ---- detail header: icon frame + divider
        var content = detail.Find("DetailScroll/Viewport/Content");
        var header = content.Find("Header");
        _headerFill = IgnoredChild("IconFrame", header, new Vector2(0f, 0.5f), new Vector2(-12f, 0f), new Vector2(134f, 134f));
        _headerFill.sprite = Cut(16);
        _headerFill.type = Image.Type.Sliced;
        _headerStroke = IgnoredChild("IconStroke", header, new Vector2(0f, 0.5f), new Vector2(-12f, 0f), new Vector2(134f, 134f));
        _headerStroke.sprite = CutStroke(16, 2f);
        _headerStroke.type = Image.Type.Sliced;
        _headerStroke.transform.SetSiblingIndex(0);
        _headerFill.transform.SetSiblingIndex(0);   // fill first, stroke second, icon after

        _divider = content.Find("Divider").GetComponent<Image>();
        _divider.sprite = HFade();

        Installed = true;
    }

    /// <summary>Builds the "AMOUNT / CHANCE" chips inside the detail header's text block.</summary>
    public static void BuildStatChips(Transform parent, TextMeshProUGUI template)
    {
        _template = template;

        var rowRt = NewRect("Stats", parent);
        var hlg = rowRt.gameObject.AddComponent<HorizontalLayoutGroup>();
        hlg.spacing = 12f;
        hlg.padding = Pad(0, 0, 10, 0);
        hlg.childAlignment = TextAnchor.MiddleLeft;
        hlg.childControlWidth = true;
        hlg.childControlHeight = true;
        hlg.childForceExpandWidth = false;
        hlg.childForceExpandHeight = false;
        _statsRow = rowRt;

        _amountChip = NewChip(rowRt, "AmountChip", out _amountBg, out _amountText, out _);
        _chanceChip = NewChip(rowRt, "ChanceChip", out _chanceBg, out _chanceText, out var fillRt);
        _fillRt = fillRt;
        _chanceFill = fillRt.GetComponent<Image>();
    }

    /// <summary>Shows amount / chance for the selected role. Pass null to hide a chip.</summary>
    public static void SetStats(int? amount, int? chance, Color c)
    {
        if (_statsRow == null) return;

        bool any = amount.HasValue || chance.HasValue;
        _statsRow.gameObject.SetActive(any);
        if (!any) return;

        var label = "<color=#" + ColorUtility.ToHtmlStringRGB(Muted) + "><size=68%>";
        var tint = Color.Lerp(c, Color.white, 0.55f);

        _amountChip.SetActive(amount.HasValue);
        if (amount.HasValue)
        {
            _amountBg.color = new Color(c.r, c.g, c.b, 0.16f);
            _amountText.color = tint;
            _amountText.text = label + "AMOUNT</size></color>   <b>" + amount.Value + "</b>";
        }

        _chanceChip.SetActive(chance.HasValue);
        if (chance.HasValue)
        {
            float p = Mathf.Clamp01(chance.Value / 100f);
            _chanceBg.color = new Color(c.r, c.g, c.b, 0.16f);
            _chanceText.color = tint;
            _chanceText.text = label + "CHANCE</size></color>   <b>" + chance.Value + "%</b>";

            // the chip doubles as a progress bar
            _chanceFill.gameObject.SetActive(p > 0.04f);
            _chanceFill.color = new Color(c.r, c.g, c.b, 0.34f);
            _fillRt.anchorMax = new Vector2(p, 1f);
        }
    }

    /// <summary>Colours the detail pane for the selected role and makes it the window accent.</summary>
    public static void StyleDetail(Color c)
    {
        if (!Installed) return;
        _accentTarget = c;
        _headerFill.color = new Color(c.r, c.g, c.b, 0.14f);
        _headerStroke.color = new Color(c.r, c.g, c.b, 0.85f);
        _divider.color = new Color(c.r, c.g, c.b, 0.75f);
    }

    public static void ResetAccent()
    {
        _accentTarget = Cyan;
    }

    /// <summary>Styles one entry of the role list.</summary>
    public static void StyleRoleRow(GameObject row, Color c)
    {
        if (!Installed) return;

        Skin(row.GetComponent<Image>(), Cut(14), Color.white);

        // slim accent bar, inset so it clears the chamfered corners
        var accent = row.transform.Find("Accent").GetComponent<RectTransform>();
        accent.anchorMin = new Vector2(0f, 0f);
        accent.anchorMax = new Vector2(0f, 1f);
        accent.pivot = new Vector2(0f, 0.5f);
        accent.anchoredPosition = new Vector2(10f, 0f);
        accent.sizeDelta = new Vector2(4f, -32f);

        // selected outline
        var hi = row.transform.Find("Highlight").GetComponent<Image>();
        hi.sprite = CutStroke(14, 2f);
        hi.type = Image.Type.Sliced;
        hi.color = new Color(c.r, c.g, c.b, 0.95f);

        // framed icon
        var icon = row.transform.Find("Icon");
        var frame = NewImage("IconFrame", row.transform);
        var frt = frame.rectTransform;
        frt.anchorMin = frt.anchorMax = new Vector2(0f, 0.5f);
        frt.pivot = new Vector2(0f, 0.5f);
        frt.anchoredPosition = new Vector2(14f, 0f);
        frt.sizeDelta = new Vector2(68f, 68f);
        frame.sprite = Cut(10);
        frame.type = Image.Type.Sliced;
        frame.color = new Color(c.r, c.g, c.b, 0.16f);
        frame.transform.SetSiblingIndex(icon.GetSiblingIndex());

        var stroke = NewImage("Stroke", frame.transform);
        Stretch(stroke.rectTransform, 0, 0, 0, 0);
        stroke.sprite = CutStroke(10, 1.5f);
        stroke.type = Image.Type.Sliced;
        stroke.color = new Color(c.r, c.g, c.b, 0.55f);
    }

    /// <summary>Styles one ability card.</summary>
    public static void StyleAbilityRow(GameObject row, Color c)
    {
        if (!Installed) return;

        var bg = row.GetComponent<Image>();
        Skin(bg, Cut(16), new Color(0.075f, 0.100f, 0.165f, 1f));

        var frame = IgnoredChild("IconFrame", row.transform, new Vector2(0f, 1f), new Vector2(12f, -12f), new Vector2(92f, 92f), pivot: new Vector2(0f, 1f));
        frame.sprite = Cut(12);
        frame.type = Image.Type.Sliced;
        frame.color = new Color(c.r, c.g, c.b, 0.12f);
        frame.transform.SetAsFirstSibling();

        var stroke = NewImage("Stroke", frame.transform);
        Stretch(stroke.rectTransform, 0, 0, 0, 0);
        stroke.sprite = CutStroke(12, 1.5f);
        stroke.type = Image.Type.Sliced;
        stroke.color = new Color(c.r, c.g, c.b, 0.5f);

        // The prefab lays the icon and text out for a short one-line row, so a wrapped description ran under the
        // icon. Pin the icon inside its frame, push the text column past the frame, and keep the row tall enough.
        var icon = row.transform.Find("Icon");
        if (icon != null)
        {
            var iconLe = icon.GetComponent<LayoutElement>();
            if (iconLe == null) iconLe = icon.gameObject.AddComponent<LayoutElement>();
            iconLe.ignoreLayout = true;

            var irt = icon.GetComponent<RectTransform>();
            irt.anchorMin = irt.anchorMax = new Vector2(0f, 1f);
            irt.pivot = new Vector2(0.5f, 0.5f);
            irt.anchoredPosition = new Vector2(12f + 46f, -(12f + 46f)); // centre of the 92x92 frame
            irt.sizeDelta = new Vector2(76f, 76f);
            icon.SetSiblingIndex(frame.transform.GetSiblingIndex() + 1);

            var iconImg = icon.GetComponent<Image>();
            if (iconImg != null) iconImg.preserveAspect = true;
        }

        var group = row.GetComponent<LayoutGroup>();
        if (group != null) group.padding = Pad(12 + 92 + 18, 18, 14, 14);

        var rowLe = row.GetComponent<LayoutElement>();
        if (rowLe == null) rowLe = row.AddComponent<LayoutElement>();
        rowLe.minHeight = 12f + 92f + 12f;

        // left stripe in the role colour
        var stripe = NewImage("Stripe", row.transform);
        var srt = stripe.rectTransform;
        srt.anchorMin = new Vector2(0f, 0f);
        srt.anchorMax = new Vector2(0f, 1f);
        srt.pivot = new Vector2(0f, 0.5f);
        srt.anchoredPosition = Vector2.zero;
        srt.sizeDelta = new Vector2(4f, -44f);
        stripe.color = c;
        var le = stripe.gameObject.AddComponent<LayoutElement>();
        le.ignoreLayout = true;
    }

    /// <summary>A thin scroll indicator on the right edge of a scroll area. Returns its rect for UpdateScrollbar.</summary>
    public static RectTransform CreateScrollbar(Transform scrollArea)
    {
        var img = NewImage("Scrollbar", scrollArea);
        var rt = img.rectTransform;
        rt.anchorMin = rt.anchorMax = new Vector2(1f, 1f);
        rt.pivot = new Vector2(1f, 1f);
        rt.sizeDelta = new Vector2(4f, 40f);
        img.sprite = Pill();
        img.type = Image.Type.Sliced;
        TrackAccent(img, 0.6f, false);
        return rt;
    }

    public static void UpdateScrollbar(RectTransform bar, RectTransform viewport, RectTransform content)
    {
        if (bar == null) return;

        float vh = viewport.rect.height;
        float ch = content.rect.height;
        float max = ch - vh;
        bool show = max > 1f;
        if (bar.gameObject.activeSelf != show) bar.gameObject.SetActive(show);
        if (!show) return;

        float h = Mathf.Max(40f, vh * vh / ch);
        float t = Mathf.Clamp01(content.anchoredPosition.y / max);
        bar.sizeDelta = new Vector2(4f, h);
        bar.anchoredPosition = new Vector2(-4f, -(10f + t * Mathf.Max(0f, vh - h - 20f)));
    }

    /// <summary>Call whenever the wiki opens.</summary>
    public static void OnOpen()
    {
        if (!Installed) return;
        _openT = 0f;
        _group.alpha = 0f;
        foreach (var rt in Animated) rt.localScale = Vector3.one * 0.94f;
    }

    /// <summary>Call every frame while the wiki is open.</summary>
    public static void Tick()
    {
        if (!Installed) return;

        float dt = Time.unscaledDeltaTime;
        _accent = Color.Lerp(_accent, _accentTarget, 1f - Mathf.Exp(-9f * dt));
        float pulse = 0.5f + 0.5f * Mathf.Sin(Time.unscaledTime * 2.2f);

        foreach (var a in Accents)
        {
            if (a.Img == null) continue;
            var c = _accent;
            c.a = a.Alpha * (a.Pulse ? 0.78f + 0.22f * pulse : 1f);
            a.Img.color = c;
        }

        if (_openT < 1f)
        {
            _openT = Mathf.Min(1f, _openT + dt / 0.24f);
            float e = 1f - Mathf.Pow(1f - _openT, 3f);
            var s = Vector3.one * Mathf.Lerp(0.94f, 1f, e);
            foreach (var rt in Animated) rt.localScale = s;
            _group.alpha = e;
        }
    }

    // ------------------------------------------------------------------ small UI helpers

    private static void TrackAccent(Image img, float alpha, bool pulse)
    {
        img.raycastTarget = false;
        Accents.Add(new AccentRef { Img = img, Alpha = alpha, Pulse = pulse });
    }

    // Il2Cpp's RectOffset has no (l, r, t, b) constructor
    private static RectOffset Pad(int l, int r, int t, int b)
    {
        var o = new RectOffset();
        o.left = l;
        o.right = r;
        o.top = t;
        o.bottom = b;
        return o;
    }

    private static RectTransform NewRect(string name, Transform parent)
    {
        var go = new GameObject(name);
        go.layer = parent.gameObject.layer;
        var rt = go.AddComponent<RectTransform>();
        rt.SetParent(parent, false);
        return rt;
    }

    private static Image NewImage(string name, Transform parent)
    {
        var rt = NewRect(name, parent);
        var img = rt.gameObject.AddComponent<Image>();
        img.raycastTarget = false;
        return img;
    }

    private static void Center(RectTransform rt, Vector2 size)
    {
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = Vector2.zero;
        rt.sizeDelta = size;
    }

    private static void Stretch(RectTransform rt, float l, float b, float r, float t)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = new Vector2(l, b);
        rt.offsetMax = new Vector2(-r, -t);
    }

    private static void Skin(Image img, Sprite sprite, Color color)
    {
        img.sprite = sprite;
        img.type = Image.Type.Sliced;
        img.color = color;
    }

    /// <summary>An Image child that a layout group must not move or resize.</summary>
    private static Image IgnoredChild(string name, Transform parent, Vector2 anchor, Vector2 pos, Vector2 size, Vector2? pivot = null)
    {
        var img = NewImage(name, parent);
        var rt = img.rectTransform;
        rt.anchorMin = rt.anchorMax = anchor;
        rt.pivot = pivot ?? anchor;
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;
        var le = img.gameObject.AddComponent<LayoutElement>();
        le.ignoreLayout = true;
        return img;
    }

    private static GameObject NewChip(Transform parent, string name, out Image bg, out TextMeshProUGUI text, out RectTransform fill)
    {
        var rt = NewRect(name, parent);
        bg = rt.gameObject.AddComponent<Image>();
        bg.sprite = Pill();
        bg.type = Image.Type.Sliced;
        bg.raycastTarget = false;

        var h = rt.gameObject.AddComponent<HorizontalLayoutGroup>();
        h.padding = Pad(18, 18, 8, 8);
        h.childAlignment = TextAnchor.MiddleCenter;
        h.childControlWidth = true;
        h.childControlHeight = true;
        h.childForceExpandWidth = false;
        h.childForceExpandHeight = false;

        // progress fill (ignored by the layout, sits behind the text)
        var fillImg = NewImage("Fill", rt);
        fill = fillImg.rectTransform;
        fill.anchorMin = Vector2.zero;
        fill.anchorMax = Vector2.one;
        fill.offsetMin = Vector2.zero;
        fill.offsetMax = Vector2.zero;
        fillImg.sprite = Pill();
        fillImg.type = Image.Type.Sliced;
        fillImg.gameObject.AddComponent<LayoutElement>().ignoreLayout = true;

        var tgo = Object.Instantiate(_template.gameObject, rt);
        tgo.name = "Label";
        text = tgo.GetComponent<TextMeshProUGUI>();
        text.fontSize = 24f;
        text.fontStyle = FontStyles.Normal;
        text.alignment = TextAlignmentOptions.Center;
        text.richText = true;
        text.enableWordWrapping = false;
        text.overflowMode = TextOverflowModes.Overflow;
        text.raycastTarget = false;
        text.text = "";

        return rt.gameObject;
    }

    // ------------------------------------------------------------------ procedural sprites
    // All sprites are white with alpha, so Image.color tints them. Coordinates: pixels, origin bottom-left.

    public static Sprite Pill() => Round(16);
    public static Sprite PillStroke(float t = 2f) => RoundStroke(16, t);

    public static Sprite Magnifier()
    {
        return Make("magnifier", 64, 64, (x, y) =>
        {
            float ring = Mathf.Abs(Dist(x, y, 26f, 38f) - 15f) - 3f;
            float handle = SegDist(x, y, 38f, 26f, 55f, 9f) - 3.6f;
            return 0.5f - Mathf.Min(ring, handle);
        }, Vector4.zero);
    }

    private static Sprite LogoSprite()
    {
        // arrow-head / launch chevron
        var poly = new[] { 32f, 61f, 58f, 8f, 32f, 24f, 6f, 8f };
        return Make("logo", 64, 64, (x, y) => PolyCoverage(x, y, poly), Vector4.zero);
    }

    /// <summary>Filled rectangle with the top-left and bottom-right corners chamfered.</summary>
    public static Sprite Cut(int c, bool tl = true, bool br = true)
    {
        int s = c * 2 + 4;
        return Make("cut" + c + (tl ? "T" : "t") + (br ? "B" : "b"), s, s,
            (x, y) => 0.5f - CutD(x, y, s, s, c, tl, br), new Vector4(c, c, c, c));
    }

    /// <summary>Outline of <see cref="Cut"/>, t pixels thick, drawn inside the shape.</summary>
    public static Sprite CutStroke(int c, float t)
    {
        int s = c * 2 + 4;
        return Make("cutS" + c + "_" + t, s, s, (x, y) =>
        {
            float d = CutD(x, y, s, s, c, true, true);
            return 0.5f - (Mathf.Abs(d + t * 0.5f) - t * 0.5f);
        }, new Vector4(c, c, c, c));
    }

    public static Sprite Round(int r)
    {
        int s = r * 2 + 4;
        return Make("round" + r, s, s, (x, y) => 0.5f - RoundD(x, y, s, s, r), new Vector4(r, r, r, r));
    }

    public static Sprite RoundStroke(int r, float t)
    {
        int s = r * 2 + 4;
        return Make("roundS" + r + "_" + t, s, s, (x, y) =>
        {
            float d = RoundD(x, y, s, s, r);
            return 0.5f - (Mathf.Abs(d + t * 0.5f) - t * 0.5f);
        }, new Vector4(r, r, r, r));
    }

    /// <summary>Soft halo for a chamfered panel: opaque inside the panel, fading out over 'spread' px.</summary>
    public static Sprite Glow(int spread, int c)
    {
        int b = spread + c;
        int s = b * 2 + 4;
        return Make("glow" + spread + "_" + c, s, s, (x, y) =>
        {
            float d = CutD(x - spread, y - spread, s - spread * 2, s - spread * 2, c, true, true);
            if (d <= 0f) return 1f;
            float t = Mathf.Clamp01(1f - d / spread);
            return t * t;
        }, new Vector4(b, b, b, b));
    }

    public static Sprite Radial()
    {
        return Make("radial", 128, 128, (x, y) =>
        {
            float t = Mathf.Clamp01(1f - Dist(x, y, 64f, 64f) / 64f);
            return t * t;
        }, Vector4.zero);
    }

    /// <summary>Opaque at the top, transparent at the bottom.</summary>
    public static Sprite VGrad() => Make("vgrad", 4, 64, (x, y) => y / 64f, Vector4.zero);

    /// <summary>Opaque at the left, transparent at the right.</summary>
    public static Sprite HFade() => Make("hfade", 64, 4, (x, y) => 1f - x / 64f, Vector4.zero);

    public static Sprite Dots()
    {
        return Make("dots", 28, 28, (x, y) => Mathf.Clamp01(1.6f - Dist(x, y, 14f, 14f)),
            Vector4.zero, TextureWrapMode.Repeat);
    }

    // ------------------------------------------------------------------ sprite generation internals

    private static Sprite Make(string key, int w, int h, Func<float, float, float> alpha, Vector4 border,
        TextureWrapMode wrap = TextureWrapMode.Clamp)
    {
        if (Cache.TryGetValue(key, out var cached) && cached != null) return cached;

        var px = new Color32[w * h];
        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                byte a = (byte)Mathf.RoundToInt(Mathf.Clamp01(alpha(x + 0.5f, y + 0.5f)) * 255f);
                px[y * w + x] = new Color32(255, 255, 255, a);
            }
        }

        var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
        tex.SetPixels32(px);
        tex.Apply(false, false);
        tex.wrapMode = wrap;
        tex.filterMode = FilterMode.Bilinear;
        tex.hideFlags |= HideFlags.DontUnloadUnusedAsset;

        var sprite = Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), 100f, 0u, SpriteMeshType.FullRect, border);
        sprite.hideFlags |= HideFlags.DontUnloadUnusedAsset;
        Cache[key] = sprite;
        return sprite;
    }

    private static float Dist(float x, float y, float cx, float cy)
    {
        float dx = x - cx, dy = y - cy;
        return Mathf.Sqrt(dx * dx + dy * dy);
    }

    private static float SegDist(float x, float y, float ax, float ay, float bx, float by)
    {
        float abx = bx - ax, aby = by - ay;
        float t = Mathf.Clamp01(((x - ax) * abx + (y - ay) * aby) / (abx * abx + aby * aby));
        return Dist(x, y, ax + abx * t, ay + aby * t);
    }

    // signed distance (px, negative inside) of a w*h rectangle with optional chamfered top-left / bottom-right corners
    private static float CutD(float x, float y, float w, float h, float c, bool tl, bool br)
    {
        float dx = Mathf.Abs(x - w * 0.5f) - w * 0.5f;
        float dy = Mathf.Abs(y - h * 0.5f) - h * 0.5f;
        float d = Mathf.Max(dx, dy);
        const float k = 0.70710678f;
        if (tl) d = Mathf.Max(d, (c - (x + (h - y))) * k);
        if (br) d = Mathf.Max(d, (c - ((w - x) + y)) * k);
        return d;
    }

    private static float RoundD(float x, float y, float w, float h, float r)
    {
        float px = Mathf.Abs(x - w * 0.5f) - (w * 0.5f - r);
        float py = Mathf.Abs(y - h * 0.5f) - (h * 0.5f - r);
        float ox = Mathf.Max(px, 0f), oy = Mathf.Max(py, 0f);
        return Mathf.Sqrt(ox * ox + oy * oy) + Mathf.Min(Mathf.Max(px, py), 0f) - r;
    }

    // 4x4 supersampled coverage of a polygon given as x0,y0,x1,y1,...
    private static float PolyCoverage(float x, float y, float[] poly)
    {
        int hit = 0;
        for (int i = 0; i < 4; i++)
        {
            for (int j = 0; j < 4; j++)
            {
                if (PointInPoly(x - 0.5f + (i + 0.5f) / 4f, y - 0.5f + (j + 0.5f) / 4f, poly)) hit++;
            }
        }
        return hit / 16f;
    }

    private static bool PointInPoly(float px, float py, float[] poly)
    {
        bool inside = false;
        int n = poly.Length / 2;
        for (int i = 0, j = n - 1; i < n; j = i++)
        {
            float xi = poly[i * 2], yi = poly[i * 2 + 1];
            float xj = poly[j * 2], yj = poly[j * 2 + 1];
            if ((yi > py) != (yj > py) && px < (xj - xi) * (py - yi) / (yj - yi) + xi)
                inside = !inside;
        }
        return inside;
    }
}
