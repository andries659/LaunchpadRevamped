using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using BepInEx;
using HarmonyLib;
using TMPro;
using UnityEngine;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace LaunchpadReloaded.Patches;

public static class MainMenuLayout
{
    private const string Left = "MainUI/AspectScaler/LeftPanel";
    private const string Btn = Left + "/Main Buttons/";
    private const string Right = "MainUI/AspectScaler/RightPanel";
    private const string Background = "MainUI/AspectScaler/BackgroundTexture";

    // Players drop their own images in <game folder>/LPR_DATA/Background
    private const string DataFolder = "LPR_DATA";
    private const string BackgroundFolder = "Background";
    private static readonly string[] ImageExtensions = { ".png", ".jpg", ".jpeg" };

    private const float Duration = 0.55f;
    private const float Stagger = 0.06f;

    // Arc shape: Play sits at the apex, every other button curves away from it.
    private const float ApexX = 2.9f;   // x of the button closest to the screen center
    private const float ApexY = -0.55f; // y of Play
    private const float Curve = 0.1f;   // higher = stronger bend

    // Mod list text (Reactor / BepInEx / Mods / ...): where the text starts,
    // as a fraction of the screen (0,0 = bottom-left, 1,1 = top-right).
    private static readonly Vector2 ModListViewport = new(0.40f, 0.80f);
    private const float ModListZ = -20f; // in front of the wall and the monitor

    private static readonly Vector2 FromRight = new(8f, 0f);
    private static readonly Vector2 FromLeft = new(-9f, 0f);
    private static readonly Vector2 FromTop = new(0f, 3f);
    private static readonly Vector2 FromBottom = new(0f, -3f);

    // path, y, scale (top to bottom)
    private static readonly (string Path, float Y, float Scale)[] Arc =
    {
        (Btn + "Inventory Button",   1.0f,   0.78f),
        (Btn + "ShopButton",         0.3f,   0.78f),
        (Btn + "PlayButton",         ApexY,  1.1f),
        (Btn + "NewsButton",         -1.35f, 0.7f),
        (Btn + "AcountButton",       -1.95f, 0.7f),
        (Btn + "SettingsButton",     -2.55f, 0.7f),
        (Btn + "BottomButtonBounds", -3.2f,  1f),
    };

    private static List<(string Path, float? X, float? Y, float? Scale, Vector2? Slide)> BuildLayout()
    {
        var list = new List<(string Path, float? X, float? Y, float? Scale, Vector2? Slide)>
        {
            // containers: centered so child coordinates are screen coordinates
            (Left, 0f, 0f, null, null),

            // logo, top-left
            (Left + "/Sizer", -3.4f, 2.05f, 1.1f, FromTop),

            // monitor with Local / Online
            (Right, -3.0f, -0.6f, 0.7f, FromLeft),

            // build number: bottom-left, under the monitor
            (Left + "/VersionShower", -5.3f, -3.35f, null, FromBottom),
        };

        foreach (var (path, y, scale) in Arc)
        {
            var dy = y - ApexY;
            list.Add((path, ApexX + Curve * dy * dy, y, scale, FromRight));
        }

        return list;
    }

    private sealed class Anim
    {
        public Transform T = null!;
        public Vector3 From;
        public Vector3 To;
        public float Delay;
    }

    private static readonly List<Anim> Anims = new();
    private static readonly List<(Transform T, Vector3 Pos)> Pins = new();
    private static Transform? _modList;
    private static Vector3 _modListPos;
    private static float _time;

    public static void Apply(Transform root)
    {
        Anims.Clear();
        Pins.Clear();
        _modList = null;
        _time = 0f;

        // hide the frame graphic on the left panel
        var left = root.Find(Left);
        if (left != null)
        {
            foreach (var r in left.GetComponents<Renderer>()) r.enabled = false;
        }

        var divider = root.Find(Btn + "Divider");
        if (divider != null) divider.gameObject.SetActive(false);

        DisableFloatingCrewmates();
        ApplyBackground(root);

        var disabled = 0;
        var order = 0;
        foreach (var (path, x, y, scale, slide) in BuildLayout())
        {
            var t = root.Find(path);
            if (t == null)
            {
                Warning($"Menu layout: '{path}' not found");
                continue;
            }

            // Stop the game re-anchoring anything we place.
            var aspect = t.GetComponent<AspectPosition>();
            if (aspect != null)
            {
                aspect.enabled = false;
                disabled++;
            }

            var p = t.localPosition;
            var target = new Vector3(x ?? p.x, y ?? p.y, p.z);

            if (scale.HasValue)
            {
                t.localScale = new Vector3(scale.Value, scale.Value, t.localScale.z);
            }

            Pins.Add((t, target));

            if (slide.HasValue)
            {
                var from = target + new Vector3(slide.Value.x, slide.Value.y, 0f);
                t.localPosition = from;
                Anims.Add(new Anim { T = t, From = from, To = target, Delay = order++ * Stagger });
            }
            else
            {
                t.localPosition = target;
            }
        }

        Info($"Menu layout: disabled {disabled} AspectPosition component(s)");

        // children of BottomButtonBounds: Credits and Quit, small, side by side
        SetLocal(root, Btn + "BottomButtonBounds/CreditsButton", -0.75f, 0f, 0.7f);
        SetLocal(root, Btn + "BottomButtonBounds/ExitGameButton", 0.75f, 0f, 0.7f);

        PlaceModList();
    }

