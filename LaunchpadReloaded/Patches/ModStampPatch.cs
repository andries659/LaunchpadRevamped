using System.IO;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using UnityEngine;

namespace LaunchpadReloaded.Patches;

[HarmonyPatch(typeof(ModManager), nameof(ModManager.LateUpdate))]
public static class ModStampPatch
{
    private static Sprite? _stampSprite;
    private static bool _loggedMissing;

    private static Sprite? LoadStamp()
    {
        if (_stampSprite != null) return _stampSprite;

        var asm = Assembly.GetExecutingAssembly();
        var names = asm.GetManifestResourceNames();
        var resourceName = names.FirstOrDefault(n => n.EndsWith("ModStamp.png"));
        if (resourceName == null)
        {
            if (!_loggedMissing)
            {
                _loggedMissing = true;
                Error("ModStamp.png not embedded. Resources found: " + string.Join(", ", names));
            }
            return null;
        }

        using var stream = asm.GetManifestResourceStream(resourceName);
        if (stream == null) return null;

        using var ms = new MemoryStream();
        stream.CopyTo(ms);

        var tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
        ImageConversion.LoadImage(tex, ms.ToArray(), false);

        _stampSprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height),
            new Vector2(0.5f, 0.5f), 100f);
        _stampSprite.hideFlags = HideFlags.HideAndDontSave | HideFlags.DontUnloadUnusedAsset;
        return _stampSprite;
    }

    public static void Postfix(ModManager __instance)
    {
        __instance.ShowModStamp();

        var sprite = LoadStamp();
        if (sprite == null) return;

        if (__instance.ModStamp == null)
        {
            Error("ModStamp is null");
            return;
        }

        __instance.ModStamp.sprite = sprite;
    }
}