    // The crewmates that float past the monitor's window. Starfield stays.
    private static void DisableFloatingCrewmates()
    {
        var particles = GameObject.Find("Ambience/PlayerParticles");
        if (particles != null)
        {
            particles.SetActive(false);
        }
        else
        {
            Warning("Menu layout: 'Ambience/PlayerParticles' not found");
        }
    }

    private static void PlaceModList()
    {
        var go = GameObject.Find("ReactorVersion");
        if (go == null)
        {
            Warning("Menu layout: 'ReactorVersion' not found");
            return;
        }

        var aspect = go.GetComponent<AspectPosition>();
        if (aspect != null) aspect.enabled = false;

        var tmp = go.GetComponentInChildren<TMP_Text>();
        if (tmp != null)
        {
            // The text may be clipped to the monitor's screen by a stencil test.
            // Make the font material's stencil comparison always pass.
            var mat = tmp.fontMaterial;
            if (mat != null && mat.HasProperty("_StencilComp"))
            {
                mat.SetFloat("_StencilComp", 8f); // CompareFunction.Always
            }
            else if (mat != null)
            {
                Warning($"Menu layout: shader '{mat.shader.name}' has no _StencilComp");
            }
        }
        else
        {
            Warning("Menu layout: no TMP_Text on 'ReactorVersion'");
        }

        var cam = Camera.main;
        if (cam == null)
        {
            Warning("Menu layout: no main camera for the mod list");
            return;
        }

        var w = cam.ViewportToWorldPoint(new Vector3(ModListViewport.x, ModListViewport.y, 10f));
        _modListPos = new Vector3(w.x, w.y, ModListZ);
        _modList = go.transform;
        _modList.position = _modListPos;
    }

    private static void SetLocal(Transform root, string path, float x, float y, float scale)
    {
        var t = root.Find(path);
        if (t == null)
        {
            Warning($"Menu layout: '{path}' not found");
            return;
        }

        t.localPosition = new Vector3(x, y, t.localPosition.z);
        t.localScale = new Vector3(scale, scale, t.localScale.z);
    }

    // ---------------------------------------------------------------- background

    // Loaded sprites, keyed by file path (or "embedded"), so we never decode twice.
    private static readonly Dictionary<string, Sprite> BgCache = new();

    // <game folder>/LPR_DATA/Background
    public static string BackgroundDir => Path.Combine(Paths.GameRootPath, DataFolder, BackgroundFolder);

    private static void ApplyBackground(Transform root)
    {
        var bg = root.Find(Background);
        var sr = bg != null ? bg.GetComponent<SpriteRenderer>() : null;
        if (sr == null)
        {
            Warning($"Menu layout: no SpriteRenderer at '{Background}'");
            return;
        }

        // 1) a player's own image, 2) the one embedded in the mod, 3) the vanilla wall
        var sprite = LoadUserBackground() ?? LoadEmbeddedBackground();
        if (sprite == null) return;

        sr.drawMode = SpriteDrawMode.Simple; // the vanilla one may be tiled/sliced
        sr.sprite = sprite;
        bg!.localScale = Vector3.one;

        // Optional: the game's dark overlay sits on top of the background.
        // Uncomment if the art looks dim.
        // var tint = root.Find("MainUI/Tint");
        // if (tint != null) tint.gameObject.SetActive(false);
    }

    // Looks in LPR_DATA/Background (created on first run so players can find it).
    // A file named "MenuBackground" always wins; otherwise one is picked at random
    // each time the menu loads, so several images work as a rotation.
    private static Sprite? LoadUserBackground()
    {
        string[] files;
        try
        {
            var dir = BackgroundDir;
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
                Info($"Menu layout: created '{dir}', drop .png/.jpg images in there for custom backgrounds");
                return null;
            }

            files = Directory.GetFiles(dir)
                .Where(f => ImageExtensions.Contains(Path.GetExtension(f).ToLowerInvariant()))
                .OrderBy(f => f, StringComparer.OrdinalIgnoreCase)
                .ToArray();
        }
        catch (Exception e)
        {
            Warning($"Menu layout: could not read background folder: {e.Message}");
            return null;
        }

        if (files.Length == 0) return null;

        var pinned = files.FirstOrDefault(f =>
            Path.GetFileNameWithoutExtension(f).Equals("MenuBackground", StringComparison.OrdinalIgnoreCase));
        var pick = pinned ?? files[Random.Range(0, files.Length)];

        if (BgCache.TryGetValue(pick, out var cached) && cached != null) return cached;

        try
        {
            var sprite = CreateSprite(File.ReadAllBytes(pick));
            if (sprite == null)
            {
                Warning($"Menu layout: '{Path.GetFileName(pick)}' is not a valid image");
                return null;
            }

            BgCache[pick] = sprite;
            Info($"Menu layout: using custom background '{Path.GetFileName(pick)}'");
            return sprite;
        }
        catch (Exception e)
        {
            Warning($"Menu layout: failed to load '{Path.GetFileName(pick)}': {e.Message}");
            return null;
        }
    }

    // Optional default: embed Resources/MenuBackground.png in the mod.
    private static Sprite? LoadEmbeddedBackground()
    {
        if (BgCache.TryGetValue("embedded", out var cached) && cached != null) return cached;

        var asm = Assembly.GetExecutingAssembly();
        var name = asm.GetManifestResourceNames().FirstOrDefault(n => n.EndsWith("MenuBackground.png"));
        if (name == null) return null; // not embedded: keep vanilla background

        using var stream = asm.GetManifestResourceStream(name);
        if (stream == null) return null;
        using var ms = new MemoryStream();
        stream.CopyTo(ms);

        var sprite = CreateSprite(ms.ToArray());
        if (sprite != null) BgCache["embedded"] = sprite;
        return sprite;
    }

    // Builds a sprite that COVERS a 16:9 screen: nothing is stretched, and an image with
    // a different aspect ratio just gets cropped on the long side.
    private static Sprite? CreateSprite(byte[] data)
    {
        var tex = new Texture2D(2, 2, TextureFormat.RGBA32, false)
        {
            filterMode = FilterMode.Bilinear,
            wrapMode = TextureWrapMode.Clamp
        };
        tex.hideFlags = HideFlags.HideAndDontSave | HideFlags.DontUnloadUnusedAsset;

        if (!ImageConversion.LoadImage(tex, data, false))
        {
            Object.Destroy(tex);
            return null;
        }

        // 12.7 local units is roughly the full screen width at 16:9 (height = 12.7 * 9 / 16)
        const float width = 12.7f;
        const float height = width * 9f / 16f;
        var ppu = Mathf.Min(tex.width / width, tex.height / height);

        var sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), ppu);
        sprite.hideFlags = HideFlags.HideAndDontSave | HideFlags.DontUnloadUnusedAsset;
        return sprite;
    }

    // ---------------------------------------------------------------- per-frame

    public static void Tick()
    {
        if (Pins.Count == 0 && Anims.Count == 0) return;

        // The menu is gone (scene changed): stop touching anything.
        if (Pins.Count > 0 && Pins[0].T == null)
        {
            Pins.Clear();
            Anims.Clear();
            _modList = null;
            return;
        }

        if (Anims.Count > 0)
        {
            _time += Time.deltaTime;
            var done = true;

            foreach (var a in Anims)
            {
                if (a.T == null)
                {
                    Anims.Clear();
                    return;
                }

                var k = Mathf.Clamp01((_time - a.Delay) / Duration);
                var eased = 1f - Mathf.Pow(1f - k, 3f);
                a.T.localPosition = Vector3.Lerp(a.From, a.To, eased);

                if (k < 1f) done = false;
            }

            if (done) Anims.Clear();
        }

        // Keep everything where we put it, in case something else moves it.
        foreach (var (t, pos) in Pins)
        {
            if (t == null || Anims.Exists(a => a.T == t)) continue;
            if ((t.localPosition - pos).sqrMagnitude > 0.0001f)
            {
                t.localPosition = pos;
            }
        }

        if (_modList != null && (_modList.position - _modListPos).sqrMagnitude > 0.0001f)
        {
            _modList.position = _modListPos;
        }
    }
}

[HarmonyPatch(typeof(MainMenuManager), nameof(MainMenuManager.Start))]
public static class MainMenuLayoutPatch
{
    public static void Postfix(MainMenuManager __instance)
    {
        MainMenuLayout.Apply(__instance.transform);
    }
}

// ModManager.LateUpdate already runs every frame, so it doubles as the animation tick.
[HarmonyPatch(typeof(ModManager), nameof(ModManager.LateUpdate))]
public static class MainMenuAnimTickPatch
{
    public static void Postfix()
    {
        MainMenuLayout.Tick();
    }
}
